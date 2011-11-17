class FileSync
  attr_accessor :source, :destination

  def destination_file_name_for(file)
    return file.gsub(/^#{Regexp.quote(@source) }/i, @destination)
  end

  def sync(file)
    destination_file = destination_file_name_for file
    FileUtils.mkdir_p File.dirname(destination_file)
    FileUtils.cp file, destination_file
  end
end
