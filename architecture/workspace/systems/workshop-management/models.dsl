workshopManagement = softwareSystem "The new Workshop Management System" {

    tags "team-ws"

    database = container "Customer DB" {
        description "Holds the customer data, its a document database"
        technology "mongodb"
        tags "mongodb" "team-ws"
    }

    webApi = container "Customer API" {
        description "This the new API for managing customer data"
        technology ".NET"
        tags "csharp" "WebApi" "team-ws"

        customerTransformedHandler = component "Customer Transformed Handler" {
            description "Subscribes to the customer-transformed-topic and saves changes to the database"
            technology ".NET"
            tags "csharp" "team-ws"
        }

        getCustomerEndpoint = component "Get" {
            description "Returns all customers"
            technology ".NET"
            tags "csharp" "team-ws"
        }
    }

    webApp = container "Blazor Web Application" {
        description "The frontend of the new Workshop Management System"
        technology ".NET"
        tags "csharp" "WebApp" "team-ws"
    }
    
    cdcCustomerContainer = container "CDC Customer" {
        description "The Debezium Connector to publish CDC Changes"
        technology "debezium"
        tags "debezium" "team-ws"
    } 

    cdcCustomerTopic = container "cdc.public.customer" {
        description "The topic to push cdc changes to"
        technology "kafka"
        tags "kafka" "team-ws"
    }

    transformer = container "transformer stream" {
        description "The transformer application, converts cdc events to Customer"
        technology "kafka stream"
        tags "java" "team-ws"
    }

    customerTransformedTopic = container "customer-transformed" {
        description "The topic to push cdc changes to"
        technology "kafka"
        tags "kafka" "team-ws"
    }

    webApi -> database "reads data from"
    webApp -> webApi.getCustomerEndpoint "fetches customer data" {
        tags "sync" "http"
    }
    
    transformer -> cdcCustomerTopic "streams data"{
        tags async
    }

    transformer -> customerTransformedTopic "pushes transformed data" {
        tags async
    }

    webApi.customerTransformedHandler -> customerTransformedTopic "subscribed"{
        tags async
    }
    webApi.customerTransformedHandler -> database "saves arriving data"
    webApi.customerTransformedHandler -> webApp "pushes customer data - websocket" {
        tags "async"
    }
}
