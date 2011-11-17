(function() {
  var $createToolTip, toolTipTemplate, toolTips;
  toolTipTemplate = '\
	<div class="border dropshadow title" style="padding: 10px">{{html message}}</div>\
	';
  toolTips = {};
  $createToolTip = function(message) {
    return $.tmpl(toolTipTemplate, {
      message: message
    });
  };
  this.toolTip = {
    init: function($element, key, startingMessage, endingMessage, left, top) {
      var $toolTip;
      if (!toolTips[key]) {
        toolTips[key] = {
          startingMessage: startingMessage,
          endingMessage: endingMessage,
          currentMessage: startingMessage,
          messageCount: 0
        };
      }
      $toolTip = null;
      return $element.hover(function() {
        if (toolTips[key].messageCount > 3) {
          $toolTip = null;
          return;
        }
        if (toolTips[key].messageCount > 2) {
          toolTips[key].currentMessage = toolTips[key].endingMessage;
        }
        $toolTip = $createToolTip(toolTips[key].currentMessage);
        $("body").append($toolTip);
        $toolTip.hide();
        $toolTip.css({
          position: "absolute",
          left: left,
          top: top
        });
        $toolTip.fadeIn();
        return toolTips[key].messageCount++;
      }, function() {
        if ($toolTip) {
          return $toolTip.fadeOut(function() {
            return $toolTip.remove();
          });
        }
      });
    }
  };
}).call(this);
