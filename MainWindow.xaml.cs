using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace OPSKWA
{
    public partial class MainWindow : Window
    {
        private OPSKWAController _opskwaController;

        public MainWindow(System.Windows.Forms.Screen targetScreen)
        {
            InitializeComponent();

            _opskwaController = new OPSKWAController(this);

            this.WindowStartupLocation = WindowStartupLocation.Manual;

            this.Left = targetScreen.WorkingArea.Left;
            this.Top = targetScreen.WorkingArea.Top;
            this.Width = targetScreen.WorkingArea.Width;
            this.Height = targetScreen.WorkingArea.Height;

            this.Loaded += (sender, e) =>
            {
                this.WindowState = WindowState.Maximized;
                _opskwaController.Initialize();
            };
        }
    }
}