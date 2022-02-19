<!DOCTYPE html>
<html>

<head>

</head>
<?php
if (isset($_POST['submit'])) {

  $errors = []; // Store errors here
  $fileExtensionsAllowed = ['bin']; // These will be the only file extensions allowed 
  $fileName = $_FILES['my-file']['name'];
  $fileSize = $_FILES['my-file']['size'];
  $fileTmpName  = $_FILES['my-file']['tmp_name'];
  $fileType = $_FILES['my-file']['type'];
  //fileExtension = strtolower(end(explode('.',$fileName)));
  $uploadFileDir = '/var/www/html/kvchecker/';// /var/www/html/
  $uploadPath = $uploadFileDir . basename($fileName);
    if ($fileSize > 4000000) {
      $errors[] = "File exceeds maximum size (4MB)";
    }

    if (empty($errors)) {
        $didUpload = move_uploaded_file($fileTmpName, $uploadPath);

        if ($didUpload) 
        {
          //echo "The file " . basename($fileName) . " has been uploaded";
        } 
        else 
        {
          //echo "An error occurred. Please contact the administrator.";
        }
    } 
    else 
    {
      foreach ($errors as $error) {
          //echo $error . "These are the errors" . "\n";
      }
    }
  $socket = @fsockopen("127.0.0.1", 5000, $errno, $errstr);
    if (!$socket){ return exit("");}
  header("refresh: 1; url = index.php");
    
}
?>
</html>
