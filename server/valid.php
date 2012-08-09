<?php
    /*
     This script will basically check whether or not the used version
     of xcap is compatible with this server-version. Because why not.

     If the version used is compatible, a '+' is output, whilst a
     non-compatible version, or non-given version will simply supply a '-'.
    */

    // Incase any incompatibilities pop up cross-version, this
    //  will help, by only allowing known-to-work versions.
    //  ofcourse the user can probably compile their own xcap without the
    //  checks inplace. What can you do..
    //  You can add * to match all sorts of versions, like 2.*.* will match 2.0.0 and 2.3.4 etc.
    $ACCEPTED_VERSIONS = array('2.*.*');

    $VERSION = $_SERVER['QUERY_STRING'];

    if (!isset($_SERVER['QUERY_STRING'])) {
        exit ('-');
    }

    foreach ($ACCEPTED_VERSIONS as $ACCEPTED) {
        $REGEX = '/^'.$ACCEPTED.'$/';
        if (preg_match($REGEX, $VERSION)) {
            exit ('+');
        }
    }

    exit ('-');
?>