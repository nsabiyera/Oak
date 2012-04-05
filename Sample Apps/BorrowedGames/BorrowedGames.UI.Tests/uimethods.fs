module uimethods

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

let clickWait css =
    click css
    sleep()
    ()

let writeWait css value =
    write css value
    sleep()
    ()

let email userName =
    userName + "@example.com"

let registerUser userName =
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

let loginAs userName =
    goToSignIn()
    write "#Email" (email userName)
    write "#Password" "Password"
    click "input[value='login']"
    on "/"

let addGame name =
    clickWait "#showLibrary"
    writeWait "#gameToAdd" name
    clickWait "table tbody tr td"
    clickWait "#closeLibraryTop"
    ()

let follow name =
    clickWait "#showFriends"
    write "#handleToAdd" name
    click "#addHandle"
    clickWait "#closeFriendsTop"

let requestFirstGame _ =
    click ".request"
    ()

let giveFirstRequestedGame _ =
    click "a.check"
    ()