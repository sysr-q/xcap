using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace xcap
{
    class Snap
    {
        // -- TODO -- //
        // Add more in depths comments explaining what a lot of stuff does.
        // Add an option to run xcap on start. 
        //  -> Probably means I'll have to pack this in an installer and make sure its run from %ProgramFiles%\.xcap or something.
        // Update checking, comparing versions, the likes.


        /// <summary>
        /// DummyForm used for handling keyboard hooks and grabbing form icons.
        /// </summary>
        /// <remarks>
        /// This form is double buffered to fix glitches in frozen snap mode, where the
        /// display boxes would freak out and have the horrible update line run down the form.
        /// Because who needs vsync anyway?
        /// </remarks>
        public static DummyForm form { get; private set; }
        public static DummyForm1 form1 { get; private set; }

        /// <summary>
        /// The system-tray icon which is displayed.
        /// </summary>
        public static NotifyIcon icon { get; private set; }

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

        public Snap()
        {
            /// Setup default keybinds.
            RegistryKey key_snap = Registry.CurrentUser.CreateSubKey(@"Software\xcap");
            if (key_snap.GetValue("SNAP") == null)
                key_snap.SetValue("SNAP", "+--~");
            if (key_snap.GetValue("FULL") == null)
                key_snap.SetValue("FULL", "+-+~");
            if (key_snap.GetValue("URI") == null)
                key_snap.SetValue("URI", Settings.ServerUrl.ToString());
            if (key_snap.GetValue("OWN_SERV") == null)
                key_snap.SetValue("OWN_SERV", Settings.UseOwnServer);

            form = new DummyForm();
            form1 = new DummyForm1();

            Settings.RefreshKeyBinds();
            
            icon = new NotifyIcon();
            icon.Text = "xcap";
            icon.Icon = form.Icon;
            icon.Visible = true;
            icon.ContextMenu = new ContextMenu();

            MenuItem options = new MenuItem("Options");
            options.Click += (sender, e) =>
            {
                new Options().Show();
            };
            icon.ContextMenu.MenuItems.Add(options);
            
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
            form1.Dispose();
            Settings.ghk_Snap.Unregister();
            Settings.ghk_Full.Unregister();

            icon.Visible = false;
            icon.Dispose();
        }

        public static void LogError(Exception ex)
        {
            StreamWriter stream = new StreamWriter(Path.Combine(Settings.Folder, "error.log"), true);
            stream.WriteLine(String.Format("---- {0} ----", DateTime.Now.ToLongTimeString()));
            stream.WriteLine(ex);
            stream.Dispose();
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
            SelectFrm.Opacity = Settings.Frozen ? 1.0 : 0.5;
            SelectFrm.BackColor = Color.White;
            SelectFrm.TopMost = true;

            Bitmap bm = null;
            if (Settings.Frozen)
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

                if (Settings.Frozen)
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
                    LogError(ex);
                    MessageBox.Show(string.Format("An error occured!\n"
                    + "Please send the contents of your error.log to the developer."), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!SelectFrm.IsDisposed)
                SelectFrm.Dispose();
            CleanUp();
        }

        /// <summary>
        /// Takes a picture of the entire screen, allowing for easier fullscreen snapping.
        /// </summary>
        public static void TakeFull()
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
                bitmap = bitmap.Clone(new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), 
                                      PixelFormat.Format32bppArgb);
                Upload(bitmap);
                bitmap.Dispose();
                snapShot.Dispose();
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show(string.Format("An error occured!\n"
                    + "Please send the contents of your error.log to the developer."), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static void SetPercentIcon(int percent)
        {
            Brush brush = new SolidBrush(Color.White);
            Bitmap bitmap = new Bitmap(16, 16);
            Graphics graphics = Graphics.FromImage(bitmap);
            Font font = new Font(FontFamily.GenericMonospace, 8);
            graphics.FillEllipse(new SolidBrush(Color.Blue), new Rectangle(0, 0, 15, 15));
            graphics.DrawString((percent != 100 ? percent.ToString() : ":)"), font, brush, 0, 0);
            IntPtr hIcon = bitmap.GetHicon();
            Icon newIcon = Icon.FromHandle(hIcon);
            icon.Icon = newIcon;

            bitmap.Dispose();
            newIcon.Dispose();
            graphics.Dispose();
            font.Dispose();
        }

        #region Clipboard
        public static void SetClipboardWithRepeat(String s, int max)
        {
            max--;
            if (max <= 0)
            {
                MessageBox.Show("Unable to set clipboard string.\n" + s, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Clipboard.SetText(s);
            }
            catch (Exception ex)
            {
                LogError(ex);
                SetClipboardWithRepeat(s, max);
            }
        }
        #endregion

        public static void Upload(Image img)
        {
            Dictionary<String, String> post = new Dictionary<String, String>();
            post.Add("direct", "yes");

            HttpWebResponse resp = MultipartFormDataPost(Settings.UploadUrl, post, img);
            icon.Icon = form.Icon;
            Stream dataStream = resp.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String s = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            resp.Close();

            Settings.UploadResult ur = Settings.UploadResult.MISC;
            if (s.Substring(0, 1) == "+")
            {
                ur = Settings.UploadResult.SUCCESS;
                SetClipboardWithRepeat(s.Substring(1), 5);
            }
            else if (s.Substring(0, 1) == "-")
            {
                ur = Settings.UploadResult.FAILED;
            }
            else
            {
                ur = Settings.UploadResult.MISC;
            }

            switch (ur)
            {
                case Settings.UploadResult.SUCCESS:
                    icon.ShowBalloonTip(150, "Success! :)", "Successfully uploaded xcap.", ToolTipIcon.Info);
                    break;
                case Settings.UploadResult.FAILED:
                    icon.ShowBalloonTip(150, "Failure! :(", s.Substring(1), ToolTipIcon.Error);
                    break;
                case Settings.UploadResult.MISC:
                    MessageBox.Show(s, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private static String NewDataBoundary()
        {
            Random rnd = new Random();
            String formDataBoundary = "";
            while (formDataBoundary.Length < 15)
            {
                formDataBoundary = formDataBoundary + rnd.Next();
            }
            formDataBoundary = formDataBoundary.Substring(0, 15);
            formDataBoundary = "-----------------------------" + formDataBoundary;
            return formDataBoundary;
        }

        public static HttpWebResponse MultipartFormDataPost(Uri postUrl, Dictionary<String, String> postParameters, Image img)
        {
            String boundary = NewDataBoundary();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);

            // Set up the request properties
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.UserAgent = "XCAP Upload Agent";

            #region WRITING STREAM
            using (Stream formDataStream = request.GetRequestStream())
            {
                String header = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                    boundary,
                    "image",
                    "xcap-image.png",
                    "image/png");

                formDataStream.Write(Encoding.UTF8.GetBytes(header), 0, header.Length);
                
                /* This is just incase you want to plainly dump the image into the
                 * server. Otherwise we use a memory stream and check percentages.
                byte[] image = ImageToByte(img);
                formDataStream.Write(image, 0, image.Length);
                 */

                byte[] image = ImageToByte(img);
                MemoryStream ms = new MemoryStream(image);
                int count = 0, total = 0, chunkSize = 512;
                do
                {
                    byte[] buf = new byte[chunkSize];
                    count = ms.Read(buf, 0, chunkSize);
                    total += count;
                    formDataStream.Write(buf, 0, count);
                    int percent = ((total * 100) / image.Length);
                    SetPercentIcon(percent);
                } while (ms.CanRead && count > 0);

                foreach (var param in postParameters)
                {
                    String postData = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);
                }
                // Add the end of the request
                byte[] footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                formDataStream.Write(footer, 0, footer.Length);
                formDataStream.Close();
            }
            #endregion

            return request.GetResponse() as HttpWebResponse;
        }
    }
}
