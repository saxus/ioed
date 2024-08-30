using IoEditor.Models.Configuration;
using IoEditor.UI.MainWindow;
using IoEditor.UI.SettingsWindow;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Windows;

namespace IoEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        private readonly string _configFilePath;


        public App()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string configFolderPath = Path.Combine(appDataPath, "IoEditor");
            _configFilePath = Path.Combine(configFolderPath, "appsettings.json");

            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            if (!File.Exists(_configFilePath))
            {
                var defaultConfig = new
                {
                    StudioOptions = new
                    {
                    }
                };
                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }

            _host = Host.CreateDefaultBuilder()
               .ConfigureAppConfiguration((context, config) =>
               {
                   config.AddJsonFile(_configFilePath, optional: false, reloadOnChange: true);
               })
               .ConfigureServices((context, services) =>
               {
                   services.Configure<StudioOptions>(context.Configuration.GetSection("StudioOptions"));
                   services.AddSingleton<MainViewModel>();
                   services.AddSingleton<MainWindow>();
               })
               .Build();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Handle global unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            var options = _host.Services.GetRequiredService<IOptions<StudioOptions>>().Value;

            if (!ConfigurationValidator.Validate(options))
            {
                var settingsViewModel = new SettingsViewModel(_host.Services.GetRequiredService<IOptions<StudioOptions>>(), _configFilePath);
                var settingsWindow = new SettingsWindow
                {
                    DataContext = settingsViewModel
                };

                bool? result = settingsWindow.ShowDialog();
                if (result != true)
                {
                    Shutdown();
                    return;
                }
            }

            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel;
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled UI Exception: {e.Exception.Message}");
            Console.WriteLine($"Stack Trace: {e.Exception.StackTrace}");
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine($"Unhandled Non-UI Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }

}
