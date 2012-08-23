using System;
using System.Windows.Forms;

namespace xcap
{
    public partial class Options : Form
    {
        Keys key_snap, key_full;
        Char char_snap, char_full;

        public Options()
        {
            InitializeComponent();
            this.Icon = Snap.form1.Icon;
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
            
            TxtServerAddr.Text = Settings.StripUrl(Settings.ServerUrl.ToString());
            TryServer(TxtServerAddr.Text);
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
            String URI = Settings.StripUrl(TxtServerAddr.Text);
            TryServer(TxtServerAddr.Text);
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

        public static Boolean ValidServer(String ServerUrl)
        {
            String server = Settings.StripUrl(ServerUrl);
            Uri version = new Uri("http://" + server + "/?act=valid&var=" + Settings.Version.ToString());
            String reply = "--";
            try
            {
                reply = new System.Net.WebClient().DownloadString(version).Trim();
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

            return reply == "+";
        }

        private void TryServer(String ServerUrl)
        {
            String server = Settings.StripUrl(ServerUrl);
            Uri version = new Uri("http://" + server + "/?act=valid&var=" + Settings.Version.ToString());
            String reply = "--";
            try
            {
                reply = new System.Net.WebClient().DownloadString(version).Trim();
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
                ChkValidServer.Text = "Valid";
            }
            else if (reply == "-")
            {
                ChkValidServer.CheckState = CheckState.Indeterminate;
                ChkValidServer.Text = "Invalid";
            }
            else
            {
                ChkValidServer.CheckState = CheckState.Unchecked;
                ChkValidServer.Text = "Unknown";
            }
        }
    }
}
