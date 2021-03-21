/*=====================================
|             change -password        |
 =====================================*/
$(".toggle-password-old").click(function() {

   $(this).toggleClass("fa-eye fa-eye-slash");
   var input = $($(this).attr("toggle"));
   if (input.attr("type") == "password") {
     input.attr("type", "text");
   } else {
     input.attr("type", "password");
   }
 });
 $(".toggle-password-new").click(function() {

   $(this).toggleClass("fa-eye fa-eye-slash");
   var input = $($(this).attr("toggle"));
   if (input.attr("type") == "password") {
     input.attr("type", "text");
   } else {
     input.attr("type", "password");
   }
 });
 $(".toggle-password-confirm").click(function() {

   $(this).toggleClass("fa-eye fa-eye-slash");
   var input = $($(this).attr("toggle"));
   if (input.attr("type") == "password") {
     input.attr("type", "text");
   } else {
     input.attr("type", "password");
   }
 });
 /*=====================================
|             Sign-up                  |
 =====================================*/
 $(".toggle-new").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});
$(".toggle-cpass").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});