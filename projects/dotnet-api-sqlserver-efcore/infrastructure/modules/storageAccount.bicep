@description('Specifies the name of the environment.Ex test,prod. Kindly use the small case letters')
param env string
param defaultTags object
param location string
param storageAccountName string


var storageSku = (env == 'PROD') ? 'Standard_ZRS' : 'Standard_LRS'

resource storageAccountResource 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  tags:defaultTags
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  location: location
  properties: {
    allowBlobPublicAccess: false   
    publicNetworkAccess: 'Enabled' 
    minimumTlsVersion: 'TLS1_2'
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      resourceAccessRules : [
        {
            tenantId: tenant().tenantId
            resourceId: '${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Logic/workflows/*'
        }
      ]
      bypass: 'None'
      defaultAction: 'Deny'
    }
  }
}

resource blobServiceResource 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  name: 'default'
  parent: storageAccountResource
  properties: {
  }
}

resource fileServiceResource 'Microsoft.Storage/storageAccounts/fileServices@2021-09-01' = {
  name: 'default'
  parent: storageAccountResource
  properties: {
  }
}

resource tableServiceResource 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' = {
  name: 'default'
  parent: storageAccountResource
}



output storageAccountName string = storageAccountResource.name
