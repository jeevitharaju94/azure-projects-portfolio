
## 1. Enterprise API Integration

**Azure API Management + .NET + Azure Functions + Service Bus**

A reference enterprise integration demonstrating API gateway, authentication, asynchronous messaging, error handling, monitoring, and secure Azure-to-Azure communication.

### Technologies

* Azure API Management
* Azure Functions
* C# / .NET
* Azure Service Bus
* Azure Key Vault
* Managed Identity
* Application Insights
* Bicep
* Azure DevOps

### Architecture

```text
Client
  │
  ▼
Azure API Management
  │
  ├── JWT Validation
  ├── Rate Limiting
  ├── Request Validation
  └── Correlation ID
  │
  ▼
.NET Azure Function
  │
  ▼
Azure Service Bus
  │
  ▼
Downstream API
```

### Key Features

* OAuth 2.0 / JWT authentication
* API validation
* Rate limiting
* Managed Identity
* Asynchronous messaging
* Retry handling
* Dead-letter queue
* Correlation ID
* Centralized logging
* Application Insights monitoring
* Infrastructure as Code
* Automated CI/CD

