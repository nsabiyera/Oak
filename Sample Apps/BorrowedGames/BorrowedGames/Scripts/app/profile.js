modit('profile', ['growl'], function (growl) {
    var view = {
        handleHref: null,
        updateHandleButton: null,
        cancelHandleButton: null,
        handleTextBox: null,
        updateHandlePlaceHolder: null,
        init: function () {
            this.handleTextBox = $("<input type='text' id='handleTextBox' class='span4' autocomplete='off' style='margin-left: 3px' />");
            this.handleHref = $("#handle");
            this.updateHandleButton = $("<input type='button' value='update handle' class='btn btn-primary' style='margin-left: 3px' />");
            this.cancelHandleButton = $("<input type='button' value='cancel' class='btn' style='margin-left: 3px' />");
            this.updateHandlePlaceHolder = $("#updatePlaceHolder");
        }
    };

    var updateHandleUrl;

    function init(urls) {
        updateHandleUrl = urls.updateHandleUrl;

        view.init();

        wireUpEdit();
    }

    function wireUpEdit() {
        view.handleHref.one('click', edit);
    }

    function edit() {
        var currentHandle = $(this).html();

        $(this).html('');

        view.updateHandlePlaceHolder
            .append(view.handleTextBox.val(currentHandle))
            .append(view.updateHandleButton)
            .append(view.cancelHandleButton);

        view.handleTextBox.keydown(
            function(e) {
              if(e.keyCode == 13) {
                e.preventDefault();
                view.updateHandleButton.click();
              }
            });
            
        view.updateHandleButton.one('click', function () {
            var handle = view.handleTextBox.val();

            if(handle.indexOf('@') < 0) handle = '@' + handle;

            $.post(updateHandleUrl, { handle: handle }, function (d) {
                growl.info(d.message);

                var handle = d.handle;

                if (!handle) handle = "@nameless";

                view.handleHref.html(handle);
            });

            view.updateHandlePlaceHolder.empty();

            wireUpEdit();
        });

        view.cancelHandleButton.one('click', function () {
            view.updateHandlePlaceHolder.empty();

            view.handleHref.html(currentHandle);

            wireUpEdit();
        });
    }

    this.exports(init);
});
