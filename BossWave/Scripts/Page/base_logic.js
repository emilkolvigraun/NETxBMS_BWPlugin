

    var hub = $.connection.hub;
    hub.logging = true;
    hub.start().done(function () {
        console.log("SignalR connection established.");
        $.connection.actionHub.server.announce("sending msg");
    }).fail(function (reason) {
        console.log("SignalR connection failed: " + reason + ".");
    });


$.connection.actionHub.client.announce = function (message) {
}
