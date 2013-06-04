require './StackTracePreview/stack_trace_preview.rb'

namespace :stacktrace do
  task :gen_preview => :rake_dot_net_initialize do
    path = @website_deploy_directory + "\\stacktrace.txt"

    puts "reading last error from #{path}"

    stacktrace = File.readlines(path).join()

    StackTracePreview.new(stacktrace, 'c:\Development\Oak\Sample Apps\BorrowedGames').generate "StackTracePreview/stacktrace.html"

    puts "done, navigate to file:///C:/Development/Oak/Sample Apps/BorrowedGames/StackTracePreview/stacktrace.html#/toc in your browser for the results"
  end
end
