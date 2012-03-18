open runner

open canopy

open uimethods

open setupmethods

start "firefox"

test(fun _ -> 
    describe "registering a user"
    reset()
    registerUser "user1@example.com")

test(fun _ ->
    describe "request games"
    reset()
    stageGame "Dark Souls"
    registerUser "user1@example.com"
    registerUser "user2@example.com"
    loginAs "user1@example.com"
    addGame "Dark Souls"
    logOff()
    loginAs "user2@example.com")

run()
 
quit()