class GrowlNotifier
  def self.growl_path
    @@growl_path
  end
  
  def self.growl_path= value
    @@growl_path = value
  end

  def execute title, text, color
    return unless GrowlNotifier.growl_path

    text.gsub!('"', "''")

    text = text + "\n\n---"

    opts = ["\"#{GrowlNotifier.growl_path}\"", "\"#{text}\"", "/t:\"#{title}\""]

    opts << "/i:\"#{File.expand_path("#{color}.png")}\"" 

    `#{opts.join ' '}`
  end
end

class CommandShell
  def execute cmd
    puts cmd + "\n\n"

    str=""
    STDOUT.sync = true # That's all it takes...
    IO.popen(cmd+" 2>&1") do |pipe| # Redirection is performed using operators
      pipe.sync = true
      while s = pipe.gets
        str+=s # This is synchronous!
      end
    end
    
    puts str + "\n\n"

    str
  end
end


@sh = CommandShell.new

@growl = GrowlNotifier.new

GrowlNotifier.growl_path = 
  'C:\program files (x86)\Growl for Windows\growlnotify.exe'

watch('.*.\.cs$') do 
  results = @sh.execute "rake wip"

  if(!tests_passed? results)
    if(results =~ /FAILURES/)
      @growl.execute "sad panda", results[results.index("**** FAILURES ****")..-1], "red"
    else
      @growl.execute "sad panda", results, "red"
    end
  else
    @growl.execute "passed", "passed", "green"
  end
end


def tests_passed? results
  return /, 0 Failed/.match results.split('/n').last
end
