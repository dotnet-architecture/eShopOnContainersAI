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
        //public bool CanTakePhoto => CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        //public bool CanPickPhoto => CrossMedia.Current.IsPickPhotoSupported;

        public FiltersView()
        {
            var result = CrossMedia.Current.Initialize().Result;
            InitializeComponent();

            //if (CanTakePhoto)
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                System.EventHandler p = async (sender, args) =>
                   {
                       var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
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
                takePhoto.Clicked += p;
            }
                

            //if (CanPickPhoto)
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                System.EventHandler p1 = async (sender, args) =>
                   {
                       var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                       {
                           PhotoSize = PhotoSize.Full,

                       });

                       await SetImageFilter(file);
                   };
                pickPhoto.Clicked += p1;

            }
                
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
