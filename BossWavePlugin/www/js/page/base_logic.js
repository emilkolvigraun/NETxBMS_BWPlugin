    var hub = $.connection.hub;
    var actionhub = $.connection.actionHub;


    hub.logging = true;
    hub.start().done(function () {
        console.log("SignalR connection established.");
        actionhub.server.inform("client connected.");
    }).fail(function (reason) {
        console.log("SignalR connection failed: " + reason + ".");
    });


actionhub.client.inform = function (message) {
    console.log(message);
}
