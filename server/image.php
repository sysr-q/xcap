<?php 
    ob_start();
    include ('utility.php');
    ob_end_clean();
    template_header('View Image');
    $image = $_GET['img']; 
    define ('FILE', './i/'.$image.'.png');
    if (empty($image) || !file_exists(FILE)) {
        echo '<p>Sorry, there is no image under that name here!</p>';
    } else {
        echo '<img src="./i/'.$image.'.png" align="left" />';
    }
    template_footer();
?>