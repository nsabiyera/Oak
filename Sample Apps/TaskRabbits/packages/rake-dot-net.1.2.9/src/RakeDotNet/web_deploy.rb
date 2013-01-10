require 'fileutils'

class WebDeploy
  attr_accessor :asp_compiler_path

  def initialize()
    @asp_compiler_path = "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\aspnet_compiler.exe"
    @destination = "c:\\inetpub\\wwwroot"
    @command_shell = CommandShell.new
  end

  def deploy(source, destination, delete_after = nil, delete_before = nil)
    raise RuntimeError, "source (web project directory) doesn't exists or has not been specified" if !source? source

    FileUtils.rm_rf(destination)

    delete_before.each { |f| FileUtils.rm_rf in_dir(source, f) } if !delete_before.nil?

    @command_shell.execute(command(source, destination))

    FileUtils.rm project_files(destination)

    FileUtils.rm_rf(in_dir(destination, "Properties"))

    delete_after.each { |f| FileUtils.rm_rf in_dir(destination, f) } if !delete_after.nil?
  end

  def project_files(destination)
    return Dir.glob(in_dir(destination, "*.csproj*"))
  end

  def in_dir(destination, pattern)
    return File.join(destination, pattern)
  end

  def source? directory
    return File.directory? directory
  end

  def command(source, destination)
    return "\"#{asp_compiler_path}\" \"#{destination}\" -u -v \"\/\" -p \"#{source}\""
  end
end
