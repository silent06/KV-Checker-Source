$(document).ready(function(){
    
  $("#submit").click(function(){

    var username = $("#adminUsername").val();
    var password = $("#adminPassword").val();
    
    if((username == "") || (password == "")) {
      $("#message").html("<div class=\"alert alert-danger alert-dismissable\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-hidden=\"true\">&times;</button>Please enter a username and a password</div>");
    }
    else {
      $.ajax({
        type: "POST",
        url: "check_login.php",
        data: "adminUsername="+username+"&adminPassword="+password,
        success: function(html){    
          if(html=='true') {
            window.location="index.php";
          }
          else {
            $("#message").html(html);
          }
        },
        beforeSend:function()
        {
          $("#message").html("<p class='text-center'><img src='images/ajax-loader.gif'></p>")
        }
      });
    }
    return false;
  });
});