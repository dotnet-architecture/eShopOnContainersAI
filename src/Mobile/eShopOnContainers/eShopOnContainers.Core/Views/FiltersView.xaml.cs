using eShopOnContainers.Core.AI.ProductSearchImageBased;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SlideOverKit;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace eShopOnContainers.Core.Views
{
    public partial class FiltersView : SlideMenuView
    {
        public bool CanTakePhoto => CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        public bool CanPickPhoto => CrossMedia.Current.IsPickPhotoSupported;

        public FiltersView()
        {
            var result = CrossMedia.Current.Initialize().Result;
            InitializeComponent();

            if (CanTakePhoto)
                takePhoto.Clicked += async (sender, args) =>
                {
                    var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "eshop",
                        SaveToAlbum = true,
                        CompressionQuality = 75,
                        CustomPhotoSize = 50,
                        PhotoSize = PhotoSize.Full,
                        MaxWidthHeight = 1024,
                        DefaultCamera = CameraDevice.Rear
                    });

                    await SetImageFilter(file);
                };

            if (CanPickPhoto)
                pickPhoto.Clicked += async (sender, args) =>
                {
                    var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Full,

                    });

                    await SetImageFilter(file);
                };

        }

        private async Task SetImageFilter(MediaFile file)
        {
            if (file != null)
            {
                var ms = new MemoryStream();
                await file.GetStream().CopyToAsync(ms);

                var imageFilter = ms.ToArray();
                var bindingContext = BindingContext as ViewModels.CatalogViewModel;
                bindingContext.ImageFilter = imageFilter;
                ms.Dispose();

                bindingContext.ImageFilterSource = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });
            }
        }
    }
}
