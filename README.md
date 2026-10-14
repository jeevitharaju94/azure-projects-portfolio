# Azure Integration & Architecture Portfolio

### Azure Integration Engineer | C# / .NET | Microsoft 365 | DevOps

Hands-on projects showing how I design, secure, deploy and monitor enterprise integration solutions on Microsoft Azure, with Infrastructure as Code and CI/CD.

---

## About me

I'm an Azure Integration and .NET developer building enterprise integrations with Azure and Microsoft 365. I focus on solutions that are secure, observable and repeatable to deploy.

## Skills

**Hands-on (shown in this portfolio)**
- Azure API Management, Logic Apps Standard, Service Bus, Azure Functions
- Microsoft Entra ID, Managed Identity, Azure Key Vault
- Microsoft 365 / Microsoft Graph, Dynamics 365
- Bicep (Infrastructure as Code), Azure DevOps Pipelines
- Log Analytics, Azure Monitor, KQL
- C# / .NET

**Also experienced with**
- Azure Data Factory, ADLS Gen2, Azure SQL, Application Insights, SPFx, ReactJS, PowerShell.

---

## Featured projects

| Project | Problem and solution | Links |
|---|---|---|
| **Enterprise Integration with Dynamics 365 CRM** | Give external suppliers access to approved CRM functionality without exposing the CRM. Request-response via API Management and Logic Apps, plus event-driven messaging via Service Bus. | [Case study](./projects/project-integration-with-crm/project-info.md) · [Architecture](./projects/project-integration-with-crm/project-md365.drawio.png) |
| **Microsoft 365 Data Governance & Group Lifecycle** | Storage insights across SharePoint, Teams and M365 Groups, plus owner-driven renewal or deletion of inactive groups. Azure Functions, Logic Apps, Synapse, SQL and Key Vault. | [Case study](./projects/M365-data-governance/project-info.md) · [Data governance](./projects/M365-data-governance/architecture.svg) · [Lifecycle](./projects/M365-data-governance/lifecycle-management.svg) |
| **Azure Cost Reporting** | A .NET Azure Function collects the previous month's costs across configurable subscriptions and stores monthly summaries in Azure Table Storage. Includes reusable Bicep infrastructure and step-by-step instructions to deploy the solution into your own Azure environment. | [Project, customization and deployment guide](./projects/azure-cost-reporting/README.md) |

### Deploy Azure Cost Reporting in your own environment

The cost reporting project is designed to be reusable, not just a portfolio demo. Follow its [deployment guide](./projects/azure-cost-reporting/README.md) to provision the Function App and Azure Table Storage with Bicep in your own Azure environment. Configure your subscription IDs, schedule, and resource names, then grant the Function's managed identity access to the subscriptions you want to report on.

---

## Infrastructure as Code & CI/CD

**[azure-bicep-library](https://github.com/jeevitharaju94/azure-bicep-library)** provides reusable Bicep modules and Azure DevOps CI/CD pipelines for repeatable infrastructure delivery. The pipeline lints and validates templates, runs an Azure what-if preview, packages the infrastructure as a build artifact, and can deploy it to the selected DEV or TEST environment through an Azure service connection.

- Modules for APIM, Logic Apps Standard, Service Bus, Key Vault, Storage, networking and monitoring
- Environment-specific `.bicepparam` files (DEV / TEST / PROD)
- Deployment is an explicit pipeline action, so teams can review validation and what-if results before provisioning resources in their Azure environment
- Security defaults: RBAC-enabled Key Vault, Service Bus with local auth disabled, diagnostic settings to Log Analytics


---

## Connect

- LinkedIn: [Jeevitha Bommanahalli Raju](https://www.linkedin.com/in/jeevitharaju/)