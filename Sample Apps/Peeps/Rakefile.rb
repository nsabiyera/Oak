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

task :https_server do
  start_iis_express_with_config "applicationHost.config"
end

desc "deletes all http and https public access and deletes cert registration"
task :delete_https do
  delete_cert
  begin
    sh "netsh http delete urlacl url=https://#{computer_name}:443/"
  rescue
  end
  begin
    sh "netsh http delete sslcert ipport=0.0.0.0:443"
  rescue
  end
end

desc "initializes a cert and configures your computer to run an mvc app under https with a trusted certificate"
task :init_https do
  delete_443_url_reservations
  create_cert
  sh "netsh advfirewall firewall add rule name=\"IISExpressWeb\" dir=in action=allow protocol=TCP localport=80"
  thumbprint = cert_thumbprint
  sh "netsh http add sslcert ipport=0.0.0.0:443 appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certhash=#{thumbprint}"
  sh "netsh http add urlacl url=https://#{ computer_name }:443/ user=everyone"
  move_cert_to_trusted
end

def delete_443_url_reservations
  reservation_pattern = /    Reserved URL            :/
  name = `netsh http show urlacl`
  name.each_line do |l|
    if l =~ reservation_pattern && l =~ /:443/
      sh "netsh http delete urlacl url=#{ l.gsub(reservation_pattern, "") }"
    end
  end
end

def create_cert
  sh "\"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v7.0A\\Bin\\makecert.exe\" -r -pe -n \"CN=#{computer_name}\" -b 01/01/2000 -e 01/01/2036 -eku 1.3.6.1.5.5.7.3.1 -ss my -sr localMachine -sky exchange -sp \"Microsoft RSA SChannel Cryptographic Provider\" -sy 12"
end

def move_cert_to_trusted
  cd "RakeDotNet\\CertScripts"
  sh "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe move_cert.cs"
  sh "move_cert.exe #{computer_name}"
  cd "..\\.."
end

def delete_cert
  cd "RakeDotNet\\CertScripts"
  sh "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe delete_cert.cs"
  sh "delete_cert.exe #{computer_name}"
  cd "..\\.."
end

def cert_thumbprint
  cd "RakeDotNet\\CertScripts"
  sh "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe cert_thumbprint.cs"
  thumbprint = `cert_thumbprint.exe #{computer_name}`.chomp!
  cd "..\\.."
  return thumbprint
end

def computer_name
  name = `HOSTNAME`
  name.chomp!
  return name
end

def start_iis_express_with_config config
  return unless File.exists? "dev.yml"

  yml = YAML::load File.open("dev.yml")

  execution_path = yml["iis_express"]

  create_app_host_config_from_template

  sh "start /d\"#{ execution_path }\" /MIN iisexpress /config:\"#{ pwd.gsub("/", "\\") + "\\" + config }\""
end

def create_app_host_config_from_template
  File.chmod(0777, "applicationhost.config.template")
  content = File.read("applicationhost.config.template")
  newcontent = content.gsub /COMPUTER_NAME/, computer_name
  
  File.open("applicationhost.config", 'w') { |f| f.write(newcontent) }
end
