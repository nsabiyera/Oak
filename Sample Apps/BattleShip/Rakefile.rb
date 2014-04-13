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

task :rake_dot_net_initialize do
  yml = YAML::load File.open("dev.yml")
  @website_port = yml["website_port"]
  @website_deploy_directory = yml["website_deploy_directory"]
  @website_port_load_balanced_1 = yml["website_port_load_balanced_1"]
  @website_deploy_directory_load_balanced_1 = yml["website_deploy_directory_load_balanced_1"]
  @website_port_load_balanced_2 = yml["website_port_load_balanced_2"]
  @website_deploy_directory_load_balanced_2 = yml["website_deploy_directory_load_balanced_2"]
  @solution_name = "#{ yml["solution_name"] }.sln"
  @solution_name_sans_extension = "#{ yml["solution_name"] }"
  @mvc_project_directory = yml["mvc_project"]

  @test_project = yml["test_project"]
  @test_dll = "./#{ yml["test_project"] }/bin/debug/#{ yml["test_project"] }.dll"

  @test_runner_path = yml["test_runner"]
  @test_runner_command = "#{ yml["test_runner"] } #{ @test_dll }"
  
  @iis_express = IISExpress.new
  @iis_express.execution_path = yml["iis_express"]
  @web_deploy = WebDeploy.new
  @sh = CommandShell.new
  @sln = SlnBuilder.new
  @sln.msbuild_path = "C:\\Program Files (x86)\\MSBuild\\12.0\\bin\\amd64\\msbuild.exe"
  @file_sync = FileSync.new
  @file_sync.source = @mvc_project_directory
  @file_sync.destination = @website_deploy_directory
  @database_name = yml["database_name"]
  @database_server = yml["database_server"]
  @test_database_name = yml["database_name"] + "Test"
end

desc "builds and deploys website to directories iis express will use to run app"
task :default => [:build, :deploy]

desc "builds the solution"
task :build => :rake_dot_net_initialize do
  @sln.build @solution_name 
end

desc "deploys MVC app to directory that iis express will use to run"
task :deploy => :rake_dot_net_initialize do 
  @web_deploy.deploy @mvc_project_directory, @website_deploy_directory
end

desc "start iis express for MVC app"
task :server => :rake_dot_net_initialize do
  sh @iis_express.command @website_deploy_directory, @website_port
end

desc "if you have the nuget package oak installed, use this to seed sample data"
task :sample => :rake_dot_net_initialize do
  reset_db
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/SampleEntries"), { })
end

desc "if you have the nuget package oak installed, use this to reset the database"
task :reset => :rake_dot_net_initialize do
  reset_db
end

desc "if you have the nuget package oak installed, use this to export scripts to .sql files"
task :export => :rake_dot_net_initialize do
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/Export"), { })
end

desc "run nspec tests"
task :tests => :build do
  puts "Could not find the NSpec test runner at location #{ @test_runner_path }, update your dev.yml to point to the correct runner location." if !File.exists? @test_runner_path
  sh @test_runner_command if File.exists? @test_runner_path
end

desc "synchronizes a file specfied to the website deployment directory"
task :sync, [:file] => :rake_dot_net_initialize do |t, args|
  @file_sync.sync args[:file]
end

def reset_db
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/PurgeDb"), { })
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/all"), { })
end

desc "simulate the web application as if it were load balanced, no iisexpress instances should be running when this task is executed"
task :simulate_load_balance => :rake_dot_net_initialize do
  @sln.build @solution_name 
  @web_deploy.deploy @mvc_project_directory, @website_deploy_directory_load_balanced_1
  @web_deploy.deploy @mvc_project_directory, @website_deploy_directory_load_balanced_2
  sh @iis_express.command @website_deploy_directory_load_balanced_1, @website_port_load_balanced_1
  sh @iis_express.command @website_deploy_directory_load_balanced_2, @website_port_load_balanced_2
  generate_nginx_config
  cd "nginx"
  puts "starting nginx (pronouced engine-x) for round robin load balancing"
  sh "start nginx.exe"
  puts "started!"
  cd ".."
  puts "type rake stop_nginx to stop the load balance"
end

desc "stops nginx"
task :stop_nginx do
  cd "nginx"
  sh "nginx.exe -s quit"
  cd ".."
end

def generate_nginx_config
  File.chmod(0777, "nginx/conf/nginx.conf.template")
  content = File.read("nginx/conf/nginx.conf.template")
  newcontent = content.gsub /website_port/, @website_port.to_s
  newcontent = content.gsub /website_port_load_balanced_1/, @website_port_load_balanced_1.to_s
  newcontent = content.gsub /website_port_load_balanced_2/, @website_port_load_balanced_2.to_s
  
  File.open("nginx/conf/nginx.conf.template", 'w') { |f| f.write(newcontent) }
end

desc "creates your databases if they don't exist"
task :create_db => :rake_dot_net_initialize do
  execute_sql "master", "create database #{ @database_name }", @database_server
  execute_sql "master", "create database #{ @test_database_name }", @database_server
  puts "\nDone.\n\n"
  puts "If you received DB connection errors, run the command: rake list_db_servers"
end

desc "lists database servers on the network and provides a rake command for updating your database connection string"
task :list_db_servers => :rake_dot_net_initialize do
  puts "scanning the network for db servers (this may take a while)"
  puts "if you already know what database server you want to connect to,"
  puts "you can change your connection strings in the application by running the command:"
  puts "rake update_db_server[server_name], example: rake update_db_server[.\\sqlexpress]"
  puts "looking....\n\n"

  output = `sqlcmd -Lc`
  puts "here are your db servers and the rake command to update your connection strings:\n\n"
  output.each_line do |line|
    puts "rake update_db_server[#{line.strip}]" if line.strip != ""
  end
end

desc "updates all database connection string server values to the value specified, example: rake update_db_server[./sqlexpress]"
task :update_db_server, [:new_value] => :rake_dot_net_initialize do |t, args|
  raise "You need to specify a new database server name, example: rake update_db_server[./sqlexpress]" if args[:new_value].nil?

  [
    "#{ @mvc_project_directory }/Web.config",
    "#{ @test_project }/App.config"
  ].each do |file|
    puts "updating connection string in: #{ file }"
    content = File.open(file).read
    content.gsub!("data source=#{ @database_server };", "data source=#{ args[:new_value] };")
    File.open(file, "w") { |f| f.write(content) }
    puts "done"
  end

  puts "updating dev.yml"
  content = File.open("dev.yml").read
  content.gsub!("database_server: #{ @database_server }",
                "database_server: #{ args[:new_value] }")
  File.open("dev.yml", "w") { |f| f.write(content) }
  puts "done"
  
  puts "if you're ran this because of errors related to rake create_db, run rake create_db again now"
end

def execute_sql database, sql, server
  puts `sqlcmd -d #{ database } -S #{ server } -Q \"#{ sql }\"`
end
