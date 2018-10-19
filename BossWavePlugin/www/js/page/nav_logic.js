var navbar = $("#nav-bar-id")[0]
var close = $(".close-nav")[0]
var grip = $("#grip")[0]
var gripcontainer = $("#gripcontainer")[0]



// called on 'mouse moved' event
function mouse_position(e) {
    // document.getElementById("test").innerHTML = e.x // debugging
    e = e || window.event;
    if (e.x < 20) { // if mouse is closer to left than 10 pixels
        navbar.style.width = "190px";
        grip.style.width = "0px";
        gripcontainer.className = 'hidden';
    }
}

// When the user clicks on <span> (x), close the modal
$(".close-nav").on("click", function () {
    navbar.style.width = "0px";
    gripcontainer.className = '';
    grip.style.width = "15px";
});
    
    
