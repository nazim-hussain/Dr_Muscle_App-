using DrMuscle.Dependencies;
using DrMuscle.iOS.Renderer;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DpiService))]
namespace DrMuscle.iOS.Renderer
{
    public class DpiService : IDpiService
    {
        public int GetDpi()
        {
            var scale = UIScreen.MainScreen.Scale;
            var dpi = (int)(scale * 160); // 160 is the standard DPI value
            return dpi;
        }
    }
}