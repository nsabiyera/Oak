require './StackTracePreview/stack_trace_preview.rb'

namespace :stacktrace do
  task :gen_preview => :rake_dot_net_initialize do
    path = "stacktrace.txt"

    puts "reading last error from #{path}"

    stacktrace = File.readlines(path).join()

    StackTracePreview.new(stacktrace, 'c:\Development\Oak').generate "StackTracePreview/stacktrace.html"

    puts "done, navigate to file:///C:/Development/Oak/StackTracePreview/stacktrace.html#/toc in your browser for the results"
  end
end
