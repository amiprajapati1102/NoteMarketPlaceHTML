
function sticky_header() {
    var header_height = $('.home_navbar').innerHeight() / 4;
    var scrollTop = $(window).scrollTop();;
    if (scrollTop > header_height) {
        $('body').addClass('sticky-nav')
        $(".home_navbar .navbar img").attr("src", "../img/login/logo.png");
    } else {
        $('body').removeClass('sticky-nav')
        $(".home_navbar .navbar img").attr("src", "../img/login/top-logo.png");
    }
}

$(document).ready(function () {
  sticky_header();
});

$(window).scroll(function () {
  sticky_header();  
});
$(window).resize(function () {
  sticky_header();
});


$(".toggle-password").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});



