module uimethods

open setupmethods

open runner

open canopy

let baseUrl = "http://localhost:3000";

let url = fun address -> canopy.url (baseUrl + address)

let on = fun address -> canopy.on (baseUrl + address)

let logOff = fun _ ->
    url "/account/logoff"
    ()

let goToSignIn = fun _ ->
    url "/"
    ()

let email = fun userName ->
    userName + "@example.com"

let registerUser = fun userName ->
    goToSignIn()
    click "a[href='/Account/Register']"
    on "/Account/Register"
    write "#Email" (email userName)
    write "#Password" "Password"
    write "#PasswordConfirmation" "Password"
    click "input[value='register']"
    on "/"
    click "#handle"
    write "#handleTextBox" userName
    click "input[value='update handle']"
    ".growlinfo" == ("Your handle has been updated to @" + userName + ".")
    logOff()

let loginAs = fun userName ->
    goToSignIn()
    write "#Email" (email userName)
    write "#Password" "Password"
    click "input[value='login']"
    on "/"

let addGame = fun name ->
    click "#showLibrary"
    write "#gameToAdd" name
    click "table tbody tr td"
    click "#closeLibraryTop"

let follow = fun name ->
    click "#showFriends"
    write "#handleToAdd" name
    click "#addHandle"
    click "#closeFriendsTop"

let request = fun gameName ->
    ()