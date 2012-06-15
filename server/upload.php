<?php
    define ('URL', 'http://xcap.in');

    function get_file_name() {
        $file = substr(random_alphanumeric(25, false), 0, 6) . '.png';
        return $file;
    }

    if (!isset($_FILES['file']) || !isset($_GET['direct'])) {
        exit ('-'.'Something went wrong.');
    }

    if (is_uploaded_file($_FILES['file']['tmp_name'])) {
        if (imagecreatefrompng($_FILES['file']['tmp_name'])) {
            $file_name = get_file_name();
            while (file_exists('./i/'.$file_name)) {
                $file_name = get_file_name();
            }
            if (move_uploaded_file($_FILES['file']['tmp_name'], './i/'.$file_name)) {
                if ($_GET['direct'] == "yes") {
                    echo '+'.URL.'/i/'.$file_name;
                } else {
                    echo '+'.URL.'/'.$file_name;
                }
            } else {
                echo '-'.'Failed moving uploaded file.';
            }
        } else {
            echo '-'.'Invalid file was uploaded.';
        }
    } else {
        echo '-'.'Error uploading file.';
    }

    function _exit($s) {
        @unlink($_FILES['file']['tmp_name']);
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