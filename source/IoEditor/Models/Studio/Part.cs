using IoEditor.Models.ImageCache;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.Studio
{
    internal class Part 
    {
        public int StudioItemNo { get; }
        public int BaseStudioItemNo { get; }
        public string BLItemNo { get; }
        public string BLItemKey { get; }
        public string LDrawItemNo { get; }
        public string LDDItemNo { get; }
        public string Description { get; }
        public string Options { get; }
        public int BLCatalogIndex { get; }
        public int BLCatalogSubIndex { get; }
        public int EasyModeIndex { get; }
        public bool IsAssembly { get; }
        public string FlexibleType { get; }
        public bool IsDecorated { get; }
        public int? XPCatalogIndex { get; }
        public int? XPCatalogSubIndex { get; }

        public Part(
            int studioItemNo,
            int baseStudioItemNo,
            string blItemNo,
            string blItemKey,
            string lDrawItemNo,
            string lddItemNo,
            string description,
            string options,
            int blCatalogIndex,
            int blCatalogSubIndex,
            int easyModeIndex,
            bool isAssembly,
            string flexibleType,
            bool isDecorated,
            int? xpCatalogIndex,
            int? xpCatalogSubIndex)
        {
            StudioItemNo = studioItemNo;
            BaseStudioItemNo = baseStudioItemNo;
            BLItemNo = blItemNo;
            BLItemKey = blItemKey;
            LDrawItemNo = lDrawItemNo;
            LDDItemNo = lddItemNo;
            Description = description;
            Options = options;
            BLCatalogIndex = blCatalogIndex;
            BLCatalogSubIndex = blCatalogSubIndex;
            EasyModeIndex = easyModeIndex;
            IsAssembly = isAssembly;
            FlexibleType = flexibleType;
            IsDecorated = isDecorated;
            XPCatalogIndex = xpCatalogIndex;
            XPCatalogSubIndex = xpCatalogSubIndex;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
