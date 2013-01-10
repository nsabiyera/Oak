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
  @mvc_project_directory = yml["mvc_project"]
  @test_dll = "./#{ yml["test_project"] }/bin/debug/#{ yml["test_project"] }.dll"

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
