modit('autocomplete', function () {
    var $auto;
    var $results;
    var activeIndex;
    var onSearch;
    var onSelected;
    var lastSearchResults;
    var currentId;

    function init(target, onSearch_, onSelected_) {
        activeIndex = -1
        $auto = target;
        onSearch = onSearch_;
        onSelected = onSelected_;

        $auto.keydown(
            function (e) {
                switch (e.keyCode) {
                    case 38: // up
                        moveUp();
                        e.preventDefault();
                        break;
                    case 40: // down
                        moveDown();
                        e.preventDefault();
                        break;
                    case 13: // return
                        e.preventDefault();
                        determineSelection();
                        break;
                }
            });

        $auto.keyup(
            function (e) {
                switch (e.keyCode) {
                    case 38: // up
                    case 40: // down
                    case 9:  // tab
                    case 13: // return
                        break;
                    default:
                        performSearch();
                        break;
                }
            });

        lastSearchResults = [];

        initializeResults();
    }

    function performSearch() {
        var value = $auto.val();
        if (value) {
            onSearch(value);
        }
        else {
            $results.html('');
            $results.css({
                width: "0px",
                top: "0px",
                left: "0px",
                "z-index": "99999"
            });
        }
    }

    function populateSearchResults(data, key, value) {
        if(!active) {
            $results.html("");
            return;
        }

        lastSearchResults = data;
        var element = $auto;
        $results.html('');
        var finalHtml = "<table class='table table-bordered' style='background-color: white'>";

        var currentValue = $(element).val().toLowerCase();
        for (index = 0; index < data.length; index++) {
            var dataItem = data[index];
            var dataItemValue = value(dataItem);
            var name = dataItemValue;
            var indexOf = name.toLowerCase().indexOf(currentValue);
            if (indexOf != -1 && currentValue.length >= 1) {
                name = dataItemValue.substr(0, indexOf)
                                    + '<strong>' + dataItemValue.substr(indexOf, currentValue.length)
                                    + '</strong>' + dataItemValue.substr(indexOf + currentValue.length);
            }

            finalHtml += '<tr id="' + dataItem[key] + '" style="cursor: pointer;"><td>' + name + '</td></tr>';
        }

        finalHtml += '</table>';

        $results.append(finalHtml);
        $results.find('tr').each(
            function () {
                $(this).hover(
                function () {
                    whenHover(this);
                });

                $(this).click(
                function () {
                    selectionMade($(this).attr('id'));
                });
            });

        // get the position of the input field right now (in case the DOM is shifted)
        var pos = findPos(element);
        // either use the specified width, or autocalculate based on form element
        var iWidth = $(element).width();

        // reposition
        $results.css({
            width: (parseInt(iWidth) + 2) +"px",
            top: (pos.y + $(element).height() + 10) + "px",
            left: (pos.x + 8) + "px",
            "z-index": "9999"
        });
    }

    function findPos(obj) {
        return { x: $(obj).offset().left, y: $(obj).offset().top };
    }

    function moveUp() {
        if (activeIndex > 0) {
            activeIndex -= 1;
        }

        highlightValue();
    }

    function moveDown() {
        var count = $results.find("tr").length;

        if (activeIndex < 0) {
            activeIndex = 0;
        }
        else {
            activeIndex += 1;
        }
        if (activeIndex == count) {
            activeIndex = count - 1;
        }

        highlightValue();
    }

    function whenHover(element) {
        $results.find("tr").each(
            function (index) {
                $(this).removeClass('current');
                $(this).css({ "background-color": "white" })
            });

        $(element).addClass('current');
        currentId = $(element).attr('id');
    }

    function highlightValue() {
        $results.find("tr").each(
            function (index) {
                if (index == activeIndex) {
                    currentId = $(this).attr('id');
                    $(this).addClass('current');
                    $(this).css({ "background-color": "whitesmoke" })
                }
                else {
                    $(this).removeClass('current');
                    $(this).css({ "background-color": "white" })
                }
            });
    }

    function initializeResults() {
        results = document.createElement("div");
        $results = $(results).attr('id', 'autocompletesearchresults');
        $results.css('position', 'absolute');
        $results.css({ width: "0px", top: "0px", left: "0px" });
        $("body").append(results);
    }

    function determineSelection() {
        if (lastSearchResults.length == 0) {
            currentId = null;
            onSelected(currentId);
            return;
        }

        if (activeIndex == -1) {
            currentId = lastSearchResults[0].Key;
        }

        selectionMade(currentId);
    }

    function selectionMade(id) {
        onSelected(currentId);
        $results.html('');
        activeIndex = -1;
    }

    var active = true;
    function inactivate() {
        active = false;
    }

    function activate() {
        active = true;
    }

    this.exports(init, populateSearchResults, inactivate, activate);
});
