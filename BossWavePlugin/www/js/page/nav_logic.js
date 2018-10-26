var navbar = $("#nav-bar-id")[0]
var close = $(".close-nav")[0]
var grip = $("#grip")[0]
var gripcontainer = $("#gripcontainer")[0]
var options = $("#options")[0]

var subscription_modal = $("#subscription-window")[0];
var publish_modal = $("#publish-window")[0];
var entity_modal = $("#entity-window")[0];

 /* Registering all window click events */
window.onclick = function (event) {
    if (event.target.matches('.add-setting a')) {
        toggle_options()
    }

    if (event.target == subscription_modal) {
        close_all_windows()
        document.body.className = 'outer-frame';
    }

    if (event.target == publish_modal) {
        close_all_windows()
        document.body.className = 'outer-frame';
    }

    if (event.target == entity_modal) {
        close_all_windows()
        document.body.className = 'outer-frame';
    }
}

function close_all_windows(){
    publish_modal.style.display = "none";
    subscription_modal.style.display = "none";
    entity_modal.style.display = "none";
}

/* called on 'mouse moved' event */
function mouse_position(e) {
    // document.getElementById("test").innerHTML = e.x // debugging
    e = e || window.event;
    if (e.x < 20) { // if mouse is closer to left than 10 pixels
        navbar.style.width = "190px";
        grip.style.width = "0px";
        gripcontainer.className = 'hidden';
    }
}

/* When the user clicks on <span> (x), close the navigation bar */
$(".close-nav").on("click", function () {
    navbar.style.width = "0px";
    gripcontainer.className = '';
    grip.style.width = "15px";
});

/* Turn options ON / OFF */
function toggle_options() {
    options.classList.toggle("show");
}


$("#add-subscription").on("click", function () {
    document.body.className = 'temp-disable-scroll';
    subscription_modal.style.display = "block";
});

// When the user clicks on <span> (x), close the modal
$(".close-subscription").on("click", function () {
    close_all_windows();
    document.body.className = 'outer-frame';
});

$("#add-publish").on("click", function () {
    document.body.className = 'temp-disable-scroll';
    publish_modal.style.display = "block";
});

// When the user clicks on <span> (x), close the modal
$(".close-publish").on("click", function () {
    close_all_windows();
    document.body.className = 'outer-frame';
});

// When the user clicks on <span> (x), close the modal
$(".close-entity").on("click", function () {
    close_all_windows();
    document.body.className = 'outer-frame';
});

$(".add-entity").on("click", function () {
    close_all_windows();
    document.body.className = 'outer-frame';
    var entity_modal = $("#entity-path")[0];  
    $.connection.actionHub.server.setEntity(entity_modal.value);
    entity_modal.value = '';
});

function open_load_identity() {  
    document.body.className = 'temp-disable-scroll';
    entity_modal.style.display = "block";
}
