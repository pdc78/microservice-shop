param location string = resourceGroup().location
param appName string = 'shop'
param environment string = 'dev'
param topics array = ['order', 'inventory', 'payment', 'shipping']

/*
  Add a request and response queue to the service bus and the relevant access policies 
*/
var topicsConfig = {
  ServiceBusNamespace: 'sbns-${appName}-${environment}-${location}'
  Topics: [
    {
      name: 'topic-order'
      fullTopicName: 'sbt-${appName}-${environment}-order'
      subscriptions: [
        {
          name: 'subscription-order'
          fullSubscriptionName: 'sbts-${appName}-${environment}-order-all'
        }
      ]
    }
    {
      name: 'inventory'
      fullTopicName: 'sbt-${appName}-${environment}-inventory'
      subscriptions: [
        {
          name: 'inventory-all'
          fullSubscriptionName: 'sbts-${appName}-${environment}-inventory-all'
        }
        // {
        //   name: 'inventory-request'
        //   fullSubscriptionName: 'sbts-${appName}-${environment}-inventory-request'
        //   filter: {
        //     type: 'CorrelationFilter'
        //     correlationFilter: {
        //       label: 'inventory-request'
        //       applicationProperties: {
        //         'messageType': 'InventoryCancelledEvent'
        //       }
        //     }
        //   }
        // }
      ]
    }
    {
      name: 'payment'
      fullTopicName: 'sbt-${appName}-${environment}-payment'
       subscriptions: [
        {
          name: 'payment-all'
          fullSubscriptionName: 'sbts-${appName}-${environment}-payment-all'
        }
      ]
    }
    {
      name: 'shipping'
      fullTopicName: 'sbt-${appName}-${environment}-shipping'
        subscriptions: [
        {
          name: 'payment-all'
          fullSubscriptionName: 'sbts-${appName}-${environment}-payment-all'
        }
      ]
    }
  ]
}

module serviceBus './modules/service-bus/service-bus.bicep' = {
  name: 'serviceBusModule'
  params: {
    appName: appName
    environment: environment
    topics: topics
    location: location
    topicsConfig: topicsConfig
  }
}
