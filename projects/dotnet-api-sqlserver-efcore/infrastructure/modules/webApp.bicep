@description('Globally unique Azure App Service Web App name.')
param webAppName string

@description('Azure region for the Web App and App Service plan.')
param location string = resourceGroup().location

@description('Deployment environment name.')
param env string

@description('Tags to apply to the Web App and App Service plan.')
param defaultTags object = {}

@description('SQL connection string for the Order API.')
@secure()
param connectionString string

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'plan-${uniqueString(resourceGroup().id, webAppName)}'
  location: location
  kind: 'windows'
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
  tags: defaultTags
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app'
  tags: defaultTags
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      alwaysOn: true
      ftpsState: 'Disabled'
      netFrameworkVersion: 'v10.0'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: toLower(env) == 'dev' ? 'Development' : 'Production'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: connectionString
        }
      ]
    }
  }
}

output defaultHostName string = webApp.properties.defaultHostName
