using '../main.bicep'

param location = 'westeurope'
param env = 'DEV'
param webAppName = 'app-order-api-dev-change-me'
param sqlAdministratorLoginPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD', '')
