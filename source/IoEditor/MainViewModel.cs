using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IoEditor.UI.LoaderWindow;
using IoEditor.UI.Utils;

namespace IoEditor
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public ICommand OpenFilesCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            OpenFilesCommand = new DelegateCommand(OpenFilesCmd);
            SaveFileCommand = new DelegateCommand(SaveFileCmd);
            ExitCommand = new DelegateCommand(ExitCmd);
        }

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
                Console.WriteLine(vm.ReferenceFile);
                Console.WriteLine(vm.TargetFile);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
