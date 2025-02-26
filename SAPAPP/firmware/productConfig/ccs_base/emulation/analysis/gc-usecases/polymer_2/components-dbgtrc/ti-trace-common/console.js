(function () {
    var params = location.search.substr(1).split("&").map(p => {
        var tmp = p.split("=");
        return {name : tmp[0], value: tmp[1]};
    });
    var consoleAction = params.find(x => x.name === "consoleAction");
    if ((consoleAction == undefined) || (consoleAction.value == "undefined") || (consoleAction.value == "none")) {
        return;
    } else {
        var _log = console.log;
        var _error = console.error;
        var _warning = console.warning;

        if (consoleAction.value == "capture") {    
            console.error = function(msg) {
                console._pytest_log += msg + "\n";
                _error.apply(console,arguments);
            };
            console.log = function(msg) {
                console._pytest_log += msg + "\n";
                _log.apply(console, arguments);
            };
            console.warning = function(msg) {
                console._pytest_log += msg + "\n";
                _warning.apply(console, arguments);
            };
        } else if (consoleAction.value == "suppress") {
            console.error = function(msg) {
                _error.apply(console,arguments);
            };
            console.log = function(msg) {
            };
            console.warning = function(msg) {
                _warning.apply(console, arguments);
            };
        }
    }
})();