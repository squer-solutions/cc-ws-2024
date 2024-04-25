
workshopManagement.cdcCustomerContainer -> legacySystem.database "Polls CDC"{
    tags async
}
workshopManagement.cdcCustomerContainer -> workshopManagement.cdcCustomerTopic "Pushes Changes"{
    tags async
}
