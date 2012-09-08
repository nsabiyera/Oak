modit("growl", function () {
    var $growlArea;
    var $wrapper;

    function init() {
        $growlArea = $("<div class='growlinfo' style='font-size: 20px'></div>");
        $growlArea.hide();

        $wrapper = $("<div class='alert alert-info span6' style='display: none;'></div>");

        $wrapper.append($growlArea);

        $("body").append($wrapper);
    }

    function info(message, top, icon) {
        if(!top) { top = "55px"; }

        if(!icon) { icon = "icon-ok-sign"; }
            
        $wrapper.css({ top: top, "position": "absolute", "z-index": 99999 });
        $growlArea.show();
        $growlArea.html(message);
        $wrapper.centerHorizontally();
        $wrapper.fadeIn().delay(2000).fadeOut();
        $wrapper.css("left", ($wrapper.position().left - 10) + "px");
    }

    this.exports(info, init);
});
