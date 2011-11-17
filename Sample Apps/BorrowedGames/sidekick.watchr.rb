@code_files = '.*.\.cs$'
@project_files = '(.*.csproj$)|(.*.sln$)'
@web_files = '(.*.cshtml)|(.*.js)|(.*.css)$'

watch (@code_files) do |md|
  build_test_and_deploy md[0], { :run_only_impacted_tests => false }
end

watch (@project_files) do |md| 
  reload_sidekick md[0]
end

watch (@web_files) do |md|
  deploy_web_file md[0]

  growl_success "File synced", md[0] 
end

def build_test_and_deploy file, options
  build_solution
  
  growl_failure "Build Failed", @error if @failed

  if @succeeded
    run_tests :all

    growl_failure "Tests Failed", @error if @failed

    if @succeeded
      growl_success "Tests Passed", @stats 

      deploy_web_project
    end
  end
end

def reload_sidekick file
  growl "reloading", "Reloading sidekick because #{file} changed.", "green"

  FileUtils.touch "sidekick.watchr.rb"
end

def build_solution
  rake :default

  if(@output.match /Errors/) @build_failed = true
end

def rake task
  @output = sh("rake #{ task.to_s }")
end
