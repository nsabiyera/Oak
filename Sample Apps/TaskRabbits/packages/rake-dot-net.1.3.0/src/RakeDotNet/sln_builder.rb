class SlnBuilder
  attr_accessor :msbuild_path

  def initialize
    @sh = CommandShell.new
    @msbuild_path = "#{ENV['SystemRoot']}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe"
  end

  def command(sln)
    return "\"#{@msbuild_path}\" \"#{sln}\" /verbosity:quiet /nologo"
  end

  def build(sln)
    Dir.glob("**/bin").each { |f| FileUtils.rm_rf f }
    Dir.glob("**/obj").each { |f| FileUtils.rm_rf f }

    @sh.execute command(sln)
  end

  def bins
    Dir.glob("**/bin")
  end

  def objs
    Dir.glob("**/obj")
  end
end
