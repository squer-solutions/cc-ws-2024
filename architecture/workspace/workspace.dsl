workspace {

    !adrs decisions
    !identifiers hierarchical

    model {
        properties {
            "structurizr.groupSeparator" "/"
        }

        !include ./systems/legacy/models.dsl
        !include ./systems/workshop-management/models.dsl
        
        !include relations.dsl
    }

    views {

        !include ./styles/styles.dsl

        !include ./views.dsl
    }

}
    
