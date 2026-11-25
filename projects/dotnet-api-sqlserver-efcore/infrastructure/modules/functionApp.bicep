
@description('Azure region for the function app and storage account.')
param location string = resourceGroup().location

@description('Globally unique Function App name.')
param functionAppName string

@description('Globally unique, 3-24 character lowercase storage account name.')
param storageAccountName string

@description('Six-field CRON schedule. Azure Functions evaluates this in UTC.')
param costReportSchedule string = '0 15 0 1-5 * *'

@description('Azure Table Storage table used for monthly cost summaries.')
param costTableName string = 'MonthlyCosts'

@description('Subscriptions to report, each with subscriptionId and displayName.')
param subscriptions array

var tableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}


resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'plan-${uniqueString(resourceGroup().id, functionAppName)}'
  location: location
  kind: 'linux'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      ftpsState: 'Disabled'
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'TABLE_STORAGE_ACCOUNT'
          value: storageAccount.name
        }
        {
          name: 'COST_TABLE_NAME'
          value: costTableName
        }
        {
          name: 'SUBSCRIPTIONS_JSON'
          value: string(subscriptions)
        }
        {
          name: 'COST_REPORT_SCHEDULE'
          value: costReportSchedule
        }
      ]
    }
  }
}

resource tableDataRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionAppName, tableDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      tableDataContributorRoleId
    )
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output functionAppPrincipalId string = functionApp.identity.principalId
