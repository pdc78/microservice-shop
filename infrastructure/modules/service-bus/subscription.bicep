param topicName string
param subscription object
param serviceBusName string


resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusName
}


resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' existing = {
  name: topicName
  parent: serviceBusNamespace
}

resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: serviceBusTopic
  name: subscription.fullSubscriptionName
  properties: {
    defaultMessageTimeToLive: 'P7D'
    lockDuration: 'PT5M'
    requiresSession: false
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

resource serviceBusSubscriptionRule 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2024-01-01' = [for currentFilter in (contains(subscription, 'filters') ? subscription.?filters : []):  {
  parent: serviceBusSubscription
  name:  currentFilter.label
  properties: {
    filterType:  currentFilter.type
    correlationFilter: {
      label: currentFilter.label
      // applicationProperties: contains(currentFilter, 'applicationProperties') ? currentFilter.applicationProperties : null
      properties: contains(currentFilter, 'customProperties') ? currentFilter.?customProperties : null
    }
  }
}] 

output serviceBusSubscriptionName string = serviceBusSubscription.name
output serviceBusTopicName string = serviceBusTopic.name
output subscriptionName string = subscription.name
