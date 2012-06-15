<?php
    define ('XCAP_URL', 'http://xcap.in');

    function template_header($page, $caption = null) {
        if (is_null($caption)) {
            $caption = 'Snap that whip!';
        }

        echo '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"'.PHP_EOL;
        echo '"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">'.PHP_EOL;
        echo '<html xmlns="http://www.w3.org/1999/xhtml">'.PHP_EOL;
        echo '<head>'.PHP_EOL;
        echo '<meta http-equiv="Content-type" content="text/html; charset=utf-8" />'.PHP_EOL;
        echo '<title>XCAP - '.$page.'</title>'.PHP_EOL;
        echo '<link rel="stylesheet" href="css/style.css" type="text/css" />'.PHP_EOL;
        echo '</head>'.PHP_EOL;
        echo '<body>'.PHP_EOL;
        echo '<div id="header">'.PHP_EOL;
        echo '<div class="xcap">'.PHP_EOL;
        echo '<div id="header-text">'.PHP_EOL;
        echo '<h1 class="shadow" style="font-size: 250%; font-weight: bold;"><a href="'.XCAP_URL.'">XCAP</a></h1>'.PHP_EOL;
        echo '<div class="shadow">'.$caption.'</div>'.PHP_EOL;
        echo '</div>'.PHP_EOL;
        echo '</div>'.PHP_EOL;
        echo '</div>'.PHP_EOL;
        echo '<div class="xcap">'.PHP_EOL;
        echo '<div id="container">'.PHP_EOL;
        echo '<!--text-align:center; margin-top:0px; margin-bottom:0px;-->'.PHP_EOL;
        echo '<div align="left" style="padding-top:10px;">'.PHP_EOL;
    }

    function template_footer() {
        echo '</div>'.PHP_EOL;
        echo '</div>'.PHP_EOL;
        echo '</div>'.PHP_EOL;
        echo '</body>'.PHP_EOL;
        echo '</html>'.PHP_EOL;
    }
?>