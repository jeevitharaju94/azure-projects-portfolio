targetScope = 'resourceGroup'

@description('Azure region for the function app and storage account.')
param location string = resourceGroup().location

@description('Globally unique Function App name.')
param functionAppName string

@description('Globally unique, 3-24 character lowercase storage account name.')
param storageAccountName string

@description('Subscriptions to report, each with subscriptionId and displayName.')
param subscriptions array

@description('Six-field NCRONTAB schedule. Azure Functions evaluates this in UTC.')
param costReportSchedule string = '0 15 0 1-5 * *'

@description('Azure Table Storage table used for monthly cost summaries.')
param costTableName string = 'MonthlyCosts'

var tableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'



module storageAccountModule 'modules/storageAccount.bicep' = {
  name: 'storageAccount'
  params: {
    storageAccountName: storageAccountName
    location: location
    costTableName: costTableName
  }
}

module functionAppModule 'modules/functionApp.bicep' = {
  name: 'functionApp'
  params: {
    location: location
    functionAppName: functionAppName
    storageAccountName: storageAccountModule.outputs.storageAccountName
    costTableName: costTableName
    subscriptions: subscriptions
    costReportSchedule: costReportSchedule
  }
}

module roleAssignmentModule 'modules/costMgmtRoleAssignment.bicep' = {
  name: 'roleAssignment'
  scope: subscription(subscription().subscriptionId)
  params: {
    principalId: functionAppModule.outputs.functionAppPrincipalId

  }
}

