# Integrating Dynamics 365 CRM with external suppliers

    - The main objective of this project was to expose the CRM capabilities to external supplier with out giving direct external access on CRM.
    - We designed API-led integration by using API management as API gateway. Used logic app for orchestration , used service bus for asynchronous communication.

### Technologies

* Azure API Management
* Azure Logic App standard
* Azure Service Bus
* Azure Key Vault
* Managed Identity
* Application Insights
* Bicep
* Azure DevOps

### Synchronous- Architecture

```

┌───────────────┐
│    Client     │
│               │
│ REST Request  │
└───────┬───────┘
        │
        ▼
┌─────────────────────────────┐
│   Azure API Management      │
│                             │
│  ✓ JWT Validation           │
│  ✓ Request Validation       │
│  ✓ Correlation ID           │
│  ✓ Rate Limiting            │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│       Azure Logic App       │
│                             │
│  Workflow Orchestration     │
│  Data Transformation        │
│  Error Handling             │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│       Dynamics 365 CRM      │
│                             │
│  Dataverse / CRM API        │
│  Create / Update / Retrieve │
└─────────────────────────────┘
```

### ASynchronous- Architecture

```

┌──────────────────────────┐
│   Dynamic CRM (Updates)  │
└────────────┬─────────────┘
             │
             ▼
┌─────────────────────────┐
│     Azure Logic App     │
└───────────┬─────────────┘
            │
            │
            ▼
┌──────────────────────────┐
│ Azure Service Bus Topic  │
└────────────┬─────────────┘
             │
       ┌─────┴─────┐
       ▼           ▼
   Consumer A   Consumer B
       │           │
       ▼           ▼
 Downstream    Notification
   System        System

```

### Key Features

* OAuth 2.0 / JWT authentication
* API validation
* Managed Identity
* Asynchronous messaging
* Retry handling
* Dead-letter queue
* Correlation ID
* Centralized logging
* Application Insights monitoring
* Infrastructure as Code
* Automated CI/CD