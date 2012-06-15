<?php
    ob_start();
    include ('utility.php');
    ob_end_clean();
    template_header('Index');
?>
<p>
    <strong>XCAP</strong> is a screen snapping tool written by <a href="http://pigbacon.net"><em>PigBacon</em></a> in C#.<br />
    It's going to be open source on GitHub once a few bugs are fixed.<br />
    <br />
    <strong>Known issues:</strong>
    <ul>
        <li>Frozen Snap bugs out graphically.</li>
        <li>Frozen Snap's selection box is darker due to layering of colors.</li>
        <li>The numbers in the selection boxes can come out of the box if you select a small area.</li>
    </ul>
    <br />
    <strong>TODO:</strong>
    <ul>
        <li>Finish cleaning code and Git everything.</li>
        <li>Fix Frozen Snap's graphic issues.</li>
        <li>Add options for startup launching.</li>
        <li>Key binds, key binds, key binds! Bind <strong>ALL</strong> the things.</li>
        <li>Make an "Options" form with a few more options:
        <ul>
            <li>Startup launching</li>
            <li>Use own site? (FTP/PHP script? Dunno.)</li>
            <li>Other stuff which I think of.</li>
        </ul>
    </li>
    </ul>
</p>
<?php
    template_footer();
?>