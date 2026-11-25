using '../main.bicep'

param location = 'westeurope'
param env = 'TEST'
param webAppName = 'app-order-api-test-change-me'
param sqlAdministratorLoginPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD', '')
