# Azure Cost Reporting

Collect each configured Azure subscription's actual cost for the previous calendar month and store a monthly summary in Azure Table Storage. The project includes a .NET 8 isolated Azure Function and reusable Bicep infrastructure that you can configure and deploy in your own Azure environment.

## Project structure

```text
azure-cost-reporting/
├── infrastructure/
│   ├── main.bicep
│   ├── modules/
│   │   ├── costMgmtRoleAssignment.bicep
│   │   ├── functionApp.bicep
│   │   └── storageAccount.bicep
│   └── parameters/
│       ├── main.DEV.bicepparam
│       ├── main.TEST.bicepparam
│       └── main.example.bicepparam
├── pipelines/
│   ├── azure-pipeline.yaml
│   ├── deploy-functionapp-pipeline.yaml
│   ├── deploy-iac-pipeline.yaml
│   └── shared-variables/
│       ├── build-variables.yaml
│       ├── env-DEV.yaml
│       └── env-TEST.yaml
├── src/
│   └── functionApp/
│       ├── AzureCostReporting.csproj
│       ├── AssemblyInfo.cs
│       ├── MonthlyCostReportFunction.cs
│       ├── Program.cs
│       └── host.json
└── tests/
    └── AzureCostReporting.Tests/
        ├── AzureCostReporting.Tests.csproj
        └── CostReportingTests.cs
```

## How it works

- A .NET 8 isolated Azure Function runs on a configurable monthly timer schedule. The default schedule is 00:15 UTC on each of the first five days of the month.
- The Function's system-assigned managed identity queries Azure Cost Management for each configured subscription.
- Each subscription's `ActualCost` query sums the `PreTaxCost` metric for the previous calendar month.
- One Azure Table Storage entity is upserted per subscription and month. The month (`YYYY-MM`) is the partition key and the subscription ID is the row key, so retries replace that month's entity instead of duplicating it.
- The Bicep templates create the Storage account and table, Function App and hosting plan, Function table-data role assignment, and a Cost Management Reader assignment in the subscription where the main template is deployed.

Cost Management data may arrive late and can be revised. The schedule retries the query during the first five days, refreshing the same monthly entities. These results are point-in-time cost reports, not final invoices.

## Table schema

| Property | Description |
|---|---|
| `PartitionKey` | Reporting month in `YYYY-MM` format |
| `RowKey` | Azure subscription ID |
| `SubscriptionId` | Azure subscription ID |
| `SubscriptionName` | Configured display name |
| `PeriodStart`, `PeriodEnd` | Inclusive previous-month dates |
| `Amount` | Sum of `PreTaxCost` in the returned currency |
| `Currency` | Currency returned by Cost Management |
| `RetrievedAtUtc` | Time the result was stored |

## Prerequisites

- An Azure subscription and a resource group for the app.
- Azure CLI with Bicep support, Azure Functions Core Tools v4, and the .NET 8 SDK.
- Permission to deploy the resources and create role assignments in the app resource group.
- Permission to grant **Cost Management Reader** on each monitored subscription (for example, Owner or Role Based Access Control Administrator).
- Cost Management query access to each monitored subscription for the Function's managed identity.

## Configure

Update the environment settings in `infrastructure/parameters/main.example.bicepparam`. For pipeline deployments, customize `main.DEV.bicepparam` and `main.TEST.bicepparam` as applicable:

```bicep
using '../main.bicep'

param location = 'westeurope'
param functionAppName = 'func-your-unique-name'
param storageAccountName = 'youruniqueaccount123'
param subscriptions = [
  {
    subscriptionId: '00000000-0000-0000-0000-000000000000'
    displayName: 'Production'
  }
  {
    subscriptionId: '11111111-1111-1111-1111-111111111111'
    displayName: 'Development'
  }
]
param costReportSchedule = '0 15 0 1-5 * *'
param costTableName = 'MonthlyCosts'
```

Use globally unique names for the Function App and Storage account. The Storage account name must contain 3-24 lowercase letters or digits. Azure Functions evaluates the six-field NCRONTAB schedule in UTC. The configured subscription list identifies which subscriptions are queried; the deployment identity needs Cost Management access to all of them.

## Deploy manually to your Azure environment

