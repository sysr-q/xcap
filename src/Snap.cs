using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace xcap
{
    class Snap
    {
        // -- TODO -- //
        // Add more in depths comments explaining what a lot of stuff does.
        // Add a way to change the key-bind.
        // Add an option to run xcap on start. 
        //  -> Probably means I'll have to pack this in an installer and make sure its run from %ProgramFiles%\.xcap or something.



        /// <summary>
        /// The location that xcap will upload it's images to.
        /// </summary>
        /// <remarks>
        /// Not sure if I should optionally add FTP or stick
        /// with a PHP uploading script.
        /// </remarks>
        public static Uri UploadUrl
        {
            get
            {
                return new Uri("http://example.com/upload.php");
            }
        }

        /// <summary>
        /// The current xcap build version.
        /// </summary>
        public static String Version
        {
            get
            {
                return "1.0.0";
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

        /// <summary>
        /// DummyForm used for handling keyboard hooks and grabbing form icons.
        /// </summary>
        /// <remarks>
        /// This form is double buffered to fix glitches in frozen snap mode, where the
        /// display boxes would freak out and have the horrible update line run down the form.
        /// </remarks>
        private static DummyForm form;
        /// <summary>
        /// The system-tray icon which is displayed.
        /// </summary>
        private static NotifyIcon icon;

        #region SELECTION_BOX
        private static bool down = false;
        private static Point 
            downPos,
            upPos,
            mousePos;
        private static Rectangle selectedRect;
        /// <summary>
        /// "where" enum defines where the selection rect's
        /// numbers should be.
        /// </summary>
        private enum where
        {
            NONE         = 0,
            TOP_LEFT     = 1,
            TOP_RIGHT    = 2,
            BOTTOM_LEFT  = 3,
            BOTTOM_RIGHT = 4
        }
        #endregion

        #region VARIABLES
        private enum UploadResult
        {
            UPLOAD_FAILED  = -1,
            UPLOAD_SUCCESS =  0,
            UPLOAD_MISC    =  1
        }

        /// <summary>
        /// Does the user like to use "Frozen" snap?
        /// This is where a still image is taken of the display when the snap is called, and they choose from that.
        /// </summary>
        private static bool Frozen
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
        private static bool Direct
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

        #endregion

        private static GlobalHotkey ghk;

        public Snap()
        {
            form = new DummyForm();
            ghk = new GlobalHotkey(Constants.CTRL, Keys.Oemtilde, form);
            ghk.Register();

            form.FormClosing += new FormClosingEventHandler(form_FormClosing);
            icon = new NotifyIcon();
            icon.Text = "xcap";
            icon.Icon = form.Icon;
            icon.Visible = true;
            icon.ContextMenu = new ContextMenu();

            MenuItem frozen = new MenuItem("Frozen Snap (BETA)");
            frozen.Click += (sender, e) =>
            {
                Frozen = !Frozen;
                frozen.Checked = Frozen;
            };
            frozen.Checked = Frozen;
            icon.ContextMenu.MenuItems.Add(frozen);

            MenuItem direct = new MenuItem("Direct Link");
            direct.Click += (sender, e) =>
            {
                Direct = !Direct;
                direct.Checked = Direct;
            };
            direct.Checked = Direct;
            icon.ContextMenu.MenuItems.Add(direct);

            icon.ContextMenu.MenuItems.Add("About", (sender, e) =>
            {
                MessageBox.Show("Ctrl-Tilde or double click the xcap icon to take a snap!\nCoded by PigBacon.", "xcap -- Version " + Version);
            });

            icon.ContextMenu.MenuItems.Add("Exit", (sender, e) =>
            {
                Application.Exit();
            });

            icon.DoubleClick += (sender, e) =>
            {
                Take();
            };
            Application.Run();

            form.Dispose();
            ghk.Unregister();
        }

        public static void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            icon.Dispose();
        }

        private static where GetWhere()
        {
            where wh = where.NONE;
            if (downPos.X > mousePos.X && downPos.Y < mousePos.Y)
            {
                wh = where.BOTTOM_LEFT;
            }
            else if (downPos.X < mousePos.X && downPos.Y > mousePos.Y)
            {
                wh = where.TOP_RIGHT;
            }
            else if (downPos.X > mousePos.X && downPos.Y > mousePos.Y)
            {
                wh = where.TOP_LEFT;
            }
            else
            {
                wh = where.BOTTOM_RIGHT;
            }
            return wh;
        }

        private static PointF PointFromWhere(where wh, SizeF sizef)
        {
            PointF point = new PointF();
            switch (wh)
            {
                case where.NONE:
                    point = new Point(-10, -10);
                    break;
                case where.TOP_LEFT:
                    point = new PointF((float)(mousePos.X + 2), (float)(mousePos.Y + 2));
                    break;
                case where.TOP_RIGHT:
                    point = new PointF((float)(mousePos.X - sizef.Width - 2), (float)(mousePos.Y + 2));
                    break;
                case where.BOTTOM_LEFT:
                    point = new PointF((float)(mousePos.X + 2), (float)(mousePos.Y - sizef.Height - 2));
                    break;
                case where.BOTTOM_RIGHT:
                    point = new PointF((float)(mousePos.X - sizef.Width - 2), (float)(mousePos.Y - sizef.Height - 2));
                    break;
            }
            return point;
        }

        public static void Take()
        {
            Form SelectFrm = new DummyForm();
            SelectFrm.Cursor = Cursors.Cross;
            /// Make sure that the form will use the *entire* screen bounds, or else it might shrink due to window tiling.
            SelectFrm.MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            SelectFrm.WindowState = FormWindowState.Maximized;
            SelectFrm.FormBorderStyle = FormBorderStyle.None;
            SelectFrm.ShowInTaskbar = false;
            SelectFrm.Opacity = Frozen ? 1.0 : 0.5;
            SelectFrm.BackColor = Frozen ? Color.White : Color.White;
            Bitmap bm = null;
            if (Frozen)
            {
                bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                Graphics snapShot = Graphics.FromImage(bm);
                snapShot.CopyFromScreen(0, 0,
                    Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    Screen.PrimaryScreen.Bounds.Size,
                    CopyPixelOperation.SourceCopy);
                bm = bm.Clone(new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), PixelFormat.Format32bppArgb);
                SelectFrm.BackgroundImage = bm;
            }
            SelectFrm.Paint += (sender, e) =>
            {
                mousePos = (down) ? Cursor.Position : upPos;
                where wh = GetWhere();

                selectedRect = new Rectangle(Math.Min(downPos.X, mousePos.X),
                    Math.Min(downPos.Y, mousePos.Y),
                    Math.Abs(downPos.X - mousePos.X),
                    Math.Abs(downPos.Y - mousePos.Y));

                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray)), selectedRect);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(0x00, 0x00, 0x00), 1), selectedRect);

                if (Frozen)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray)),
                        new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                    );
                }

                String info = selectedRect.Width + "\n" + selectedRect.Height;
                SizeF sizef = e.Graphics.MeasureString(info, new Font("Arial", 8));
                PointF point = PointFromWhere(wh, sizef);

                e.Graphics.DrawString(info, new Font("Arial", 8), new SolidBrush(Color.Black),
                    point);
            };
            SelectFrm.MouseDown += (sender, e) =>
            {
                SelectFrm.Invalidate();
                down = true;
                downPos = new Point(e.X, e.Y);
            };
            SelectFrm.MouseMove += (sender, e) =>
            {
                if (down)
                {
                    SelectFrm.Invalidate();
                }
            };
            SelectFrm.MouseUp += (sender, e) =>
            {
                down = false;
                upPos = new Point(e.X, e.Y);
                if (e.Button == MouseButtons.Right)
                {
                    SelectFrm.DialogResult = DialogResult.Cancel;
                }
                else if (e.Button == MouseButtons.Left)
                {
                    SelectFrm.DialogResult = DialogResult.OK;
                }
            };

            if (SelectFrm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                        Screen.PrimaryScreen.Bounds.Height,
                        PixelFormat.Format32bppArgb);

                    Graphics snapShot = Graphics.FromImage(bitmap);

                    snapShot.CopyFromScreen(0, 0,
                        Screen.PrimaryScreen.Bounds.X,
                        Screen.PrimaryScreen.Bounds.Y,
                        Screen.PrimaryScreen.Bounds.Size,
                        CopyPixelOperation.SourceCopy);

                    if (bm != null)
                    {
                        bitmap = bm.Clone(selectedRect, PixelFormat.Format32bppArgb);
                    }
                    else
                    {
                        bitmap = bitmap.Clone(selectedRect, PixelFormat.Format32bppArgb);
                    }
                    Upload(bitmap);
                    bitmap.Dispose();
                    snapShot.Dispose();
                    SelectFrm.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0}\n{1}", ex.Message, ex.StackTrace));
                }
            }
            if (!SelectFrm.IsDisposed)
                SelectFrm.Dispose();
            CleanUp();
        }

        /// <summary>
        /// Cleans up the selection box by moving the elements off-screen.
        /// </summary>
        /// <remarks>
        /// This is a really kludgey fix, might try and find a better way if possible.
        /// </remarks>
        private static void CleanUp()
        {
            selectedRect = new Rectangle(-2, -2, 1, 1);
            downPos = new Point(-1, -1);
            upPos = new Point(-1, -1);
            mousePos = new Point(-1, -1);
        }

        /// <summary>
        /// Saves the bitmap image to disk (simply because I don't know how to send a raw bitmap resource), then sends it to an upload script.
        /// </summary>
        /// <param name="bitmap">The bitmap to save & upload.</param>
        public static void Upload(Bitmap bitmap)
        {
            String TMP_FILE_LOC = Path.Combine(Folder, RandomCharacters() + ".png");
            bitmap.Save(TMP_FILE_LOC, ImageFormat.Png);
            WebClient Client = new WebClient();
            Client.Headers.Add("Content-Type", "binary/octet-stream");
            Client.QueryString.Add("direct", Direct ? "yes" : "no");
            byte[] result = Client.UploadFile(UploadUrl, "POST", TMP_FILE_LOC);
            string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
            Client.Dispose();
            File.Delete(TMP_FILE_LOC);

            UploadResult ur = UploadResult.UPLOAD_MISC;
            if (s.Substring(0, 1) == "+")
            {
                ur = UploadResult.UPLOAD_SUCCESS;
                Clipboard.SetText(s.Substring(1));
            }
            else if (s.Substring(0, 1) == "-")
            {
                ur = UploadResult.UPLOAD_FAILED;
            }
            else
            {
                ur = UploadResult.UPLOAD_MISC;
            }

            switch (ur)
            {
                case UploadResult.UPLOAD_SUCCESS:
                    icon.ShowBalloonTip(150, "Success! :)", "Successfully uploaded xcap and put link on clipboard.", ToolTipIcon.Info);
                    break;
                case UploadResult.UPLOAD_FAILED:
                    icon.ShowBalloonTip(150, "Failure! :(", s.Substring(1), ToolTipIcon.Error);
                    break;
                case UploadResult.UPLOAD_MISC:
                    MessageBox.Show(s, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        /// <summary>
        /// Creates a string which is a cluster of random alpha-numeric characters.
        /// Used for saving the image to disk.
        /// </summary>
        /// <param name="Length">The length the return string should be.</param>
        /// <returns>A string made of random alpha-numeric characters.</returns>
        private static string RandomCharacters(Int32 Length = 30)
        {
            Random rand = new Random();
            Char[] alpha =
            {
                'a', 'b', 'c', 'd', 
                'e', 'f', 'g', 'h', 
                'i', 'j', 'k', 'l', 
                'm', 'n', 'o', 'p', 
                'q', 'r', 's', 't',
                'u', 'v', 'w', 'x', 
                'y', 'z'
            };
            Char[] numeric =
            {
                '0', '1', '2', '3',
                '4', '5', '6', '7',
                '8', '9'
            };
            string temp = "";
            for (int i = 0; i < Length; i++)
            {
                int t = rand.Next(1, 5);
                if (t == 1)
                {
                    int u = rand.Next(0, 2);
                    char c = alpha[rand.Next(0, alpha.Length - 1)];
                    temp += (u == 1 ? c.ToString().ToLower() : c.ToString().ToUpper());
                }
                else if (t == 2)
                {
                    temp += numeric[rand.Next(0, numeric.Length - 1)].ToString();
                }
            }
            return temp;
        }
    }
}
