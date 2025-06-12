
param location string
param topicsConfig object


resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: topicsConfig.ServiceBusNamespace
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    zoneRedundant: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

var namespaceAuthorizationRuleName string = 'RootSharedAccessKeyReadWrite'

resource serviceBusAuthorizationRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2024-01-01' = {
  parent: serviceBusNamespace
  name: namespaceAuthorizationRuleName
  properties: {
    rights: [
      'Listen'
      'Send'
    ]
  }
}



module topicModules './topic.bicep' = [for topic in topicsConfig.Topics: {
  name: '${topic.name}'
   params: {
     serviceBusName: serviceBusNamespace.name
     topic: topic
    }
}]
