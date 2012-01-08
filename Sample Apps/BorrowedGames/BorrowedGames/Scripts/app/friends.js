modit('friends', ['growl'], function (growl) {
    var view = {
        modal: null,
        showFriendsLink: null,
        closeFriendsButton: null,
        closeFriendsButtonTop: null,
        handleTextBox: null,
        addHandleButton: null,
        friendsContainer: null,
        handleAttribute: null,
        friendSelectorById: function (handle) { return 'div[' + this.handleAttribute + '="' + handle + '"]'; },
        init: function () {
            this.modal = $("#friends");
            this.showFriendsLink = $("#showFriends a");
            this.closeFriendsButton = $("#closeFriends");
            this.closeFriendsButtonTop = $("#closeFriendsTop");
            this.handleTextBox = $("#handleToAdd");
            this.addHandleButton = $("#addHandle");
            this.friendsContainer = $("#handles");
            this.handleAttribute = "data-handle";
        }
    };

    var addFriendUrl;
    var listFriendsUrl;
    var deleteFriendUrl;

    function init(urls) {
        addFriendUrl = urls.addFriendUrl;

        listFriendsUrl = urls.listFriendsUrl;

        deleteFriendUrl = urls.deleteFriendUrl;

        view.init();

        view.showFriendsLink.parent().click(function () { show(); });

        var closeLogic = function () {
            view.modal.fadeOut();
            if (view.friendsContainer.find("div").length != 0) {
                view.showFriendsLink.html('view users you are following');
            }
            else {
                view.showFriendsLink.html("you aren't following anyone, click to add");
            }
        };

        view.closeFriendsButton.click(closeLogic);

        view.closeFriendsButtonTop.click(closeLogic);

        view.addHandleButton.click(function () { addHandle(); });

        view.handleTextBox.bind('keypress', function (e) {
            var code = (e.keyCode ? e.keyCode : e.which);

            if (code == 13) addHandle();
        });
    }

    function show() {
        view.modal.centerHorizontally();
        view.modal.css({ top: view.modal.css("top") });
        view.modal.fadeIn();

        load();
    }

    function addHandle() {
        var handle = view.handleTextBox.val();

        if (handle.indexOf('@') < 0) handle = '@' + handle;

        $.post(addFriendUrl, { handle: handle },
        function (d) {
            if (d.Added) {
                view.friendsContainer.append($friendRecordFor(handle).hide().fadeIn('slow'));
            }

            view.handleTextBox.val('');
            growl.info(d.Message, view.handleTextBox.offset().top - 5);

            refreshGames();
        });
    }

    function $friendRecordFor(handle) {
        var $friend = $("<div class='border' style='height: 30px; clear: all'>" + handle + "</div>");
        $friend.attr(view.handleAttribute, handle);

        $links = $("<div style='float: right'></div>");

        $links.append($deleteLinkFor(handle));

        $friend.append($links);

        return $friend;
    }

    function $deleteLinkFor(handle) {
        var $deleteLink = $("<a href='javascript:;' style='text-decoration: none; color: black; display: block; margin: 5px;' class='delete'>remove</a>");
        $deleteLink.attr(view.handleAttribute, handle);

        $deleteLink.click(function () { removeHandle(handle); });

        return $deleteLink;
    }

    function removeHandle(handle) {
        $.post(deleteFriendUrl, { handle: handle }, function () {
            $friend = $findFriend(handle);

            $friend.fadeOut('fast', function () { $friend.remove(); });

            refreshGames();
        });
    }

    function $findFriend(handle) {
        return view.friendsContainer.find(view.friendSelectorById(handle));
    }

    function load() {
        view.friendsContainer.html('');

        $.getJSON(listFriendsUrl, function (handles) {
            for (var i in handles) {
                view.friendsContainer.append($friendRecordFor(handles[i]));
            }
        });
    }

    function refreshGames() {
      refreshPreferredGames();
      refreshWantedGames();
    }

    function refreshPreferredGames() {
        preferred.getPreferredGames();
    }

    function refreshWantedGames() {
        wanted.getWantedGames();
    }

    this.exports(init);
});
