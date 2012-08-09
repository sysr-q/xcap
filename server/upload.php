<?php

    ob_start();
    include ('utility.php');
    ob_end_clean();

    function get_file_name() {
        $file = random_alphanumeric(6, false) . '.png';
        return $file;
    }

    global $image;
    $image = "image";

    if (!isset($_FILES[$image]) || !isset($_GET['direct'])) {
        exit_('-'.'Something went wrong.');
    }

    if (!is_uploaded_file($_FILES[$image]['tmp_name'])) {
        exit_('-'.'Error uploading image.');
    }

    if (!imagecreatefrompng($_FILES[$image]['tmp_name'])) {
        exit_('-'.'Non image was uploaded.');
    }

    if (!is_dir(UPLOAD_DIR)) {
        mkdir(UPLOAD_DIR, 0755);
    }

    $file_name = get_file_name();
    while (file_exists(UPLOAD_DIR.'/'.$file_name)) {
        $file_name = get_file_name();
    }

    if (!move_uploaded_file($_FILES[$image]['tmp_name'], UPLOAD_DIR.'/'.$file_name)) {
        exit_('-'.'Failed moving uploaded image.');
    }

    if ($_GET['direct'] == "yes") {
        $dir = UPLOAD_DIR;
        if (substr($dir, 0, 2) == './') {
            $dir = substr($dir, 2);
        }
        exit_('+'.get_url($dir.$file_name));
    } else {
        exit_('+'.get_url($file_name));
    }

    function exit_($s) {
        @unlink($_FILES[$image]['tmp_name']);
        exit($s);
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
?>