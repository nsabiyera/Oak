#MIT License
#http://www.github.com/amirrajan/rake-dot-net
#Copyright (c) 2011 Amir Rajan

require './RakeDotNet/command_shell.rb'
require './RakeDotNet/web_deploy.rb'
require './RakeDotNet/iis_express.rb'
require './RakeDotNet/sln_builder.rb'
require './RakeDotNet/file_sync.rb'
require 'net/http'
require 'yaml'
require 'coffee-script'

task :rake_dot_net_initialize do
  yml = YAML::load File.open("dev.yml")
  @website_port = yml["website_port"]
  @website_deploy_directory = yml["website_deploy_directory"]
  @solution_name = yml["solution_name"]
  @mvc_project_directory = yml["mvc_project_directory"]
  @test_dll = yml["test_dll"]
  
  @test_runner_path = yml["test_runner"]
  @test_runner_command = "#{ yml["test_runner"] } #{ @test_dll }"
  
  @iis_express = IISExpress.new
  @iis_express.execution_path = yml["iis_express"]
  @web_deploy = WebDeploy.new
  @sh = CommandShell.new
  @sln = SlnBuilder.new
  @file_sync = FileSync.new
  @file_sync.source = @mvc_project_directory
  @file_sync.destination = @website_deploy_directory
end

desc "builds and deploys website to directories iis express will use to run app"
task :default => [:build, :deploy]

desc "run ui automation tests"
task :ui => [:build, :ui_tests]

desc "builds the solution"
task :build => :rake_dot_net_initialize do
  @sln.build @solution_name 
end

desc "builds, runs tests, deploys and seeds sample data"
task :all => [:build, :coffee, :compass, :deploy, :tests, :server, :sample]

desc "deploys MVC app to directory that iis express will use to run"
task :deploy => :rake_dot_net_initialize do 
  @web_deploy.deploy @mvc_project_directory, @website_deploy_directory
end

desc "compile compass css"
task :compass => :rake_dot_net_initialize do
  @sh.execute "compass compile BorrowedGames"
end

desc "start iis express for MVC app"
task :server => :rake_dot_net_initialize do
  sh @iis_express.command @website_deploy_directory, @website_port
end

desc "if you have the nuget package oak installed, use this to seed sample data"
task :sample => :rake_dot_net_initialize do
  delete_all_records
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/SampleEntries"), { })
end

desc "if you have the nuget package oak installed, use this to reset the database"
task :reset => :rake_dot_net_initialize do
  delete_all_records
end

desc "run nspec tests"
task :tests => :build do |t, args|
  command = @test_runner_command
  
  command = @test_runner_command + " --tag #{ args[:tag] }" if args[:tag]

  command = command + " --failfast" if args[:failfast]

  puts "Could not find the NSpec test runner at location #{ @test_runner_path }, update your dev.yml to point to the correct runner location" if !File.exists? @test_runner_path
  
  sh command if File.exists? @test_runner_path
end

desc "synchronizes a file specfied to the website deployment directory"
task :sync => :rake_dot_net_initialize do |t, args|
  @file_sync.sync args[:file]
end

desc "convert coffee script to javascript"
task :coffee => :rake_dot_net_initialize do 
  build_coffee_scripts
end

def delete_all_records
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/DeleteAllRecords"), { })
end

def build_coffee_scripts
  puts "coffee time"

  Dir.glob("BorrowedGames/Scripts/app/*.coffee").each do |f| 
    js = CoffeeScript.compile(File.open(f))
    js = remove_encoding js
    jsFileName = File.dirname(f) + "/" + File.basename(f, ".coffee") + ".js"
    file = File.new(jsFileName, "w")
    file.write(js)
    file.close
    @file_sync.sync jsFileName
    puts "compiled/synced #{jsFileName}" 
  end
end

def remove_encoding output
  return output.gsub "\u0393\u00EA\u2310\u0393\u00F2\u00F9\u0393\u00F6\u00C9;", ""
end

desc "runs ui tests (without building)"
task :ui_tests do
  sh "BorrowedGames.UI.Tests\\bin\\Debug\\BorrowedGames.UI.Tests.exe"
end

desc "purges the database and regenerates schema"
task :regen_db => :build do
  sh regen_db_command("Data Source=(local);Initial Catalog=BorrowedGames;Integrated Security=true")

  sh regen_db_command("Data Source=(local);Initial Catalog=BorrowedGames_Test;Integrated Security=true")
end

def regen_db_command connection_string
  exe_location = "BorrowedGames.SchemaGen\\bin\\debug\\BorrowedGames.SchemaGen.exe"

  return "#{ exe_location } \"#{ connection_string }\""
end

