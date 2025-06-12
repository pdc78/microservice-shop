param location string = resourceGroup().location
param appName string = 'shop'
param environment string = 'dev'

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
        {
          name: 'inventory-request'
          fullSubscriptionName: 'sbts-${appName}-${environment}-inventory-request'
          filters: [
            {
              type: 'CorrelationFilter'
              label: 'inventory-cancelled-rule'
              customProperties: {
                messageType: 'InventoryCancelledEvent'
              }
            }
            {
              type: 'CorrelationFilter'
              label: 'inventory-requested-rule'
              customProperties: {
                messageType: 'InventoryRequestedEvent'
              }
            }
          ]
        }
      ]
    }
    // {
    //   name: 'payment'
    //   fullTopicName: 'sbt-${appName}-${environment}-payment'
    //    subscriptions: [
    //     {
    //       name: 'payment-all'
    //       fullSubscriptionName: 'sbts-${appName}-${environment}-payment-all'
    //     }
    //   ]
    // }
    // {
    //   name: 'shipping'
    //   fullTopicName: 'sbt-${appName}-${environment}-shipping'
    //     subscriptions: [
    //     {
    //       name: 'payment-all'
    //       fullSubscriptionName: 'sbts-${appName}-${environment}-payment-all'
    //     }
    //   ]
    // }
  ]
}

module serviceBus './modules/service-bus/service-bus.bicep' = {
  name: 'serviceBusModule'
  params: {
    location: location
    topicsConfig: topicsConfig
  }
}
