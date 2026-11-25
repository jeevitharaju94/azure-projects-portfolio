@description('Globally unique name for the Azure SQL logical server.')
@minLength(1)
@maxLength(63)
param sqlServerName string

@description('Name of the Azure SQL database to create on the logical server.')
param sqlDatabaseName string

@description('Administrator login for SQL authentication.')
param administratorLogin string

@description('Administrator password. Supply this securely, for example from Azure Key Vault.')
@secure()
param administratorLoginPassword string

@description('Azure region for the SQL server and database.')
param location string = resourceGroup().location

@description('Tags to apply to the SQL server and database.')
param defaultTags object = {}

@description('Azure SQL Database SKU name. The default creates a Basic DTU database.')
param databaseSkuName string = 'Basic'

@description('Azure SQL Database SKU tier.')
param databaseSkuTier string = 'Basic'

@description('Azure SQL Database SKU capacity in DTUs or vCores.')
param databaseSkuCapacity int = 5

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: sqlServerName
  location: location
  tags: defaultTags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  tags: defaultTags
  sku: {
    name: databaseSkuName
    tier: databaseSkuTier
    capacity: databaseSkuCapacity
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    requestedBackupStorageRedundancy: 'Local'
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
