using System;
using DrMuscle.Dependencies;
using DrMuscle.iOS.Service;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(VersionInfoService))]
namespace DrMuscle.iOS.Service
{
    public class VersionInfoService : IVersionInfoService
    {
        public string GetDeviceUniqueId()
        {
            return UIDevice.CurrentDevice.IdentifierForVendor.ToString();
        }

        public int GetVersionInfo()
        {
            try
            {

            var ary = string.Format("{0}", NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleShortVersionString")]).Split('.');
            return int.Parse(ary[ary.Length - 1]);

            }
            catch (Exception ex)
            {

            }
            return 0;
        }
    }
}
