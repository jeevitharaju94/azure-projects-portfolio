# Order API

A .NET 10 ASP.NET Core Web API for managing customers, products, orders, and payments. The API uses Entity Framework Core with Azure SQL Database and can be deployed to a Windows Azure App Service Web App using the included Bicep templates and Azure DevOps pipeline.

## Project structure

```text
dotnet-api-sqlserver-efcore/
├── infrastructure/
│   ├── main.bicep
│   ├── modules/
│   │   ├── sqldb.bicep
│   │   ├── sqlFirewallRules.bicep
│   │   └── webApp.bicep
│   └── parameters/
│       ├── main.DEV.bicepparam
│       └── main.TEST.bicepparam
├── pipelines/
│   ├── azure-pipeline.yaml
│   ├── deploy-iac-pipeline.yaml
│   ├── deploy-webapp-pipeline.yaml
│   └── shared-variables/
├── src/
│   └── OrderApi/
│       ├── Controllers/
│       ├── DTOs/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       ├── DbContext.cs
│       ├── OrderApi.csproj
│       └── Program.cs
└── tests/
    └── OrderApi.Tests/
```

`sqlFirewallRules.bicep` is retained as a standalone module, but `main.bicep` does not reference or deploy it.

## API

All routes are prefixed with `/api`. Swagger UI is enabled in the Development environment at `/swagger`.

| Resource | Routes |
|---|---|
| Customers | `GET /api/customers`, `GET /api/customers/{id}`, `POST /api/customers`, `PUT /api/customers/{id}`, `DELETE /api/customers/{id}` |
| Products | `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}` |
| Orders | `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/orders`, `PATCH /api/orders/{id}/status`, `DELETE /api/orders/{id}` |
| Payments | `GET /api/payments`, `GET /api/payments/{id}`, `POST /api/payments`, `PATCH /api/payments/{id}/status`, `DELETE /api/payments/{id}` |

The API uses DTOs for request and response data. Entity Framework Core models customers, products, orders, order items, and payments; database migrations are in `src/OrderApi/Migrations`.

## Prerequisites

- .NET 10 SDK.
- SQL Server for local development.
- Azure CLI with Bicep support for manual infrastructure deployments.
- An Azure subscription and permission to deploy the App Service and Azure SQL resources.
- Azure DevOps service connections with the required resource deployment permissions if using the pipeline.

## Run locally

Set the connection string for your local SQL Server, then start the API:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=OrderDb;User Id=sa;Password=<local-password>;TrustServerCertificate=True;"
dotnet run --project .\src\OrderApi\OrderApi.csproj
```

Do not commit local credentials. The connection string can be provided through the `ConnectionStrings__DefaultConnection` environment variable rather than changing the checked-in settings.

To apply the existing EF Core migrations, install the EF Core CLI tool if needed and run:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef database update --project .\src\OrderApi\OrderApi.csproj
```

Run the unit tests from the project root:

```powershell
dotnet test .\tests\OrderApi.Tests\OrderApi.Tests.csproj
```

## Azure infrastructure

The Bicep templates provision:

- A Windows App Service plan (Basic B1) and a Windows Web App configured for .NET 10.
- An Azure SQL logical server and Basic database.
- The Web App's SQL connection string as an app setting, supplied through a secure parameter.
- HTTPS-only access and disabled FTP/FTPS.

No SQL firewall rules are deployed. Configure SQL networking separately if the Web App must connect to the database; without an allowed network path, database requests from the deployed API will fail.

Update the environment-specific `.bicepparam` files and matching `pipelines/shared-variables/env-DEV.yaml` or `env-TEST.yaml` values before deploying. Web App names must be globally unique, and the name in each parameter file must match `webAppName` in the corresponding pipeline variable file.

The Bicep parameter files read `SQL_ADMIN_PASSWORD` from the process environment. The Azure DevOps pipeline maps the secret variable `sqlAdministratorLoginPassword` to this environment variable. Define that variable as a secret in the pipeline; do not put the password in source control.

## Azure DevOps pipeline

The pipeline definition is `pipelines/azure-pipeline.yaml`. Configure its environment files with the resource group, Web App name, parameter file, and valid Azure service connection names. The checked-in service connection values are examples and may need to be replaced with the connections available in your Azure DevOps project.

Queue the pipeline with an environment and one of these actions:

- `none`: validate, preview, test, and package without deploying infrastructure or the API. Validation creates the resource group if it does not exist.
- `iac`: deploy the Bicep infrastructure.
- `webapp`: deploy the API package to an already provisioned Web App.
- `all`: deploy infrastructure, then deploy the Web App package.

The pipeline builds with the .NET 10 SDK, runs `OrderApi.Tests`, publishes the API, and deploys the resulting package to the Windows Web App.

## Deploy manually

From this project directory, sign in and select the subscription and resource group for the deployment. Set `SQL_ADMIN_PASSWORD` securely in the shell environment before running the deployment; the parameter file reads it during Bicep compilation.

```powershell
az login
az account set --subscription "<subscription-id>"
az group create --name "<resource-group>" --location "westeurope"
az deployment group validate --resource-group "<resource-group>" --name "order-api" --template-file ".\infrastructure\main.bicep" --parameters ".\infrastructure\parameters\main.DEV.bicepparam"
az deployment group create --resource-group "<resource-group>" --name "order-api" --template-file ".\infrastructure\main.bicep" --parameters ".\infrastructure\parameters\main.DEV.bicepparam"
```
