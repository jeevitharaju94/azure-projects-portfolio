targetScope = 'resourceGroup'

@description('Azure region for the Web App and SQL database.')
param location string = resourceGroup().location

param env string

@description('Globally unique Azure App Service Web App name.')
param webAppName string

@description('SQL administrator password. Supply this as a secure deployment parameter.')
@secure()
param sqlAdministratorLoginPassword string

var defaultTags = {
    Environment: toUpper(env)
    DeploymentMethod: 'Bicep'
    Project: 'DotnetApiSqlServerEfCore'
}

module webAppModule 'modules/webApp.bicep' = {
  name: 'webApp'
  params: {
    location: location
    webAppName: webAppName
    env: env
    defaultTags: defaultTags
    connectionString: 'Server=tcp:${sqlDbModule.outputs.sqlServerFqdn},1433;Initial Catalog=${sqlDbModule.outputs.sqlDatabaseName};Persist Security Info=False;User ID=sqladmin;Password=${sqlAdministratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

module sqlDbModule 'modules/sqldb.bicep' = {
  name: 'sqlDb'
  params: {
    sqlServerName: toLower('sqlserver-${webAppName}')
    sqlDatabaseName: toLower('sqldb-${webAppName}')
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdministratorLoginPassword
    location: location
    defaultTags: defaultTags
  }
}

output webAppUrl string = 'https://${webAppModule.outputs.defaultHostName}'

