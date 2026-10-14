using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public sealed class MonthlyCostReportFunction(ILogger<MonthlyCostReportFunction> logger)
{
    private const string CostManagementApiVersion = "2023-11-01";
    private const string CostManagementScope = "https://management.azure.com/.default";
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function(nameof(MonthlyCostReportFunction))]
    public async Task Run(
        [TimerTrigger("%COST_REPORT_SCHEDULE%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting the monthly cost reporting timer invocation.");

        var subscriptions = LoadSubscriptions(
            Environment.GetEnvironmentVariable("SUBSCRIPTIONS_JSON"));
        var storageAccountName = RequiredSetting("TABLE_STORAGE_ACCOUNT");
        var tableName = Environment.GetEnvironmentVariable("COST_TABLE_NAME") ?? "MonthlyCosts";
        var (periodStart, periodEnd) = PreviousMonthWindow(DateTimeOffset.UtcNow);

        using var credential = new DefaultAzureCredential();
        var tableClient = new TableClient(
            new Uri($"https://{storageAccountName}.table.core.windows.net"),
            tableName,
            credential);

        await tableClient.CreateIfNotExistsAsync(cancellationToken);

        foreach (var subscription in subscriptions)
        {
            var accessToken = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext([CostManagementScope]),
                cancellationToken);
            var (amount, currency) = await QuerySubscriptionCostAsync(
                subscription.SubscriptionId,
                accessToken.Token,
                periodStart,
                periodEnd,
                cancellationToken);

            var entity = new TableEntity(periodStart.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                subscription.SubscriptionId)
            {
                ["SubscriptionId"] = subscription.SubscriptionId,
                ["SubscriptionName"] = subscription.DisplayName,
                ["PeriodStart"] = periodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["PeriodEnd"] = periodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["Amount"] = (double)amount,
                ["Currency"] = currency,
                ["RetrievedAtUtc"] = DateTimeOffset.UtcNow
            };

            await tableClient.UpsertEntityAsync(
                entity,
                TableUpdateMode.Replace,
                cancellationToken);

            logger.LogInformation(
                "Stored {Amount} {Currency} for subscription {SubscriptionId} ({SubscriptionName}), period {PeriodStart} to {PeriodEnd}.",
                amount,
                currency,
                subscription.SubscriptionId,
                subscription.DisplayName,
                periodStart,
                periodEnd);
        }
    }

    internal static (DateOnly Start, DateOnly End) PreviousMonthWindow(DateTimeOffset now)
    {
        var firstOfCurrentMonth = new DateOnly(now.Year, now.Month, 1);
        var end = firstOfCurrentMonth.AddDays(-1);
        return (new DateOnly(end.Year, end.Month, 1), end);
    }

    internal static (decimal Amount, string Currency) ParseQueryResult(JsonElement properties)
    {
        if (!properties.TryGetProperty("columns", out var columns)
            || columns.ValueKind != JsonValueKind.Array
            || !properties.TryGetProperty("rows", out var rows)
            || rows.ValueKind != JsonValueKind.Array
            || rows.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Cost Management returned no cost rows or an invalid result.");
        }

        var columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var columnIndex = 0;
        foreach (var column in columns.EnumerateArray())
        {
            if (column.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String
                && name.GetString() is { } columnName)
            {
                columnIndexes[columnName] = columnIndex;
            }

            columnIndex++;
        }

        if (!columnIndexes.TryGetValue("PreTaxCost", out var costIndex)
            || !columnIndexes.TryGetValue("Currency", out var currencyIndex))
        {
            throw new InvalidOperationException(
                "Cost Management response is missing PreTaxCost or Currency.");
        }

        decimal total = 0;
        var currencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array
                || row.GetArrayLength() <= Math.Max(costIndex, currencyIndex))
            {
                throw new InvalidOperationException(
                    "Cost Management returned a row with an unexpected shape.");
            }

            var cost = row[costIndex];
            decimal amount;
            if (cost.ValueKind == JsonValueKind.Number && cost.TryGetDecimal(out var numericAmount))
            {
                amount = numericAmount;
            }
            else if (cost.ValueKind == JsonValueKind.String
                && decimal.TryParse(
                    cost.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var stringAmount))
            {
                amount = stringAmount;
            }
            else
            {
                throw new InvalidOperationException(
                    "Cost Management returned a non-numeric cost.");
            }

            var currencyValue = row[currencyIndex];
            if (currencyValue.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(currencyValue.GetString()))
            {
                throw new InvalidOperationException(
                    "Cost Management returned an invalid currency.");
            }

            total += amount;
            currencies.Add(currencyValue.GetString()!);
        }

        if (currencies.Count != 1)
        {
            throw new InvalidOperationException(
                "Cost Management returned costs in multiple currencies.");
        }

        return (total, currencies.Single());
    }

    private static async Task<(decimal Amount, string Currency)> QuerySubscriptionCostAsync(
        string subscriptionId,
        string token,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            type = "ActualCost",
            timeframe = "Custom",
            timePeriod = new
            {
                from = $"{periodStart:yyyy-MM-dd}T00:00:00Z",
                to = $"{periodEnd:yyyy-MM-dd}T23:59:59Z"
            },
            dataset = new
            {
                granularity = "None",
                aggregation = new
                {
                    totalCost = new { name = "PreTaxCost", function = "Sum" }
                }
            }
        };

        var endpoint =
            $"https://management.azure.com/subscriptions/{subscriptionId}" +
            $"/providers/Microsoft.CostManagement/query?api-version={CostManagementApiVersion}";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var result = await JsonDocument.ParseAsync(
            responseStream,
            cancellationToken: cancellationToken);

        if (!result.RootElement.TryGetProperty("properties", out var properties))
        {
            throw new InvalidOperationException(
                $"Cost Management returned no query properties for subscription {subscriptionId}.");
        }

        return ParseQueryResult(properties);
    }

    private static List<SubscriptionConfiguration> LoadSubscriptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("SUBSCRIPTIONS_JSON is required.");
        }

        var subscriptions = JsonSerializer.Deserialize<List<SubscriptionConfiguration>>(
            json,
            JsonOptions);
        if (subscriptions is null || subscriptions.Count == 0)
        {
            throw new InvalidOperationException(
                "SUBSCRIPTIONS_JSON must contain a non-empty JSON array.");
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var subscription in subscriptions)
        {
            if (subscription is null)
            {
                throw new InvalidOperationException(
                    "Each configured subscription must be a JSON object.");
            }

            if (!Guid.TryParse(subscription.SubscriptionId, out var subscriptionId))
            {
                throw new InvalidOperationException(
                    $"Subscription ID is not a valid GUID: {subscription.SubscriptionId}");
            }

            subscription.SubscriptionId = subscriptionId.ToString();
            if (string.IsNullOrWhiteSpace(subscription.DisplayName))
            {
                subscription.DisplayName = subscription.SubscriptionId;
            }
            else
            {
                subscription.DisplayName = subscription.DisplayName.Trim();
            }

            if (!seenIds.Add(subscription.SubscriptionId))
            {
                throw new InvalidOperationException(
                    $"Duplicate subscription configured: {subscription.SubscriptionId}");
            }
        }

        return subscriptions;
    }

    private static string RequiredSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required.")
            : value;
    }

    private sealed class SubscriptionConfiguration
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }
}
