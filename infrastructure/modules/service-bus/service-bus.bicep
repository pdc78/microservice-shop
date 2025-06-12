
param location string
param appName string = 'shop'
param environment string = 'dev'
param topics array = ['order', 'inventory', 'payment', 'shipping']
param topicsConfig object


//     environment: environment
//     topics: topics
//     location: location


// var sbNamespace string = 'sbns-${appName}-${environment}-${location}'

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


// module topicModules './topic.bicep' = [for topic in topics: {
//   name: 'topicModule-${topic}'
//   params: {
//      serviceBusName: serviceBusNamespace.name
//      topicName: 'sbt-${appName}-${topic}-${environment}'
//   }
// }]

  // name: 'serviceBusModule'
  // params: {
  //   appName: appName
  //   environment: environment
  //   topics: topics
  //   location: location
  // }



// // param sbtOrderDev string = 'sbt-order-dev'

// resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
//   parent: serviceBusNamespace
//   name: sbtOrderDev
//   properties: {
//     defaultMessageTimeToLive: 'P7D'
//     maxSizeInMegabytes: 1024
//     requiresDuplicateDetection: false
//     duplicateDetectionHistoryTimeWindow: 'PT10M'
//     enableBatchedOperations: true
//     status: 'Active'
//     supportOrdering: false
//   }
// // }

// resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
//   parent: serviceBusTopic
//   name: sbtsOrderDev
//   properties: {
//     defaultMessageTimeToLive: 'P7D'
//     lockDuration: 'PT5M'
//     requiresSession: false
//     deadLetteringOnMessageExpiration: true
//     maxDeliveryCount: 10
//   }
// }

// resource serviceBusSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2024-01-01' = {
//   name: '${sbNamespace}/${sbtOrderDev}/${sbtsOrderDev}/Rule1'
//   properties: {
//     filterType: 'CorrelationFilter'
//     correlationFilter: {
//       label: 'order-filter'
//       applicationProperties: {
//         'orderType': 'new'
//       }
//     }
//   }
// }
