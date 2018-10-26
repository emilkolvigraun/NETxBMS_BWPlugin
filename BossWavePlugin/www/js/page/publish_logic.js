var actionhub = $.connection.actionHub;

actionhub.client.getPubItem = function (message) {
    console.log(message);
}

function load_pub(title){
    actionhub.server.getItemInfo("pub", title)
}

$(".add-pub").on("click", function () {
    var title = $("#publish-title")[0].value
    var namespace = $("#namespace-pub")[0].value
    var message = $("#message-pub")[0].value
    var loop = $("#repeat-publish")[0].checked
    var chain = $("#accesschain")[0].value
    var ms = $("#loop-input")[0].value
    var label = $("#error-label-pub")[0]
    var id = "blah"

    
    label.innerHTML = "<br><br><br><br>"
    if (title == '' || namespace == '' || message == '' || chain == ''){
        label.innerHTML += "You need to enter a title and a namespace<br>";
    } else {
        pub_info = '{ "itemid": "' + id + '", "title": ' + title + '", "message": "' + message + '", "loop": "' + loop + '", "chain": "' + chain + '", "namespace": "' + namespace + '", "ms": "' + ms + '" }'

        actionhub.server.createPublishing(pub_info)
        close_all_windows()       
        title = '';
        label.innerHTML = '';
    }
});