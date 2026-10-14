targetScope = 'subscription'

@description('Principal ID of the deployed cost-reporting Function App managed identity.')
param principalId string

var costManagementReaderRoleId = '72fafb9e-0641-4937-9268-a91bfd8191a3'

resource costManagementReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().id, principalId, costManagementReaderRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      costManagementReaderRoleId
    )
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
