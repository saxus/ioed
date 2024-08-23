using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoEditor.UI.Utils;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Windows;
using System.Reflection.Metadata;

namespace IoEditor.UI.LoaderWindow
{
    internal class LoaderViewModel : INotifyPropertyChanged
    {
        private string? _referenceFile;
        public string? ReferenceFile
        {
            get { return _referenceFile; }
            set
            {
                if (_referenceFile != value)
                {
                    _referenceFile = value;
                    RaisePropertyChanged(nameof(ReferenceFile));
                }
            }
        }

        private string? _targetFile;
        public string? TargetFile
        {
            get { return _targetFile; }
            set
            {
                if (_targetFile != value)
                {
                    _targetFile = value;
                    RaisePropertyChanged(nameof(TargetFile));
                }
            }
        }

        public ICommand OpenCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseReferenceFileCommand { get; }
        public ICommand BrowseTargetFileCommand { get; }

        public LoaderViewModel()
        {
            OpenCommand = new DelegateCommand(Open);
            CancelCommand = new DelegateCommand(Cancel);
            BrowseReferenceFileCommand = new DelegateCommand(BrowseReferenceFile);
            BrowseTargetFileCommand = new DelegateCommand(BrowseTargetFile);
        }

        private void Open(object param)
        {
            if (!File.Exists(ReferenceFile))
            {
                MessageBox.Show($"Reference file does not exist: {ReferenceFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(TargetFile))
            {
                MessageBox.Show($"Target file does not exist: {TargetFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ReferenceFile == TargetFile)
            {
                MessageBox.Show("Reference file and Target file cannot be the same.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private void BrowseReferenceFile(object param)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ReferenceFile = openFileDialog.FileName;
            }
        }

        private void BrowseTargetFile(object param)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TargetFile = openFileDialog.FileName;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
