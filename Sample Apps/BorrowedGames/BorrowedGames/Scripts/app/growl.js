modit("growl", function () {
    var $growlArea;
    var $wrapper;

    function init() {
        $growlArea = $("<div class='growlinfo'></div>");
        $growlArea.hide();

        $wrapper = $("<div class='growlwrapper' style='display: none;'></div>");

        $wrapper.append($growlArea);

        $("body").append($wrapper);
    }

    function info(message, top) {
        if (!top) {
            top = "200px";
        }

        $wrapper.centerHorizontally();
        $wrapper.css({ top: top, "z-index": 9999 });
        $growlArea.show();
        $growlArea.html(message);
        $wrapper.fadeIn().delay(2000).fadeOut();
    }

    this.exports(info, init);
});