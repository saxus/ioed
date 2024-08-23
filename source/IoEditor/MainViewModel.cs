using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IoEditor.Model;
using IoEditor.UI.LoaderWindow;
using IoEditor.UI.Utils;

namespace IoEditor
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Commands
        public ICommand OpenFilesCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion


        #region propeties

        private StudioProject _project;
        public StudioProject Project
        {
            get { return _project; }
            set
            {
                if (_project != value)
                {
                    _project = value;
                    RaisePropertyChanged(nameof(Project));
                }
            }
        }

        #endregion

        public MainViewModel()
        {
            OpenFilesCommand = new DelegateCommand(OpenFilesCmd);
            SaveFileCommand = new DelegateCommand(SaveFileCmd);
            ExitCommand = new DelegateCommand(ExitCmd);
        }

        #region Command handlers
        private void ExitCmd(object obj)
        {
            Application.Current.Shutdown();
        }

        private void SaveFileCmd(object obj)
        {
            throw new NotImplementedException();
        }

        private void OpenFilesCmd(object obj)
        {
            var vm = new LoaderViewModel();
            var win = new LoaderWindow() { DataContext = vm };
            var dr = win.ShowDialog();

            if (dr == true)
            {
                OpenFiles(vm.ReferenceFile, vm.TargetFile);
            }
        }

        #endregion


        public void OpenFiles(string reference, string target)
        {
            try
            {
                Project = StudioProject.Open(reference, target);
                // Additional logic to handle the opened project if needed
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user, etc.)
                Console.WriteLine($"Error opening files: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
