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
        gameSelectorById: function (gameId) { return 'div[' + this.gameIdAttribute + '="' + gameId + '"]'; },
        init: function () {
            this.gameToAddTextBox = $("#gameToAdd");
            this.games = $("#games");
            this.modal = $("#library");
            this.showLibraryLink = $("#showLibrary a");
            this.closeLibraryButton = $("#closeLibrary");
            this.closeLibraryButtonTop = $("#closeLibraryTop");
            this.gameIdAttribute = "data-game-id";
            this.gamesSelector = "div[" + this.gameIdAttribute + "]";
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

        view.modal.hide();

        view.showLibraryLink.parent().click(function () { show(); });

        var closeLogic = function () {
            if (hasGames()) {
                view.showLibraryLink.html('view your game library');
            }
            else {
                view.showLibraryLink.html('looks like you have no games, click to add');
            }

            view.modal.fadeOut();
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
        growl.info(message, view.gameToAddTextBox.offset().top - 5);
    }

    function clearSearchBox() {
        view.gameToAddTextBox.val('');
    }

    function listGame(game) {
        view.games.append($gameRecordFor(game).hide().fadeIn('slow'));
    }

    function $gameRecordFor(game) {
        var $game = $("<div class='border' style='height: 30px; clear: all'>" + game.Name + "</div>")

        $game.attr(view.gameIdAttribute, game.Id);

        $links = $("<div style='float: right'></div>");

        $links.append($deleteLinkFor(game));

        $game.append($links);

        return $game;
    }

    function $deleteLinkFor(game) {
        var $deleteLink = $("<a href='javascript:;' style='text-decoration: none; color: black; display: block; margin: 5px;' class='delete'>remove</a>");
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
        view.modal.centerHorizontally();
        view.modal.css({ top: view.modal.css("top") });
        view.modal.fadeIn();

        load();
    }

    function $findGame(gameId) {
        return view.games.find(view.gameSelectorById(gameId));
    }

    function populateSearchResults(data) {
        autocomplete.populateSearchResults(data, "Id", "Name");
    }

    function clearGamesList() {
        view.games.children(view.gamesSelector).remove();
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