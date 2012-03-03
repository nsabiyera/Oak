require 'capybara/rspec'

Capybara.default_driver = :selenium

feature "registering a user" do
  scenario "registering with passwords that match" do
    `rake reset`
    visit "http://localhost:3000/Account/Register" 
    fill_in "Email", :with => 'user@example.com'
    fill_in "Password", :with => 'password'
    fill_in "PasswordConfirmation", :with => 'password'
    click_on "register"
    current_path.should == "/"
  end
end
