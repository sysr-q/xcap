<?php
    ob_start();
    include ('utility.php');
    ob_end_clean();
    template_header('Index');
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
            <li>For now, put a shortcut to xcap.exe in your Startup folder, and don't move the xcap application.</li>
        </ul>
    </li>
    </ul>
</p>
<?php
    template_footer();
?>