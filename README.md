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


---

## Infrastructure as Code & CI/CD

**[azure-bicep-library](https://github.com/jeevitharaju94/azure-bicep-library)** is a reusable, modular Bicep library with an Azure DevOps pipeline (lint, validate, what-if, build, deploy) that supports the projects above.

- Modules for APIM, Logic Apps Standard, Service Bus, Key Vault, Storage, networking and monitoring
- Environment-specific `.bicepparam` files (DEV / TEST / PROD)
- Security defaults: RBAC-enabled Key Vault, Service Bus with local auth disabled, diagnostic settings to Log Analytics

---

## Connect

- LinkedIn: [Jeevitha Bommanahalli Raju](https://www.linkedin.com/in/jeevitharaju/)