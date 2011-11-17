modit("toggle", function () {
    var leftPosition = '4px 2px';
    var rightPosition = '66px 2px';
    var onColor = '#46629D';
    var offColor = '#E0E5EE';

    function applyOnStyle($toggle) {
        applyOnLabelStyle($toggle);

        $toggle.css({
            'background-position': leftPosition,
            'background-color': onColor
        });
    }

    function applyOnLabelStyle($toggle) {
        var $toggleText = $getToggleText($toggle);

        $toggleText.html($toggle.attr('data-on'));

        $toggleText.css({
            'float': 'right',
            'color': '#FFFFFF'
        });
    }

    function $getToggleText($toggle) {
        return $toggle.find('label');
    }

    function applyOffStyle($toggle) {
        applyOffLabelStyle($toggle)

        $toggle.css({
            'background-position': rightPosition,
            'background-color': '#E0E5EE'
        });
    }

    function applyOffLabelStyle($toggle) {
        var $toggleText = $getToggleText($toggle);

        $toggleText.html($toggle.attr('data-off'));

        $toggleText.css({
            'float': 'left',
            'color': '#545454'
        });
    }

    function init($toggle, callback) {
        var value = $toggle.attr('data-value');

        $toggle.css({
            'background-image': "url('/content/images/circle20.png')",
            'background-repeat': 'no-repeat',
            'border': 'solid 1px black',
            'border-radius': '5px',
            'padding-left': '5px',
            'padding-right': '5px',
            'padding-top': '5px',
            'padding-bottom': '5px',
            'width': '80px',
            'height': '15px',
            'cursor': 'pointer'
        });

        var $toggleText = $('<label></label>');

        $toggle.append($toggleText);

        $toggleText.css({
            'font-weight': 'bold',
            'cursor': 'pointer'
        });

        if (value == "on") {
            applyOnStyle($toggle);
            $toggle.toggle(function () { toggleOff($toggle, callback); }, function () { toggleOn($toggle, callback); });
        }
        else {
            applyOffStyle($toggle);
            $toggle.toggle(function () { toggleOn($toggle, callback); }, function () { toggleOff($toggle, callback); });
        }
    }

    function setToggle($toggle, state) {
        if (value == "on") {
            toggleOn($toggle);
        }
        else {
            toggleOff($toggle);
        }
    }

    function toggleOn($toggle, callback) {
        var $text = $getToggleText($toggle);

        $text.hide();

        applyOnLabelStyle($toggle);

        $toggle.animate({
            backgroundPosition: leftPosition,
            backgroundColor: onColor
        },
        100,
        function () {
            $text.fadeIn();

            if (callback) {
                callback('on');
            }
        });
    }

    function toggleOff($toggle, callback) {
        var $text = $getToggleText($toggle);

        $text.hide();

        applyOffLabelStyle($toggle);

        $toggle.animate({
            backgroundPosition: rightPosition,
            backgroundColor: offColor
        },
        100,
        function () {
            $text.fadeIn();

            if (callback) {
                callback('off');
            }
        });
    }

    this.exports(init, setToggle);
});