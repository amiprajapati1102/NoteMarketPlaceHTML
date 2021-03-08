/*=====================================
|             password                 |
 =====================================*/
 $(".toggle-password").click(function() {

   $(this).toggleClass("fa-eye fa-eye-slash");
   var input = $($(this).attr("toggle"));
   if (input.attr("type") == "password") {
     input.attr("type", "text");
   } else {
     input.attr("type", "password");
   }
 });
 /*=====================================
|             password  valid          |
 =====================================*/
 var email=document.forms['form']['email'];
 var password=document.forms['form']['password'];
 var pass_error=document.getElementById("error-password");
 var pass_box=document.getElementById("password");
 function validated(){

     if(password.value.length<6){
        //alert("check");
        pass_error.style.display="block";
        pass_box.style.border="1px solid #FF3636;"

     }

 }