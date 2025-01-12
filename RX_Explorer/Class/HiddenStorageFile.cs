﻿using RX_Explorer.Interface;
using ShareClassLibrary;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace RX_Explorer.Class
{
    public class HiddenStorageFile : FileSystemStorageFile, IHiddenStorageItem
    {
        protected HiddenDataPackage RawData { get; set; }

        public override string DisplayType => (RawData?.DisplayType) ?? Type;

        protected override async Task LoadCoreAsync(bool ForceUpdate)
        {
            if (RawData == null || ForceUpdate)
            {
                RawData = await GetRawDataAsync();
            }
        }

        public override async Task<BitmapImage> GetThumbnailAsync(ThumbnailMode Mode)
        {
            if ((RawData?.IconData.Length).GetValueOrDefault() > 0)
            {
                using (MemoryStream IconStream = new MemoryStream(RawData.IconData))
                {
                    BitmapImage Image = new BitmapImage();
                    await Image.SetSourceAsync(IconStream.AsRandomAccessStream());
                    return Image;
                }
            }
            else
            {
                return null;
            }
        }

        public override Task<IRandomAccessStream> GetThumbnailRawStreamAsync(ThumbnailMode Mode)
        {
            if ((RawData?.IconData.Length).GetValueOrDefault() > 0)
            {
                using (MemoryStream IconStream = new MemoryStream(RawData.IconData))
                {
                    return Task.FromResult(IconStream.AsRandomAccessStream());
                }
            }
            else
            {
                return null;
            }
        }

        public override Task<IStorageItem> GetStorageItemAsync()
        {
            return Task.FromResult<IStorageItem>(null);
        }

        public override void SetThumbnailOpacity(ThumbnailStatus Status)
        {
            ThumbnailOpacity = 0.5;
            OnPropertyChanged(nameof(ThumbnailOpacity));
        }

        public async Task<HiddenDataPackage> GetRawDataAsync()
        {
            using (RefSharedRegion<FullTrustProcessController.ExclusiveUsage> ControllerRef = GetProcessRefShareRegion())
            {
                if (ControllerRef != null)
                {
                    return await ControllerRef.Value.Controller.GetHiddenItemDataAsync(Path);
                }
                else
                {
                    using (FullTrustProcessController.ExclusiveUsage Exclusive = await FullTrustProcessController.GetAvailableControllerAsync())
                    {
                        return await Exclusive.Controller.GetHiddenItemDataAsync(Path);
                    }
                }
            }
        }

        public HiddenStorageFile(Win32_File_Data Data) : base(Data)
        {
            base.SetThumbnailOpacity(ThumbnailStatus.ReducedOpacity);
        }
    }
}
