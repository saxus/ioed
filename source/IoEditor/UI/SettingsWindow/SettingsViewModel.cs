using IoEditor.Models.Configuration;
using IoEditor.UI.Utils;

using Microsoft.Extensions.Options;
using Microsoft.Win32;

using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace IoEditor.UI.SettingsWindow
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly StudioOptions _options;
        private readonly string _configFilePath;

        public SettingsViewModel(IOptions<StudioOptions> options, string configFilePath)
        {
            _options = options.Value;
            _configFilePath = configFilePath;
            StudioFolderPath = _options.StudioFolder;

            BrowseStudioFolderCommand = new DelegateCommand(BrowseStudioFolder);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private string _studioFolderPath;
        public string StudioFolderPath
        {
            get { return _studioFolderPath; }
            set
            {
                if (_studioFolderPath != value)
                {
                    _studioFolderPath = value;
                    RaisePropertyChanged(nameof(StudioFolderPath));
                }
            }
        }

        public ICommand BrowseStudioFolderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void BrowseStudioFolder(object param)
        {
            var folderDialog = new OpenFileDialog
            {
                Filter = "Studio Executable (studio.exe)|studio.exe",
                Title = "Select Studio Folder"
            };

            if (folderDialog.ShowDialog() == true)
            {
                string selectedPath = Path.GetDirectoryName(folderDialog.FileName);
                if (File.Exists(Path.Combine(selectedPath, "studio.exe")))
                {
                    StudioFolderPath = selectedPath;
                }
                else
                {
                    MessageBox.Show("The selected folder does not contain studio.exe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save(object param)
        {
            _options.StudioFolder = StudioFolderPath;

            var config = new
            {
                StudioOptions = _options
            };

            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, json);

            if (param is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void Cancel(object param)
        {
            if (param is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
