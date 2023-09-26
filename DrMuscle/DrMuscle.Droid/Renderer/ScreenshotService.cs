using System;
using System.IO;
using Android.App;
using Android.Graphics;
using DrMuscle.Dependencies;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenshotService))]
namespace DrMuscle.Droid.Renderer
{
public class ScreenshotService : IScreenshotService
    {


        public byte[] Capture()
        {
            var rootView = MainActivity._currentActivity.Window.DecorView.RootView;

            using (var screenshot = Bitmap.CreateBitmap(
                                    rootView.Width,
                                    rootView.Height,
                                    Bitmap.Config.Argb8888))
            {
                var canvas = new Canvas(screenshot);
                rootView.Draw(canvas);

                using (var stream = new MemoryStream())
                {
                    screenshot.Compress(Bitmap.CompressFormat.Png, 90, stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