Run these commands from the project directory. Choose a deployment subscription that is included in the `subscriptions` parameter; the main template grants Cost Management Reader to the Function identity in the subscription containing the deployment resource group. Replace the example parameter file with your configured parameters file before deploying.

```powershell
az login
az account set --subscription "<deployment-subscription-id>"
az group create --name "<resource-group>" --location "westeurope"
az deployment group validate --resource-group "<resource-group>" --name "cost-reporting" --template-file "infrastructure/main.bicep" --parameters "infrastructure/parameters/main.example.bicepparam"
az deployment group create --resource-group "<resource-group>" --name "cost-reporting" --template-file "infrastructure/main.bicep" --parameters "infrastructure/parameters/main.example.bicepparam"
```

The main template creates the Function identity and assigns it **Storage Table Data Contributor** on the configured Storage account. It also assigns **Cost Management Reader** on the deployment subscription. For every additional monitored subscription, grant the Function identity the same Cost Management role at subscription scope:

```powershell
$principalId = az deployment group show --resource-group "<resource-group>" --name "cost-reporting" --query "properties.outputs.functionAppPrincipalId.value" --output tsv
az role assignment create --assignee-object-id $principalId --assignee-principal-type ServicePrincipal --role "Cost Management Reader" --scope "/subscriptions/<monitored-subscription-id>"
```

Run the role-assignment command for each additional subscription, while signed in as an operator authorized to create role assignments there. Allow a few minutes for RBAC changes to propagate.

Publish the Function from its project directory:

```powershell
Set-Location "src\functionApp"
func azure functionapp publish "<function-app-name>"
```

## Azure DevOps CI/CD

The `pipelines` folder contains an Azure DevOps pipeline modeled on the structure used by the Azure Bicep library. It builds and validates the Bicep template, runs deployment validation and what-if, restores and tests the .NET solution, and packages both the infrastructure and Function App as pipeline artifacts.

1. Create Azure Resource Manager service connections for DEV and TEST. The deployment identity needs permissions to create the resource group resources and role assignments in its deployment subscription.
2. Edit `pipelines/shared-variables/env-DEV.yaml` and `env-TEST.yaml` with your service connection names, resource group names, and Function App names. The Function App name must match the one in the corresponding environment `.bicepparam` file.
3. Replace the placeholder subscription IDs, globally unique resource names, and other settings in `infrastructure/parameters/main.DEV.bicepparam` and `main.TEST.bicepparam`.
4. In Azure DevOps, create a pipeline pointing to `projects/azure-cost-reporting/pipelines/azure-pipeline.yaml`.
5. Queue a run and choose the target environment and action:
   - `none`: lint, validate, preview, test, and package without deploying application infrastructure. Validation creates the target resource group if it does not exist.
   - `iac`: deploy the Bicep infrastructure.
   - `functionapp`: publish the Function package to an already deployed Function App.
   - `all`: deploy the infrastructure, then publish the Function package.

The Bicep template grants Cost Management Reader on the deployment subscription. If the configured subscription list includes other subscriptions, grant the Function's managed identity Cost Management Reader on each of them as described in [Deploy manually](#deploy-manually-to-your-azure-environment). The environment variable files and parameter files are checked-in configuration examples; replace their sample values before running deployments.

## Local development and tests

From the project root, restore and run the unit tests:

```powershell
dotnet restore .\src\functionApp\AzureCostReporting.csproj
dotnet test .\tests\AzureCostReporting.Tests\AzureCostReporting.Tests.csproj
```

To run the Function locally, configure a `local.settings.json` in `src/functionApp` with `AzureWebJobsStorage`, `TABLE_STORAGE_ACCOUNT`, `SUBSCRIPTIONS_JSON`, and `COST_REPORT_SCHEDULE`, then run `func start` from that directory. Sign in with `az login` and grant the signed-in identity Table Storage data access and Cost Management Reader access to the test subscriptions. Do not commit local settings or credentials.

## Customize

| Setting | Parameter or app setting | Default |
|---|---|---|
| Monitored subscriptions and display names | `subscriptions` | Example subscription |
| Collection time/frequency | `costReportSchedule` | `0 15 0 1-5 * *` |
| Azure Table Storage table | `costTableName` | `MonthlyCosts` |
| Azure region | `location` | Resource group location |
| Function App name | `functionAppName` | Required |
| Storage account name | `storageAccountName` | Required |
