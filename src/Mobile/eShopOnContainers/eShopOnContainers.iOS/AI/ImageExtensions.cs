using CoreGraphics;
using CoreVideo;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(eShopOnContainers.iOS.AI.ImageClassifier))]
namespace eShopOnContainers.iOS.AI
{
    public static class ImageExtensions
    {
        public static CVPixelBuffer ToCVPixelBuffer(this UIImage image, CGSize size)
        {
            var attrs = new CVPixelBufferAttributes
            {
                CGImageCompatibility = true,
                CGBitmapContextCompatibility = true
            };
            var cgImg = image.Scale(size).CGImage;

            var pb = new CVPixelBuffer(cgImg.Width, cgImg.Height, CVPixelFormatType.CV32ARGB, attrs);
            pb.Lock(CVPixelBufferLock.None);
            var pData = pb.BaseAddress;
            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var ctxt = new CGBitmapContext(pData, cgImg.Width, cgImg.Height, 8, pb.BytesPerRow, colorSpace, CGImageAlphaInfo.NoneSkipFirst);
            ctxt.TranslateCTM(0, cgImg.Height);
            ctxt.ScaleCTM(1.0f, -1.0f);
            UIGraphics.PushContext(ctxt);
            image.Draw(new CGRect(0, 0, cgImg.Width, cgImg.Height));
            UIGraphics.PopContext();
            pb.Unlock(CVPixelBufferLock.None);

            return pb;
        }
    }
}