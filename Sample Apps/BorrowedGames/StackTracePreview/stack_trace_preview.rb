require 'json'
require 'securerandom'
require 'cgi'

class StackTracePreview
  def initialize stacktrace, working_directory
    @stacktrace = stacktrace
    @working_directory = working_directory
  end

  def models_for stacktrace, working_directory
    all_models = Array.new
    model = nil
    was_in_stack_trace = true
    stacktrace.each_line do |l|
      if(l.strip.match /^at/)
        was_in_stack_trace = true
        hash = {
          :id => SecureRandom.uuid.to_s,
          :description => l.strip,
          :method => "",
          :file => "",
          :detail => "",
          :no_lines => true
        }
        file_part = hash[:description].split(" in ").last
        hash[:method] = hash[:description].split(" in ").first.gsub /^at /, ""
        tokens = file_part.split(":line ")
        hash[:file] = { :path => tokens[0], :line => tokens[1].to_i }
        hash[:detail] = lines_from(hash[:file][:path], hash[:file][:line])
        hash[:no_lines] = hash[:detail].empty?
        model[:errors] << hash
      else
        if(was_in_stack_trace == true)
          model = Hash.new
          model[:errors] ||= Array.new
          model[:working_directory] = working_directory
          all_models << model
        end

        was_in_stack_trace = false
        model[:header] ||= Array.new
        model[:header] << l.strip if l.strip != ""
      end
    end
    all_models
  end

  def generate path
    models = models_for(@stacktrace, @working_directory)

    table_of_contents = table_of_contents_for(models[0..4])

    all_slides = ""

    models[0..4].each_with_index { |model, i| all_slides += slides_for(model, i * 1500) }

    html = html_page table_of_contents, all_slides

    File.open(path, 'w') { |f| f.write(html) }
  end

  def lines_from path, line
    return Array.new if !File.exists? path 

    line_range = 10

    starting_line = line - line_range

    starting_line = line if line < line_range

    array = File.readlines(path)[starting_line..line + line_range] || Array.new
    
    array 
      .each_with_index
      .map do |l, i| {
        :number => starting_line + i,
        :line => l,
        :trace_line => (starting_line + 1) + i == line || (i + 1 == line && line < line_range),
        :is_empty => l.strip.length == 0
      }
    end
  end

  def table_of_contents_for models
    links = ""

    models.each do |model|
      links += "<li style='margin-top: 10px; font-size: 18px'>#{model[:header].join('<br />')}<ol>"

      model[:errors].each do |kvp|
        if(kvp[:no_lines])
          links += "<li>#{kvp[:file][:path].gsub(model[:working_directory], "")} - #{kvp[:file][:line]}</li>"
        else
          links += "<li><a href=\"##{ kvp[:id] }\">#{kvp[:file][:path].gsub(model[:working_directory], "")} - #{kvp[:file][:line]}</a></li>"
        end
      end

      links += "</li></ol>"
    end

    toc = <<here 
    <div id="toc" class="step slide" data-x="#{ (models.count * 200) }" data-y="-3000" data-z="-500">
      <h1 style="font-size: 20px !important">Stack Trace Preview [use arrow keys to navigate]
        <div style="float: right; padding: 3px; font-size: 14px">
          about: <a href="http://amirrajan.github.io/StackTracePreview/#/toc" target="_blank">Awesome sample to show others</a> -
                 <a href="http://github.com/amirrajan/StackTracePreview" target="_blank">Fork me on GitHub</a> - 
                 <a href="http://amirrajan.github.com/Oak" target="_blank">An integral part of Oak</a>
        </div>
      </h1>
      <hr>
      <ul id="toc-ol" style="list-style-type: decimal; font-family: courier">
       #{ links }
      </ul>
    </div>
