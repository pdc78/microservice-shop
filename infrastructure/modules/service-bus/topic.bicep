param serviceBusName string
param topic object


resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusName
}

// param sbtOrderDev string = 'sbt-order-dev'

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: topic.fullTopicName
  properties: {
    defaultMessageTimeToLive: 'P7D'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: false
  }
}



module subscriptionModules './subscription.bicep' = [for currentSubscription in topic.subscriptions: {
  name: '${currentSubscription.name}'
   params: {
     serviceBusName: serviceBusNamespace.name
     topicName: serviceBusTopic.name
     subscription: currentSubscription
    }
}]


output serviceBusTopicName string = serviceBusTopic.name
output serviceBusTopicCreated object = serviceBusTopic

output debugTopic object = topic
