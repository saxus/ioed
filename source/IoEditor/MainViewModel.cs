using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IoEditor.Model;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Model;
using IoEditor.Models.Studio;
using IoEditor.UI.LoaderWindow;
using IoEditor.UI.Utils;

using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace IoEditor
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly PartImageCache _partImageCache = new PartImageCache();
        private readonly StudioOptions _options;
        private readonly PartLibrary _partLibrary;
        private readonly ColorLibrary _colorLibrary;

        

        #region Commands
        public ICommand OpenFilesCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion


        #region propeties

        private IoEdProject _project;
        public IoEdProject Project
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

        public MainViewModel(IOptions<StudioOptions> options)
        {
            _options = options.Value;
            _partLibrary = new PartLibrary(_options);
            _colorLibrary = new ColorLibrary(_options);

            OpenFilesCommand = new DelegateCommand(OpenFilesCmd);
            SaveFileCommand = new DelegateCommand(SaveFileCmd);
            SaveAsCommand = new DelegateCommand(SaveAsCmd); 
            ExitCommand = new DelegateCommand(ExitCmd);
        }

        #region Command handlers
        private void ExitCmd(object obj)
        {
            Application.Current.Shutdown();
        }

        private void SaveFileCmd(object obj)
        {
            throw new NotImplementedException("Save not implemented");
        }

        private void SaveAsCmd(object obj)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Studio file (*.io)|*.io",
                Title = "Save As"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            string filePath = saveFileDialog.FileName;

            if (File.Exists(filePath))
            {
                var result = MessageBox.Show("File already exists. Do you want to overwrite?", "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            try
            {
                throw new NotImplementedException("Save not implemented");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                Project = IoEdProjectLoader.Load(reference, target);

                UpdateImageCache(Project.Target);
                UpdateImageCache(Project.Reference);

                var stepB = new IndexedStepsBuilder(_partLibrary, _colorLibrary);
                var list = stepB.CreateIndexedSteps(Project.Reference.MainModel, Project.Reference);

                foreach (var l in list)
                {
                    Console.WriteLine($"#{l.Index}");
                    foreach (var p in l.Submodels)
                    {
                        Console.WriteLine($"\tS\t{p.Quantity} x {p.ModelName}");
                    }
                    foreach (var p in l.Parts)
                    {
                        Console.WriteLine($"\tP\t{p.Quantity} x {p.Color?.BLColorName} x \t{p.Part?.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening files: {ex.Message}");
            }
        }

        public async void UpdateImageCache(StudioFile studioFile)
        {
            var tasks = new List<Task>();

            foreach (var model in studioFile.Models.Values)
            {
                foreach (var part in model.Steps.SelectMany(step => step.Parts)
                                                .Where(x => x.IsOfficialPart))
                {
                    if (part.Image == null)
                    {
                        tasks.Add(UpdatePartImageAsync(part));
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task UpdatePartImageAsync(LDrawPart part)
        {
            var ldrawColorId = part.LDrawColorId;
            var blColorId = _colorLibrary.GetColorByLDrawColorCode(ldrawColorId).BLColorCode.Value;

            var image = await _partImageCache.LoadImageAsync(part.PartName, blColorId);
            if (image != null)
            {
                Application.Current.Dispatcher.Invoke(() => part.Image = image);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
