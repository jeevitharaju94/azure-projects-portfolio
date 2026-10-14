using '../main.bicep'

param location = 'westeurope'
param functionAppName = 'func-cost-report-change-me'
param storageAccountName = 'costreportchange123'
param subscriptions = [
  {
    subscriptionId: '00000000-0000-0000-0000-000000000000'
    displayName: 'Replace with a subscription name'
  }
]
param costReportSchedule = '0 15 0 1-5 * *'
param costTableName = 'MonthlyCosts'
