modit('profile', ['growl'], function (growl) {
    var view = {
        handleHref: null,
        updateHandleButton: null,
        cancelHandleButton: null,
        handleTextBox: null,
        updateHandlePlaceHolder: null,
        init: function () {
            this.handleTextBox = $("<input id='handleTextBox' />");
            this.handleHref = $("#handle");
            this.updateHandleButton = $("<input type='button' value='update handle' />");
            this.cancelHandleButton = $("<input type='button' value='cancel' />");
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

        view.updateHandleButton.one('click', function () {
            var handle = view.handleTextBox.val();

            if(handle.indexOf('@') < 0) handle = '@' + handle;

            $.post(updateHandleUrl, { handle: handle }, function (d) {
                growl.info(d.Message);

                var handle = d.Handle;

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