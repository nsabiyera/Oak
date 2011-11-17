jQuery.fn.centerHorizontally = function () {
    $(this).css(
    {
        position: "absolute",
        left: (($("html").width() / 2) - ($(this).width() / 2)) + "px"
    });
}