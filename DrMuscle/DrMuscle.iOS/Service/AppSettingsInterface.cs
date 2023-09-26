using System;
using DrMuscle.Dependencies;
using DrMuscle.iOS.Service;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppSettingsInterface))]

namespace DrMuscle.iOS.Service
{
    public class AppSettingsInterface : IAppSettingsHelper
    {
        public void OpenAppSettings()
        {
            var url = new NSUrl($"app-settings:");
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}

