
@description('Globally unique, 3-24 character lowercase storage account name.')
param storageAccountName string

@description('Azure region for the function app and storage account.')
param location string = resourceGroup().location

@description('Azure Table Storage table used for monthly cost summaries.')
param costTableName string = 'MonthlyCosts'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
    supportsHttpsTrafficOnly: true
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource costTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: costTableName
}

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
