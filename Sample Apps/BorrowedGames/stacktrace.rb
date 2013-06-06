require './StackTracePreview/stack_trace_preview.rb'

namespace :stacktrace do
  desc "generates a stack trace preview from the last error in IISExpress"
  task :gen_preview => :rake_dot_net_initialize do
    gen_preview(@website_deploy_directory + "\\stacktrace.txt")
  end

  desc "generates a stack trace preview from the last execution of stacktrace:tests or specwatchr"
  task :gen_tests_preview => :rake_dot_net_initialize do
    gen_preview "stacktrace.txt" 
  end

  desc "runs tests and writes any failures to stacktrace.txt so that stacktrace:gen_tests_preview can be executed"
  task :tests do
    results = `#{@test_runner_command}`

    puts results

    write_stack_trace results
  end

  def gen_preview from_file
    puts "reading last error from #{from_file}"

    stacktrace = File.readlines(from_file).join()

    StackTracePreview.new(stacktrace, 'c:\Development\Oak\Sample Apps\BorrowedGames').generate "StackTracePreview/stacktrace.html"

    puts "done, navigate to file:///C:/Development/Oak/Sample Apps/BorrowedGames/StackTracePreview/stacktrace.html#/toc in your browser for the results"
  end

  def write_stack_trace test_output
    stacktrace = ""

    if test_output.match /FAILURES/
      stacktrace = test_output.split('**** FAILURES ****').last.strip
      stacktrace = stacktrace.split(/^.*Examples, /).first.strip
    end

    File.open("stacktrace.txt", 'w') { |f| f.write(stacktrace) }
  end
end
