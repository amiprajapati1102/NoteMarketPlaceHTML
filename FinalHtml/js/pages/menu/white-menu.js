/*=====================================
|              Navigation             |
 =====================================*/

 $(function(){
    if ($(window).width() < 640) {
        $('#add').removeClass('text-right');
        
    } else{
        $('#add').addClass('text-right');
    }
    
    showHideNav();
    
    $(window).scroll(function(){
        showHideNav();
    });
    
    function showHideNav(){
        if( $(window).scrollTop() > 50) {
//           Show White Nav
            $("nav").addClass("white-nav-top");
            
//            Show dark logo
            $(".navbar-brand img").attr("src", "../img/login/logo.png");
            $("#add img").attr("src", "../img/login/logo.png");
            
//              Show back to top button
            $("#back-to-top").fadeIn();
            
           }
           else{
//            Hide white Nav
               $("nav").removeClass("white-nav-top");
               
//            Show Normal logo
            $(".navbar-brand img").attr("src", "../img/login/logo.png");

               
//              Hide back to top button
            $("#back-to-top").hide();
 
               
            }
        }
});


/*=====================================
|              Mobile Menu             |
 =====================================*/

$(function(){
    
//    Show Mobile Nav
    $("#mobile-nav-open-btn").click(function() {
       $("#mobile-nav").css("height", "100%"); 
    });
    //    Hide Mobile Nav
    $("#mobile-nav-close-btn, #mobile-nav a").click(function() {
       $("#mobile-nav").css("height", "0%"); 
    });
    
});





































