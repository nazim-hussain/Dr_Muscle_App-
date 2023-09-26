using System;
using System.IO;
using DrMuscle.Dependencies;
using DrMuscle.iOS.Renderer;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenshotService))]
namespace DrMuscle.iOS.Renderer
{
    public class ScreenshotService : IScreenshotService
    {
        public byte[] Capture()
        {
            var capture = UIScreen.MainScreen.Capture();
            using (NSData data = capture.AsPNG())
            {
                var bytes = new byte[data.Length];
                System.Runtime.InteropServices.Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
                return bytes;
            }
        }
    }
}
