using IoEditor.Model;
using IoEditor.Models.Studio;

using System.IO;

namespace IoEditor.Models.Instructions
{
    internal class ImageResourceMerger
    {
        private IoEdProject _project;

        private int _counter = 0;
        private Dictionary<ImageSource, Dictionary<string, string>> _images;

        public ImageResourceMerger(IoEdProject project)
        {
            this._project = project;
            this._images = new Dictionary<ImageSource, Dictionary<string, string>>();
        }

        public string RegisterImage(ImageSource source, string name)
        {
            if (!_images.TryGetValue(source, out var mappings))
            {
                mappings = new Dictionary<string, string>();
                _images.Add(source, mappings);
            }

            if (!mappings.TryGetValue(name, out var value))
            {
                var file = GetStudioFile(source);
                if (!file.ImageResources.ContainsKey(name))
                {
                    throw new InvalidOperationException($"ImageResource file not found in {source}: {name}");
                }

                _counter++;
                var extension = Path.GetExtension(name);
                var newName = $"ImageResource/{_counter}{extension}";

                mappings.Add(name, newName);

                return newName;
            }
            else
            {
                return value;
            }
        }

        private StudioFile GetStudioFile(ImageSource imageSource)
            => imageSource switch
            {
                ImageSource.Reference => _project.Reference,
                ImageSource.Target => _project.Target,
                _ => throw new NotImplementedException($"Unknown image source: {imageSource}")
            };

        public Dictionary<string, byte[]> GetMergedDictionary()
        {
            var result = new Dictionary<string, byte[] >();

            foreach (var mappings in _images)
            {
                var file = GetStudioFile(mappings.Key);

                foreach (var mappedFile in mappings.Value)
                {
                    var newFileName = mappedFile.Value;
                    var content = file.ImageResources[mappedFile.Key];

                    result.Add(newFileName, content);
                }
            }

            return result;            
        }
    }
}
