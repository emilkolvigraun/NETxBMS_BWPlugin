var actionhub = $.connection.actionHub;

String.prototype.isNumber = function(){return /^\d+$/.test(this);}

actionhub.client.getSubItem = function (message) {
    console.log(message);
}

function load_sub(title){
    actionhub.server.getItemInfo("sub", title)
}

$(".add-sub").on("click", function () {
    var title = $("#subscribtion-title")[0].value
    var expirydelta = $("#expirydelta")[0].value
    var namespace = $("#namespace")[0].value
    var expirymili = $("#expirymili")[0].value
    var autochain = $("#autochain")[0].checked
    var label = $("#error-label")[0]

    
    label.innerHTML = "<br><br><br><br>"
    if (title == '' || namespace == ''){
        label.innerHTML += "You need to enter a title and a namespace<br>";
    } else if (!expirymili.isNumber() || !expirydelta.isNumber()) {
        label.innerHTML += "Expiry time has to be in miliseconds<br>";
    } else {
        sub_info = '{ "title": "' + title + '", "expirydelta": "' + expirydelta + '", "expirymili": "' + expirymili + '", "autochain": "' + autochain + '", "namespace": "' + namespace + '" }'

        actionhub.server.createSubscription(sub_info)
        close_all_windows()       
        title = '';
        label.innerHTML = '';
    }
});