using System.Configuration;
using System.Data;
using System.Windows;

namespace OPSKWA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the screen with the mouse cursor
            var point = new System.Drawing.Point(
                System.Windows.Forms.Cursor.Position.X,
                System.Windows.Forms.Cursor.Position.Y);

            System.Windows.Forms.Screen targetScreen = null;
            foreach (var scr in System.Windows.Forms.Screen.AllScreens)
            {
                if (scr.Bounds.Contains(point))
                {
                    targetScreen = scr;
                    break;
                }
            }

            if (targetScreen == null)
                targetScreen = Screen.PrimaryScreen;

            var splash = new SplashScreen();
            splash.Left = targetScreen.WorkingArea.Left + (targetScreen.WorkingArea.Width - splash.Width) / 2;
            splash.Top = targetScreen.WorkingArea.Top + (targetScreen.WorkingArea.Height - splash.Height) / 2;
            splash.Show();

            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(10000); // Simulate loading

                Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow(targetScreen);
                    mainWindow.Show();
                    splash.Close();
                });
            });
        }
    }

}
