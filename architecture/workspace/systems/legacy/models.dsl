legacySystem = softwareSystem "The legacy software system" {

    tags "team-legacy"
    

    database = container "Legacy" {
        description "Holds the legacy data, a relational database"
        technology "postgres"
        tags "postgres" "team-legacy"
    }

    webApp = container "MVC Web App" {
        description "This is the old monolithic application"
        technology ".NET"
        tags "csharp" "WebApp" "team-legacy"
    }
    
    webApp -> database "Reads/writes data from/to"
}
