using System;
using System.Threading;
using System.Windows.Forms;

namespace xcap
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool mutexCreated = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, @"xcapSnappingTool", out mutexCreated);

            if (mutexCreated)
            {
                Thread t1 = new Thread(new ThreadStart(SplashForm));
                t1.Name = "Splash";
                t1.Start();
                Thread.Sleep(1000);
                t1.Abort();
                new Snap();
            }
            else
            {
                mutex.Dispose();
                Application.Exit();
                return;
            }
        }

        private static void SplashForm()
        {
            DummyForm1 dummy = new DummyForm1();
            SplashScreen splash = new SplashScreen() { 
                Owner = dummy
            };
            splash.ShowDialog();
            splash.Dispose();
            dummy.Dispose();
        }
    }
}