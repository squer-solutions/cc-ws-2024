!include ./styles.dsl

systemContext workshopManagement "SysContext_WsMngApp" "Shows the system relations of the Workshop Management" {
    include *
    autolayout
}

//container workshopManagement "WS_ContainerView_With_External_Systems" "Shows all the containers of the Workshop Management System and Related Systems" {
//    include *
//}

container workshopManagement "WS_ContainerView_With_External_Containers" "Shows all the containers of the Workshop Management System & Related External Containers" {
    include "->element.tag==team-ws-> && element.type==Container"
}
    
component workshopManagement.webApi "WS_WebApi_Components" "All the components running inside the web api service"{
    include *
    autolayout
}