here
    
    toc
  end

  def calc_common_white_space strings
    min_white_space = 1000

    strings.each do |s|
      if(s.strip.length != 0)
        white_space_for_line = s.length - s.lstrip.length

        min_white_space = white_space_for_line if white_space_for_line < min_white_space
      end
    end

    min_white_space
  end

  def slides_for model, x
    formatted = ""

    model[:errors].each do |kvp|
      min_white_space = calc_common_white_space kvp[:detail].map { |d| d[:line] }

      kvp[:detail].each do |detail|
        detail[:min_leading_white_space] = min_white_space
      end
    end

    i = 0

    model[:errors].reject { |kvp| kvp[:no_lines] }.each do |kvp|
      formatted += <<here
      <div id="#{ kvp[:id] }" class="step slide code" data-x="#{x}" data-y="-#{ i * 600 }" data-z="-#{ i * 300 }">
        <div style="float: right; margin-right: 20px; font-size: small"><a href="#toc">toc</a></div>
        <pre class="test-results" style="font-family: courier">
#{kvp[:file][:path].gsub(model[:working_directory], "")} - #{kvp[:file][:line]}
        </pre>
        <pre class="diff" style="font-family: courier">
#{kvp[:detail].map do |kvp|
      line = CGI::escapeHTML(kvp[:number].to_s + kvp[:line])

      if(!kvp[:is_empty])
        line = CGI::escapeHTML(kvp[:number].to_s + "  " + kvp[:line][kvp[:min_leading_white_space]..kvp[:line].length])
      end

      if kvp[:trace_line]
        "<b style='color: #FA8072'>" + line + "</b>"
      else
        line
      end
  end.join()}
        </pre>
      </div>
here
      i += 1
    end

    formatted
  end


  def html_page table_of_contents, slides
return <<Impress
<!DOCTYPE html>
<html lang="en">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<meta charset="utf-8">
<meta name="viewport" content="width=1024">
<meta name="apple-mobile-web-app-capable" content="yes">
<title>Stack Trace Preview</title>
<meta name="description" content="impress.js is a presentation tool based on the power of CSS3 transforms and transitions in modern browsers and inspired by the idea behind prezi.com.">
<meta name="author" content="Bartek Szopka">
<link href="http://fonts.googleapis.com/css?family=Open+Sans:regular,semibold,italic,italicsemibold%7CPT+Sans:400,700,400italic,700italic%7CPT+Serif:400,700,400italic,700italic" rel="stylesheet">
<link href="stylesheets/impress-demo.css" rel="stylesheet">
<link rel="shortcut icon" href="favicon.png">
<link rel="apple-touch-icon" href="apple-touch-icon.png">
</head>
<body class="impress-not-supported">

<div class="fallback-message">
    <p>Your browser <b>doesn't support the features required</b> by impress.js, so you are presented with a simplified version of this presentation.</p>
    <p>For the best experience please use the latest <b>Chrome</b>, <b>Safari</b> or <b>Firefox</b> browser.</p>
</div>

<div id="impress">
    #{table_of_contents}
    #{slides}
</div>

<div class="hint">
    <p>Use a spacebar or arrow keys to navigate</p>
</div>
<script>
if ("ontouchstart" in document.documentElement) { 
    document.querySelector(".hint").innerHTML = "<p>Tap on the left or right to navigate</p>";
}
</script><script src="javascripts/impress.js"></script><script src="javascripts/jquery-1.6.2.min.js"></script><link type="text/css" href="stylesheets/jquery.jsscrollpane.css" rel="stylesheet" media="all">
<script type="text/javascript" src="javascripts/jquery-1.6.2.min.js"></script><script type="text/javascript" src="javascripts/jquery.jsscrollpane.min.js"></script><script type="text/javascript" src="javascripts/jquery.mousewheel.js"></script><script type="text/javascript" src="javascripts/prettydiff.js"></script><script>
  impress().init();
  $(document).ready(function(){
    $('.slide').jScrollPane({
    horizontalGutter:5,
    verticalGutter:5,
    'showArrows': false
    });

    $('.comment').jScrollPane({
    horizontalGutter:5,
    verticalGutter:5,
    'showArrows': false
    });
  });
</script>
</body>
</html>
Impress
  end
end

