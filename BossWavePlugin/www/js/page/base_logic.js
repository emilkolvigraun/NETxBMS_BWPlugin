var hub = $.connection.hub;
var loghub = $.connection.logHub;
var actionhub = $.connection.actionHub;

hub.logging = true;
hub.start().done(function () {
    actionhub.server.loadSubscriptions();
    actionhub.server.loadPublishes();
    actionhub.server.toggleEntity();
    loghub.server.activateLogging();
    console.log("Connection established.");
}).fail(function (reason) {
    console.log("Connection failed: " + reason + ".");
});



actionhub.client.entityHasBeenSet = function (path) {  
    var label = $("#current-entity")[0];
    label.innerHTML = path;
}

actionhub.client.inform = function (message) {
    console.log(message);
}

actionhub.client.updateSub = function (subs) {
    var subscribe_list = $("#sub-pages")[0];
    subscribe_list.innerHTML = '';
    for (var key in subs){     
        subscribe_list.innerHTML += '<li><a class="sub" onclick="load_sub(this.innerHTML)">'+subs[key]+'</a></li>';
    }
}

actionhub.client.updatePub = function (pubs) {
    var publish_list = $("#pub-pages")[0];
    publish_list.innerHTML = '';
    for (var key in pubs){   
        publish_list.innerHTML += '<li><a class="sub" onclick="load_sub(this.innerHTML)">'+pubs[key]+'</a></li>';
    }
}
