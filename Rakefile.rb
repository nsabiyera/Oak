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
  @solution_name = yml["solution_name"]
  @test_dll = yml["test_dll"]
  @test_runner_command = "#{ yml["test_runner"] } #{ @test_dll }"
  
  @sh = CommandShell.new
  @sln = SlnBuilder.new
end

desc "builds and deploys website to directories iis express will use to run app"
task :default => [:build]

desc "builds the solution"
task :build => :rake_dot_net_initialize do
  @sln.build @solution_name 
end

desc "run nspec tests"
task :tests => :build do
  sh @test_runner_command
end

desc "run nspec tests tagged with Tag[\"wip\"]"
task :wip => :build do
  sh @test_runner_command + " --tag wip"
end

desc "run nspec tests tagged with args passed in"
task :test_for_tag, [:tag] => :build do |t, args|
  sh @test_runner_command + " --tag #{args[:tag]}"
end

desc "run nspec tests"
task :tests_excluding_performance => :build do
  sh @test_runner_command + " --tag ~performance"
end

desc "generates ctags"
task :tags do
  sh "ctags --recurse --exclude=\"Sample Apps\""
end
