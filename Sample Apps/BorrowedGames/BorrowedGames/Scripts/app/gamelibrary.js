modit('gamelibrary', ['growl', 'autocomplete'], function (growl, autocompletes) {
    var view = {
        gameToAddTextBox: null,
        games: null,
        modal: null,
        showLibraryLink: null,
        closeLibraryButtonTop: null,
        closeLibraryButton: null,
        gameIdAttribute: null,
        gamesSelector: null,
        gameSelectorById: function (gameId) { return 'tr[' + this.gameIdAttribute + '="' + gameId + '"]'; },
        init: function () {
            this.gameToAddTextBox = $("#gameToAdd");
            this.games = $("#games");
            this.modal = $("#library");
            this.showLibraryLink = $("#showLibrary");
            this.closeLibraryButton = $("#closeLibrary");
            this.closeLibraryButtonTop = $("#closeLibraryTop");
            this.gameIdAttribute = "data-game-id";
            this.gamesSelector = "td[" + this.gameIdAttribute + "]";
        }
    };

    var searchUrl;
    var addGameUrl;
    var removeGameUrl;
    var libraryUrl;

    function init(urls) {
        searchUrl = urls.searchUrl;

        addGameUrl = urls.addGameUrl;

        removeGameUrl = urls.removeGameUrl;

        libraryUrl = urls.libraryUrl;

        view.init();

        view.modal.modal('hide');

        view.modal.on('hide', function () {
            autocomplete.inactivate();
        });

        view.showLibraryLink.click(function () { show(); });

        var closeLogic = function () {
            if (hasGames()) {
                view.showLibraryLink.html('view your game library');
                view.showLibraryLink.removeClass("btn-danger").addClass("btn-info")
            }
            else {
                view.showLibraryLink.html('looks like you have no games, click to add');
                view.showLibraryLink.removeClass("btn-info").addClass("btn-danger")
            }

            view.modal.modal('hide');
        };

        view.closeLibraryButton.click(closeLogic);

        view.closeLibraryButtonTop.click(closeLogic);

        autocomplete.init(
            view.gameToAddTextBox,
            function (searchString) { search(searchString); },
            function (id) { gameSelected(id); }
            );
    }

    function hasGames() {
        return view.games.find(view.gamesSelector).length > 0;
    }

    function notify(message) {
        growl.info(message, view.gameToAddTextBox.offset().top - 5, "icon-info-sign");
    }

    function clearSearchBox() {
        view.gameToAddTextBox.val('');
    }

    function listGame(game) {
        view.games.append($gameRecordFor(game).hide().fadeIn('slow'));

    }

    function $gameRecordFor(game) {
        var $game = $("<td style='width: 100%'>" + game.Name +  " (" + game.Console + ")" + "</td>")

        $game.attr(view.gameIdAttribute, game.Id);

        $links = $("<td></td>");

        $links.append($deleteLinkFor(game));

        return $("<tr></tr>").attr(view.gameIdAttribute, game.Id).append($game).append($links);
    }

    function $deleteLinkFor(game) {
        var $deleteLink = $("<a href='javascript:;' class='btn btn-danger'>remove</a>");
        $deleteLink.attr(view.gameIdAttribute, game.GameId);

        $deleteLink.click(function () { gameRemoved(game); });

        return $deleteLink;
    }

    function removeGame(gameId) {
        $game = $findGame(gameId);

        $game.fadeOut('fast', function () { $game.remove(); });

        refreshPreferredGames();
    }

    function refreshPreferredGames() {
        preferred.getPreferredGames();
    }

    function show() {
        view.modal.modal('show');

        autocomplete.activate();

        load();
    }

    function $findGame(gameId) {
        return view.games.find(view.gameSelectorById(gameId));
    }

    function populateSearchResults(data) {
        autocomplete.populateSearchResults(data, "id", function(item) { return item.name + " (" + item.console + ")" });
    }

    function clearGamesList() {
        view.games.children().remove();
    }

    function gameRemoved(game) {
        var gameId = game.Id;

        removeGameFromHaveList(gameId, function () { removeGame(gameId); });
    }

    function removeGameFromHaveList(gameId, callback) {
        $.post(removeGameUrl, { gameId: gameId }, callback);
    }

    function addGameToHaveList(gameId, callback) {
        if ($findGame(gameId).length > 0) notify("This game is already in your library.");

        else $.post(addGameUrl, { gameId: gameId }, callback);
    }

    function load() {
        clearGamesList();
        $.getJSON(libraryUrl, function (data) {
            for (var game in data) {
                listGame(data[game]);
            }
        });
    }

    function search(searchString) {
        $.getJSON(searchUrl, { searchString: searchString }, populateSearchResults);
    }

    function gameSelected(gameId) {
        if (!gameId) {
            notify("This game doesn't exist in our database.");
            return;
        }

        var callback = function (data) {
            clearSearchBox();
            listGame(data);
            refreshPreferredGames();
        };

        addGameToHaveList(gameId, callback);
    }

    this.exports(init);
});
