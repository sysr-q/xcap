using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;

namespace xcap
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            new Snap();
        }
    }
}