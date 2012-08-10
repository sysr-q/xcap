using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace xcap
{
    class Settings
    {
        /// <summary>
        /// An enum which gives the state of an image upload.
        /// </summary>
        public enum UploadResult
        {
            FAILED  = -1,
            SUCCESS = 0,
            MISC    = 1
        }

        /// <summary>
        /// The current xcap build version.
        /// </summary>
        public static String Version
        {
            get
            {
                return "2.0.2";
            }
        }

        /// <summary>
        /// Does the user want to use "Frozen" snap?
        /// This is where a still image is taken of the display when the snap is called, and they choose from that.
        /// </summary>
        public static bool Frozen
        {
            get
            {
                return Boolean.Parse(Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .GetValue("FREEZE", false).ToString());
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .SetValue("FREEZE", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Would the user prefer a "direct" link to the snap (http://example.com/i/12345.png), or
        /// an indirect link (http://example.com/12345.png) which will display the image in a page.
        /// </summary>
        public static bool Direct
        {
            get
            {
                return Boolean.Parse(Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .GetValue("DIRECT", false).ToString());
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .SetValue("DIRECT", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// The ghk_Snap variables.
        /// First three characters are + or -, depending on state of varying things.
        /// 1st: Ctrl
        /// 2nd: Alt
        /// 3rd: Shift
        /// Rest is the key bound.
        /// </summary>
        /// <example>
        /// +--~  -> Ctrl + Tilde
        /// ++-/  -> Ctrl + Alt + Slash
        /// </example>
        public static String KeySnap
        {
            get
            {
                return Registry.CurrentUser.CreateSubKey(@"Software\xcap").GetValue("SNAP", "+--~").ToString();
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap").SetValue("SNAP", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// The ghk_Full variables.
        /// First three characters are + or -, depending on state of varying things.
        /// 1st: Ctrl
        /// 2nd: Alt
        /// 3rd: Shift
        /// Rest is the key bound.
        /// </summary>
        /// <example>
        /// +--~  -> Ctrl + Tilde
        /// ++-/  -> Ctrl + Alt + Slash
        /// </example>
        public static String KeyFull
        {
            get
            {
                return Registry.CurrentUser.CreateSubKey(@"Software\xcap").GetValue("FULL", "+-+~").ToString();
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap").SetValue("FULL", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// The key bound to the Snap.Take() function.
        /// </summary>
        public static GlobalHotkey ghk_Snap
        {
            get;
            set;
        }

        /// <summary>
        /// The key bound to the Snap.TakeFull() function.
        /// </summary>
        public static GlobalHotkey ghk_Full
        {
            get;
            set;
        }

        /// <summary>
        /// The location that xcap will upload it's images to.
        /// </summary>
        public static Uri UploadUrl
        {
            get
            {
                return new Uri((UseOwnServer ? ServerUrl.ToString() : XcapUploadUrl.ToString()) + "/upload.php?direct=" + (Direct ? "yes" : "no"));
            }
        }

        /// <summary>
        /// The URL that defines the 'root' of the xcap server used.
        /// </summary>
        public static Uri ServerUrl
        {
            get
            {
                return new Uri("http://" 
                    + StripUrl(Registry.CurrentUser.CreateSubKey(@"Software\xcap").GetValue("URI", "xcap.example.com").ToString()));
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap").SetValue("URI", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Is the user using their own server, or the public xcap server?
        /// </summary>
        public static bool UseOwnServer
        {
            get
            {
                return Boolean.Parse(Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .GetValue("OWN_SERV", true).ToString());
            }
            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\xcap")
                    .SetValue("OWN_SERV", value, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// The location of the official xcap server.
        /// </summary>
        public static Uri XcapUploadUrl
        {
            get
            {
                return new Uri("im.xcap.in");
            }
        }

        /// <summary>
        /// Returns a string of the path to the .xcap folder in Application Data.
        /// </summary>
        public static String Folder
        {
            get
            {
                String PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".xcap");
                if (!Directory.Exists(PATH))
                {
                    Directory.CreateDirectory(PATH);
                }
                return PATH;
            }
        }

        public static String StripUrl(String s)
        {
            s = Regex.Replace(s, "^http:\\/\\/", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, "\\/$", "", RegexOptions.IgnoreCase);
            return s;
        }

        public static void RefreshKeyBinds()
        {
            if (ghk_Snap != null)
                ghk_Snap.Unregister();
            if (ghk_Full != null)
                ghk_Full.Unregister();

            Int32 _constants = Constants.NOMOD;
            Keys _keys = Keys.None;
            try
            {
                String snap = Settings.KeySnap;
                char ctrl = snap[0], alt = snap[1], shift = snap[2], key = snap[3];
                if (ctrl == '+')
                    _constants |= Constants.CTRL;
                if (alt == '+')
                    _constants |= Constants.ALT;
                if (shift == '+')
                    _constants |= Constants.SHIFT;
                _keys = Constants.KeysFromChar(key);
            }
            catch (Exception e)
            {
                _constants = Constants.CTRL;
                _keys = Keys.Oemtilde;
                Snap.icon.ShowBalloonTip(150, "Failure! :(", 
                    "There was a problem loading the snap bind.\nDefaulting to Ctrl-Tilde.", ToolTipIcon.Error);
                System.Diagnostics.Debug.WriteLine(e);
            }
            Settings.ghk_Snap = new GlobalHotkey(_constants, _keys, Snap.form);
            Settings.ghk_Snap.Register();

            try
            {
                String snap = Settings.KeyFull;
                char ctrl = snap[0], alt = snap[1], shift = snap[2], key = snap[3];
                if (ctrl == '+')
                    _constants |= Constants.CTRL;
                if (alt == '+')
                    _constants |= Constants.ALT;
                if (shift == '+')
                    _constants |= Constants.SHIFT;
                _keys = Constants.KeysFromChar(key);
            }
            catch (Exception e)
            {
                _constants = Constants.CTRL;
                _keys = Keys.Oemtilde;
                Snap.icon.ShowBalloonTip(150, "Failure! :(", "There was a problem loading the full-snap bind.\n"
                                                        + "Defaulting to Ctrl-Shift-Tilde.", ToolTipIcon.Error);
                System.Diagnostics.Debug.WriteLine(e);
            }
            Settings.ghk_Full = new GlobalHotkey(_constants, _keys, Snap.form1);
            Settings.ghk_Full.Register();
        }
    }
}
