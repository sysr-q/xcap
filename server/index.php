<?php
    #<DEFINES>#
    
    // UPLOAD_DIR *should* have a trailing slash, or else many bad things happen.
    define ('UPLOAD_DIR', './i/');
    define ('SERVER_VER', '2.1.0');

    // Incase any incompatibilities pop up cross-version, this
    //  will help, by only allowing known-to-work versions.
    //  ofcourse the user can probably compile their own xcap without the
    //  client-side checks inplace. But we'll try and combat that with lots of server checks.
    //  You can add * to match all sorts of versions, like 2.*.* will match 2.0.0 and 2.3.4 etc.
    $ACCEPTED_VERSIONS = array('2.2.*');

    #</DEFINES>#

    $action = $_GET['act'];
    $var    = $_GET['var'];

    switch ($action) {
        case "valid":
            /*
             This will basically check whether or not the used version
             of xcap is compatible with this server-version. Because why not.

             If the version used is compatible, a '+' is output, whilst a
             non-compatible version, or non-given version will simply supply a '-'.

             To edit allowed versions, change the $ACCEPTED_VERSIONS variable.
            */
            header('Content-Type: text/plain');

            if (empty($var)) {
                exit ('-');
            }

            exit (is_valid_version($ACCEPTED_VERSIONS, $var) ? '+' : '-');

        break;

##################################################################################

        case "image":
            echo template_header('View Image', null, true);
            define ('FILE', UPLOAD_DIR.$var.'.png');
            if (empty($var) || !file_exists(FILE)) {
                echo '<p>Sorry, there is no image under that name here!</p>';
            } else {
                echo '<img src="'.FILE.'" align="left" />';
            }
            echo template_footer(true);
        break;

##################################################################################

        case "upload":
            function get_file_name() {
                $file = random_alphanumeric(6, false) . '.png';
                return $file;
            }
            function exit_($s, $image) {
                @unlink($_FILES[$image]['tmp_name']);
                exit($s);
            }

            header('Content-Type: text/plain');

            $image = "image";

            // User agent should be in style of: XCAP Upload Agent (Ver: X.Y.Z)
            $version = preg_replace('/XCAP Upload Agent \(Ver: (.*)\)/', '$1', $_SERVER['HTTP_USER_AGENT']);
            if (!is_valid_version($ACCEPTED_VERSIONS, $version)) {
                exit_('-'.'Something went wrong.', $image);
            }

            if (!isset($_FILES[$image]) || empty($var)) {
                exit_('-'.'Something went wrong.', $image);
            }

            if (!is_uploaded_file($_FILES[$image]['tmp_name'])) {
                exit_('-'.'Error uploading image.', $image);
            }

            if (!imagecreatefrompng($_FILES[$image]['tmp_name'])) {
                exit_('-'.'Non image was uploaded.', $image);
            }

            if (!is_dir(UPLOAD_DIR)) {
                mkdir(UPLOAD_DIR, 0755);
                file_put_contents(UPLOAD_DIR.'.htaccess', "Options -Indexes +FollowSymLinks".PHP_EOL."RewriteRule [^\.png]$ .. [NC,L]");
            }

            $file_name = get_file_name();
            while (file_exists(UPLOAD_DIR.$file_name)) {
                $file_name = get_file_name();
            }

            if (!move_uploaded_file($_FILES[$image]['tmp_name'], UPLOAD_DIR.$file_name)) {
                exit_('-'.'Failed moving uploaded image.', $image);
            }

            if ($var == "yes") {
                $dir = UPLOAD_DIR;
                if (substr($dir, 0, 2) == './') {
                    $dir = substr($dir, 2);
                }
                exit_('+'.get_url($dir.$file_name), $image);
            } else {
                exit_('+'.get_url($file_name), $image);
            }
        break;

##################################################################################

        default:
            echo template_header('Index', null, true);
            ?>
            <p>
                <strong>XCAP</strong> is a screen snapping tool written by <a href="http://pigbacon.net"><em>PigBacon</em></a> in C#.<br />
                The Git for xcap is located <b><a href="http://github.com/PigBacon/xcap">here!</a></b> Hurrah!
                <br />
                <strong>Known issues:</strong>
                <ul>
                    <li>Nothing, whoo!</li>
                </ul>
                <br />
                <strong>TODO:</strong>
                <ul>
                    <li>Add options for startup launching.</li>
                    <ul>
                        <li>This will probably require actual installation.</li>
                        <li>For now, put a shortcut to xcap-X.Y.Z.exe in your Startup folder, and don't move the xcap application.</li>
                    </ul>
                </li>
                </ul>
            </p>
            <?php
            echo template_footer(true);
        break;
    }

