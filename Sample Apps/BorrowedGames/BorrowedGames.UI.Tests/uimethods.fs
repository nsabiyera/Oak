﻿module uimethods

open setupmethods

open runner

open canopy

let baseUrl = "http://localhost:3000";

let url = fun address -> canopy.url (baseUrl + address)

let on = fun address -> canopy.on (baseUrl + address)

let logOff _ =
    url "/account/logoff"
    ()

let goToSignIn _ =
    url "/"
    ()

let email userName =
    userName + "@example.com"

let registerUser userName =
    goToSignIn()
    click "a[href='/Account/Register']"
    on "/Account/Register"
    "#Email" << (email userName)
    "#Password" << "Password"
    "#PasswordConfirmation" << "Password"
    click "input[value='register']"
    on "/"
    click "#handle"
    "#handleTextBox" << userName
    click "input[value='update handle']"
    ".growlinfo" == ("Your handle has been updated to @" + userName + ".")
    logOff()

let loginAs userName =
    goToSignIn()
    "#Email" << (email userName)
    "#Password" << "Password"
    click "input[value='login']"
    on "/"

let addGame name =
    click "#showLibrary"
    "#gameToAdd" << name
    click "table tbody tr td"
    click "#closeLibraryTop"
    ()

let follow name =
    click "#showFriends"
    "#handleToAdd" << name
    click "#addHandle"
    click "#closeFriendsTop"

let requestFirstGame _ =
    click ".request"
    ()

let giveFirstRequestedGame _ =
    click "a.check"
    "#requestedGames a.cancel" == "The game has been returned"
    ()