require './watcher_dot_net.rb'

@notifier = GrowlNotifier.new
GrowlNotifier.growl_path = 'C:\Program Files (x86)\Growl for Windows\growlnotify.exe'

@sh = CommandShell.new

@test_runner = NSpecRunner.new "."

watch ('.*.\.cs$') do |md|
  file = md[0]

  result = build_project_related_to file 

  if(build_failed_based_on result)
    growl "Build Failed", result, "red"
  else
    run_tests_for file

    if (!@test_runner.failed && is_associated_with_web_project(file))
      @sh.execute "rake" 
      growl "Website", "Website deployed", "green"
    end
  end
end

watch ('(.*.csproj$)|(.*.sln$)') do |md| 
  reload_specwatchr_to_pick_up_new_files md[0]

  build_project_related_to md[0]
end

watch ('(.*.cshtml)|(.*.js)|(.*.css)$') do |md| 
  failed = false

  if(md[0].match /App_Code/)
    @sh.execute "rake"
  else
    output = @sh.execute "rake sync[\"#{ md[0] }\"]"

    failed = true if output =~ /rake aborted!/
  end

  growl "website deployed", "deployed", "green" unless failed

  growl "sync failed", 
        "it looks like the sync failed, this usually happens if the version of rake you are running is NOT 0.8.7.  Please ensure you are running version 0.8.7 of rake. To see the gem versions that are installed, run the command 'gem list' in a command prompt that supports ruby.",
        "red" if failed
end

watch ('(.*.scss)$') do |md|
  @sh.execute "compass compile BorrowedGames"
end

watch ('(.*.coffee)$') do |md|
  @sh.execute "rake coffee --trace"
end

def reload_specwatchr_to_pick_up_new_files file
  growl "reloading", "Reloading SpecWatchr because #{file} changed.", "green"

  @sh.execute "rake"

  FileUtils.touch "dotnet.watchr.rb"
end

def build_project_related_to file
  project = project_name_for file

  build project
end

def project_name_for file
  directory = directory_for file
  return directory + "/" + directory + ".csproj"
end

def directory_for file
  return file.split('/').first
end

def build project_or_solution
  return @sh.execute "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\MSBuild.exe #{ project_or_solution } /verbosity:quiet /nologo"
end

def growl title, message, color
  @notifier.execute title, message, color
end

def build_failed_based_on result
  return result.match /error/
end

def run_tests_for file
  output = ""

  if is_associated_with_web_project file 
    output = build "BorrowedGames.Tests/BorrowedGames.Tests.csproj" 
  end

  if(build_failed_based_on(output))
    growl "build failed", output, "red"

    @test_runner.failed = true
  else
    test_category = @test_runner.find file

    test_output = @test_runner.execute test_category

    growl "no test found", "create spec #{ spec }", "red" if @test_runner.inconclusive

    growl "tests failed", @test_runner.first_failed_test, "red" if @test_runner.failed

    test_category = "" if test_category.nil?

    growl "all passed in test category: " + test_category, test_output.split("\n").last , "green" if !@test_runner.failed and !@test_runner.inconclusive
  end
end

def is_associated_with_web_project file
  return true if project_name_for(file) == "BorrowedGames/BorrowedGames.csproj"
end
