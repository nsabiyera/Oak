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
require './stacktrace.rb'

task :rake_dot_net_initialize do
  yml = YAML::load File.open("dev.yml")
  @solution_name = yml["solution_name"]
  @test_dll = yml["test_dll"]
  @test_runner_command = "#{ yml["test_runner"] } #{ @test_dll }"
  @database_name = yml["database_name"]
  @database_server = yml["database_server"]
  
  @sh = CommandShell.new
  @sln = SlnBuilder.new
  @sln.msbuild_path = "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe"
end

desc "builds and deploys website to directories iis express will use to run app"
task :default => [:build]

desc "builds the solution"
task :build => :rake_dot_net_initialize do
  @sln.build @solution_name 
end

desc "run nspec tests"
task :tests => :build do
  results = `#{@test_runner_command}`

  puts results

  write_stack_trace results
end

desc "run nspec tests tagged with Tag[\"wip\"]"
task :wip => :build do
  sh @test_runner_command + " --tag wip"
end

desc "run nspec tests tagged with args passed in"
task :test_for_tag, [:tag] => :build do |t, args|
  command = @test_runner_command + " --tag #{args[:tag]}"

  results = `#{command}`

  puts results

  write_stack_trace results
end

desc "run nspec tests"
task :tests_excluding_performance => :build do
  sh @test_runner_command + " --tag ~performance"
end

desc "generates ctags"
task :tags do
  sh "ctags --recurse --exclude=\"Sample Apps\""
end

desc "creates the databases needed to run oak tests"
task :create_db => :rake_dot_net_initialize do
  execute_sql "master", "create database #{ @database_name }", @database_server
  execute_sql "master", "create database #{ @database_name }2", @database_server
  puts "\nDone.\n\n"
  puts "If you received DB connection errors, run the command: rake list_db_servers"
end

desc "lists database servers on the network and provides a rake command for updating your database connection string"
task :list_db_servers => :rake_dot_net_initialize do
  puts "scanning the network for db servers (this may take a while)"
  puts "if you already know what database server you want to connect to,"
  puts "you can change your connection strings in the application by running the command:"
  puts "rake update_db_server[server_name], example: rake update_db_server[\'.\\sqlexpress\']"
  puts "looking....\n\n"

  output = `sqlcmd -Lc`
  puts "here are your db servers and the rake command to update your connection strings:\n\n"
  output.each_line do |line|
    puts "rake update_db_server[\'#{line.strip}\']" if line.strip != ""
  end

  puts "\nRun the command exactly as it is displayed above, including single quotes around the server name"
end

desc "updates all database connection string server values to the value specified, example: rake update_db_server[./sqlexpress]"
task :update_db_server, [:new_value] => :rake_dot_net_initialize do |t, args|
  raise "You need to specify a new database server name, example: rake update_db_server[./sqlexpress]" if args[:new_value].nil?

  [
    "Oak.Tests/App.config"
  ].each do |file|
    if File.exist? file
      puts "updating connection string in: #{ file }"
      content = File.open(file).read
      content.gsub!("Data Source=#{ @database_server };", "Data Source=#{ args[:new_value] };")
      File.open(file, "w") { |f| f.write(content) }
      puts "done"
    end
  end

  puts "updating dev.yml"
  content = File.open("dev.yml").read
  content.gsub!("database_server: #{ @database_server }",
                "database_server: #{ args[:new_value] }")
  File.open("dev.yml", "w") { |f| f.write(content) }
  puts "done"
  
  puts "if you ran this because of errors related to rake create_db, run rake create_db again now"
end

def execute_sql database, sql, server
  puts `sqlcmd -d #{ database } -S #{ server } -Q \"#{ sql }\"`
end

def write_stack_trace test_output
  stacktrace = ""

  if test_output.match /FAILURES/
    stacktrace = test_output.split('**** FAILURES ****').last.strip
    stacktrace = stacktrace.split(/^.*Examples, /).first.strip
  end

  File.open("stacktrace.txt", 'w') { |f| f.write(stacktrace) }
end