##################################################################################

function is_valid_version($accepted_versions, $version) {
    foreach ($accepted_versions as $ACCEPTED) {
        // We're going to be searching for X.Y.Z in the version.
        //  So we replace literal * with a .* and swap out literal . for \.
        // $ACCEPTED = 'X.Y.*' would lead to a regex like: /^X\.Y\..*$/
        $ACCEPTED = preg_replace('/\\\.\*/', '\\..*', preg_replace('/\./', '\\.', $ACCEPTED));
        $REGEX = '/^'.$ACCEPTED.'$/';
        if (preg_match($REGEX, $version)) {
            return true;
        }
    }
    return false;
}

function get_url($extra = null) {
    $page = 'http';
    if ($_SERVER["HTTPS"] == "on") {
        $page .= "s";
    }
    $page .= "://";
    if ($_SERVER["SERVER_PORT"] != "80") {
        $page .= $_SERVER["SERVER_NAME"].":".$_SERVER["SERVER_PORT"];
    } else {
        $page .= $_SERVER["SERVER_NAME"];
    }
    if (!is_null($extra)) {
        $page .= "/$extra";
    }
    return $page;
}

function random_alphanumeric($length, $upper = true) {
    $return = "";
    $alpha   = array('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z');
    $numeric = array('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

    for($count = 0; $count < $length; $count++) {
        if(rand(0, 1) == 1) {
            if($upper) {
                $return .= $alpha[rand(0, count($alpha) - 1)];
            } else {
                if(rand(0, 1) == 1) {
                    $return .= strtoupper($alpha[rand(0, count($alpha) - 1)]);
                } else {
                    $return .= strtolower($alpha[rand(0, count($alpha) - 1)]);
                }
            }
        } else {
            $return .= $numeric[rand(0, count($numeric) - 1)];
        }
    }

    if($upper) {
        $return = strtoupper($return);
    }

    return $return;
}

function template_header($page, $caption = null, $echo = false) {
    if (is_null($caption)) {
        $caption = 'Snap that whip!';
    }

    if ($echo) {
        ob_start();
    }

?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-type" content="text/html charset=utf-8" />
<title>xcap - <?= $page; ?></title>
<style type="text/css">
html, body {
  margin: 0;
  padding: 0;
}

body {
  text-align: center;
  font-family: Arial, sans-serif, Verdana;
  background-color: #0a0a0a;
  color: #d2d2d2;
  font-size: 75%;
}

a {
  color: #ffffff;
  text-decoration: none;
}

a:focus, a:hover, a:active {
  text-decoration: underline;
}

p, li {
  line-height: 1.8em;
}

h1, h2 {
  margin: 0 0 4px 0;
  letter-spacing: -1px;
}

h1 {
  font-size: 2.5em;
  color: #818181;
}

h2 {
  font-size: 1.5em;
  font-style: italic;
  font-weight: normal;
}

pre {
  font-size: 1.2em;
  line-height: 1.2em;
  overflow-x: auto;
}

div.page {
  width: 960px;
  margin: 0 auto;
  text-align: left;
}

div#container {
  padding: 0 20px;
}

div#header {
  width: 100%;
  height: 118px;
  background-color: #030303;
}

div#header-text {
  padding-top: 30px;
  padding-left: 64px;
}

div#footer {
  clear: both;
  color: #777;
  margin: 0 auto;
  padding: 20px 0 40px;
  text-align: center;
}

div.content {
  float: center;
  width: 720px;
  margin-top: 15px;
}

div.content a, div.navigation a {
  text-decoration: none;
  color: #777;
}

div.content a:focus, div.content a:hover, div.content a:active {
  text-decoration: underline;
}

.shadow {
  font-family: "Courier", courier, monospace;
  text-align: center;
  letter-spacing: -1px;
  color: white;
  font-weight: normal;
  text-shadow: 1px 1px 0px rgb(75,75,75);
}
</style>
</head>
<body>
<div id="header">
<div class="xcap">
<div id="header-text">
<h1 class="shadow" style="font-size: 250% font-weight: bold"><a href="<?= get_url() ?>">xcap</a></h1>
<div class="shadow"><?= $caption; ?></div>
</div>
</div>
</div>
<div class="xcap">
<div id="container">
<div align="left" style="padding-top:10px">
<?php

    if ($echo) {
        return ob_get_clean();
    }
}

function template_footer($echo = false) {
    if ($echo) {
        ob_start();
    }
?>
</div>
</div>
</div>
</body>
</html>
<?php
    if ($echo) {
        return ob_get_clean();
    }
}
?>