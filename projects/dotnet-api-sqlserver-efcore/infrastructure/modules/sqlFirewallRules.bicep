@description('Name of the Azure SQL logical server.')
param sqlServerName string

@description('Outbound IP addresses assigned to the App Service Web App.')
param appServiceOutboundIps array

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' existing = {
  name: sqlServerName
}

resource appServiceFirewallRules 'Microsoft.Sql/servers/firewallRules@2023-08-01' = [for (ipAddress, index) in appServiceOutboundIps: {
  parent: sqlServer
  name: 'app-service-${index}'
  properties: {
    startIpAddress: ipAddress
    endIpAddress: ipAddress
  }
}]
