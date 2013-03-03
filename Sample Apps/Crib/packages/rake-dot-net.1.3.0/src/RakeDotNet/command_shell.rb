class CommandShell
  def execute cmd
    puts cmd
    str = ""
    status = 0

    STDOUT.sync = true
    IO.popen(cmd + " 2>&1") do |pipe| 
      pipe.sync = true
      while s = pipe.gets
        str += s
      end
    end

    puts str

    raise RuntimeError, STDERR if !$?.success?
    
    return str
  end
end
