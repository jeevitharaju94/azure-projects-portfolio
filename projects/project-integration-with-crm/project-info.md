# Objective

Enable external suppliers to access approved Dynamics 365 CRM functionality without exposing the CRM directly. The solution supports both synchronous request-response interactions and asynchronous, event-driven communication.

![CRM integration architecture](./project-md365.drawio.png)

## Architecture flow

### Request-response

1. Suppliers authenticate with Microsoft Entra ID and send requests through Azure API Management.
2. API Management routes approved requests to Azure Logic Apps Standard.
3. Logic Apps Standard processes the request and integrates with Dynamics 365 CRM.
4. Operational telemetry is sent to the Log Analytics workspace.

### Event-driven

1. Dynamics 365 CRM events trigger Azure Logic Apps Standard.
2. Logic Apps processes the events and publishes messages to Azure Service Bus topics.
3. Suppliers authenticate with Microsoft Entra ID and consume the messages through the integration.
4. Operational telemetry is sent to the Log Analytics workspace.

# Technologies used

- Azure API Management
- Azure Logic Apps Standard
- Dynamics 365 CRM
- Azure Service Bus topics
- Microsoft Entra ID
- Log Analytics workspace

# DevOps

## Infrastructure

[Bicep Modules for Infrastructure as Code](https://github.com/jeevitharaju94/azure-bicep-library/tree/main/infrastructure)

## Pipelines

[Azure DevOps CI/CD pipelines](https://github.com/jeevitharaju94/azure-bicep-library/tree/main/pipelines)

# Outcome

- External supplier access without direct exposure of Dynamics 365 CRM
- Support for request-response and event-driven integration patterns
- Near real-time event processing
- Scalable architecture with improved resilience
- Operational visibility through centralized logging