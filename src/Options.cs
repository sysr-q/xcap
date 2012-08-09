using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace xcap
{
    public partial class Options : Form
    {
        Keys key_snap, key_full;
        Char char_snap, char_full;
        Timer _timer = new Timer();
        String[] _timer_chars = { "--", "\\", "|", "/" };
        int _timer_at = 0;
        bool _timer_check = false;

        public Options()
        {
            InitializeComponent();
            this.Icon = Snap.form1.Icon;
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Interval = 500;
            _timer.Start();
            this.Text = "Options (xcap-" + Settings.Version + ")";

            TxtKeyBindSnap.KeyPress += new KeyPressEventHandler(TxtKeyBindSnap_KeyPress);
            TxtKeyBindFull.KeyPress += new KeyPressEventHandler(TxtKeyBindFull_KeyPress);

            RadUseOwnServer.Checked = Settings.UseOwnServer;
            RadUseXcapServer.Checked = !RadUseOwnServer.Checked;
            SetServerEnabled(RadUseOwnServer.Checked);

            ChkDirect.Checked = Settings.Direct;
            ChkFrozen.Checked = Settings.Frozen;

            try
            {
                String snap = Settings.KeySnap;
                char ctrl = snap[0], alt = snap[1], shift = snap[2], key = snap[3];
                ChkCtrlSnap.Checked  = ctrl  == '+';
                ChkAltSnap.Checked   = alt   == '+';
                ChkShiftSnap.Checked = shift == '+';
                TxtKeyBindSnap.Text  = key.ToString().ToLower() + " (" + Constants.KeysFromChar(key).ToString() + ")";
                key_snap = Constants.KeysFromChar(key);
                char_snap = key;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            try
            {
                String snap = Settings.KeyFull;
                char ctrl = snap[0], alt = snap[1], shift = snap[2], key = snap[3];
                ChkCtrlFull.Checked = ctrl == '+';
                ChkAltFull.Checked = alt == '+';
                ChkShiftFull.Checked = shift == '+';
                TxtKeyBindFull.Text = key.ToString().ToLower() + " (" + Constants.KeysFromChar(key).ToString() + ")";
                key_full = Constants.KeysFromChar(key);
                char_full = key;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            
            TxtServerAddr.Text = StripUrl(Settings.ServerUrl.ToString());
            TryServer();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!_timer_check)
            {
                _timer_at = 0;
                this.ChkValidServer.Text = "Valid";
                return;
            }
            _timer_at++;
            if (_timer_at > _timer_chars.Length - 1)
                _timer_at = 0;
            String s = _timer_chars[_timer_at];
            this.ChkValidServer.Text = "Valid " + s + "";
        }

        private void TxtKeyBindFull_KeyPress(object sender, KeyPressEventArgs e)
        {
            Keys k = Constants.KeysFromChar(e.KeyChar);
            e.Handled = true;
            if (k != Keys.None)
            {
                TxtKeyBindFull.Text = e.KeyChar.ToString().ToLower() + " (" + k.ToString() + ")";
                key_full = k;
                char_full = e.KeyChar;
            }
        }

        private void TxtKeyBindSnap_KeyPress(object sender, KeyPressEventArgs e)
        {
            Keys k = Constants.KeysFromChar(e.KeyChar);
            e.Handled = true;
            if (k != Keys.None)
            {
                TxtKeyBindSnap.Text = e.KeyChar.ToString().ToLower() + " (" + k.ToString() + ")";
                key_snap = k;
                char_snap = e.KeyChar;
            }
        }

        private void RadUseXcapServer_CheckedChanged(object sender, EventArgs e)
        {
            SetServerEnabled(RadUseOwnServer.Checked);
        }

        private void RadUseOwnServer_CheckedChanged(object sender, EventArgs e)
        {
            SetServerEnabled(RadUseOwnServer.Checked);
        }

        private void SetServerEnabled(Boolean b)
        {
            TxtServerAddr.Enabled = b;
            ChkValidServer.Enabled = b;
            LblServ.Enabled = b;
            LblServInfo.Enabled = b;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveSettings()
        {
            String  _snap = (ChkCtrlSnap.Checked ? "+" : "-") + (ChkAltSnap.Checked ? "+" : "-") + (ChkShiftSnap.Checked ? "+" : "-") + char_snap;
            String  _full = (ChkCtrlFull.Checked ? "+" : "-") + (ChkAltFull.Checked ? "+" : "-") + (ChkShiftFull.Checked ? "+" : "-") + char_full;
            String URI = StripUrl(TxtServerAddr.Text);
            _timer_check = true;
            TryServer();
            /// Set all the Snap variables.
            Settings.KeySnap = _snap;
            Settings.KeyFull = _full;
            Settings.UseOwnServer = RadUseOwnServer.Checked;
            Settings.ServerUrl = new Uri("http://" + URI);
            Settings.Direct = ChkDirect.Checked;
            Settings.Frozen = ChkFrozen.Checked;
            Settings.RefreshKeyBinds();
            TxtServerAddr.Text = URI;
            if (!ChkValidServer.Checked)
            {
                Snap.icon.ShowBalloonTip(150, "Warning!", "There was a problem validating your selected server.\n"
                + "Make sure it's valid, or else xcap won't work!", ToolTipIcon.Warning);
            }
        }

        private String StripUrl(String s)
        {
            s = Regex.Replace(s, "^http:\\/\\/", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, "\\/$", "", RegexOptions.IgnoreCase);
            return s;
        }

        private void TryServer()
        {
            String server = StripUrl(TxtServerAddr.Text);
            Uri version = new Uri("http://" + server + "/valid.php?" + Settings.Version.ToString());
            String reply = "--";
            try
            {
                reply = new System.Net.WebClient().DownloadString(version);
            }
            catch (System.Net.WebException we)
            {
                Snap.LogError(we);
                Snap.icon.ShowBalloonTip(150, "Failure! :(", "There was a problem connecting to your selected server.", ToolTipIcon.Error);
            }
            catch (Exception e)
            {
                Snap.LogError(e);
                Snap.icon.ShowBalloonTip(150, "Failure! :(", "There was a general problem connecting to your selected server.", ToolTipIcon.Error);
            }

            if (reply == "+")
            {
                ChkValidServer.CheckState = CheckState.Checked;
            }
            else if (reply == "-")
            {
                ChkValidServer.CheckState = CheckState.Indeterminate;
            }
            else
            {
                ChkValidServer.CheckState = CheckState.Unchecked;
            }
            _timer_check = false;
        }
    }
}
