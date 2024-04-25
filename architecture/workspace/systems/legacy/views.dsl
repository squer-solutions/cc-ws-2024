!include ./styles.dsl

systemContext legacySystem "SysContext_LegacyApp" "Shows the system relations of the legacy application" {
    include *
    autolayout
}

container legacySystem "Legacy_ContainerView" "Shows all the containers of the Legacy App" {
    include *
    autolayout
}
