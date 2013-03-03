class IISExpress
  attr_accessor :execution_path

  def initialize
    @execution_path = "C:\\Program Files (x86)\\IIS Express"

    @sh = CommandShell.new
  end

  def iis_express?
    return File.exists? @execution_path
  end

  def command(path, port)
    return "start /d\"#{ execution_path }\" /MIN iisexpress /path:\"#{ path }\" /port:#{ port }"
  end
end
