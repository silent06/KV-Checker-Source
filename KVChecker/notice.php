<!DOCTYPE html>
<html lang="en" >

<head>
    <meta charset="utf-8">
    <title>Console Log</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Bootstrap -->
    <link href="css/bootstrap.css" rel="stylesheet" media="screen">
    <link href="css/bootstrap.min.css" rel="stylesheet" media="screen">
    <link href="http://twitter.github.com/bootstrap/assets/css/bootstrap.css" rel="stylesheet">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js"></script>
    <script src="http://twitter.github.com/bootstrap/assets/js/bootstrap-tooltip.js"></script>
    <script src="http://twitter.github.com/bootstrap/assets/js/bootstrap-popover.js"></script>
    <script src="https://code.jquery.com/jquery.js"></script>
    <script src="js/bootstrap.min.js"></script>
    <script src="https://code.jquery.com/jquery.js"></script>
    <script src="js/bootstrap.min.js"></script>
    <script>
    function scroll() {
	    $("#console").animate({scrollTop:$("#console")[0].scrollHeight - $("#console").height()}, 0, function() { })
    }
    </script>
</head>

<body onload="scroll()"> 
  <div class="container">
    <br/>
    <div class="row">
      <div class="col-lg-12">
        <div class="bs-component">
		<textarea id="console" style="margin: 0px; width: 938px; height: 666px;" readonly>
		<?php
			echo file_get_contents("/Users/Administrator/Desktop/KV Checker/KV.log"); 
		?></textarea>
		<a href="notice.php" class="btn btn-primary">Refresh</a>
        </div>
      </div>
    
</body>

</html>
