#MIT License
#http://www.github.com/amirrajan/rake-dot-net
#Copyright (c) 2011 Amir Rajan

require './RakeDotNet/command_shell.rb'
require './RakeDotNet/web_deploy.rb'
require './RakeDotNet/iis_express.rb'
require './RakeDotNet/sln_builder.rb'
require 'net/http'

task :rake_dot_net_initialize do
  @website_port = 3000
  @website_deploy_directory = 'c:\iisexpress\dynamicblog'
  @solution_name = "DynamicBlog.sln"
  @mvc_project_directory = "DynamicBlog"
  @test_dll = "./DynamicBlog.Tests/bin/debug/DynamicBlog.Tests.dll"
  @test_command = "./packages/nspec.0.9.43/tools/NSpecRunner.exe #{@test_dll}"
  
  @iis_express = IISExpress.new
  @iis_express.execution_path = 'C:\Program Files (x86)\IIS Express'
  @web_deploy = WebDeploy.new
  @sh = CommandShell.new
  @sln = SlnBuilder.new  
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
task :server => :rake_dot_net_initialize do |t, args|
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

desc "run nspec tests"
task :tests => :build do
  sh @test_command
end

def reset_db
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/PurgeDb"), { })
  puts Net::HTTP.post_form(URI.parse("http://localhost:#{@website_port.to_s}/seed/all"), { })
end
