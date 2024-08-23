using System.Configuration;
using System.Data;
using System.Windows;

namespace IoEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel();
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();

            // Handle command line arguments
            var args = e.Args;
            if (args.Length == 3 && args[0].Equals("/openfiles", StringComparison.OrdinalIgnoreCase))
            {
                string referenceFile = args[1];
                string targetFile = args[2];
                mainViewModel.OpenFiles(referenceFile, targetFile);
            }
        }
    }

}
