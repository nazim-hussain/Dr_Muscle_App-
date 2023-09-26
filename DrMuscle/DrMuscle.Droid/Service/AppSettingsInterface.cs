using System;
using Android.Content;
using DrMuscle.Dependencies;
using DrMuscle.Droid.Service;
using Xamarin.Forms;
using Application = Android.App.Application;


[assembly: Dependency(typeof(AppSettingsInterface))]
namespace DrMuscle.Droid.Service
{
    public class AppSettingsInterface : IAppSettingsHelper
    {
        public void OpenAppSettings()
        {
            try
            {

                var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                intent.AddFlags(ActivityFlags.NewTask);
                string package_name = "com.drmaxmuscle.dr_max_muscle";
                var uri = Android.Net.Uri.FromParts("package", package_name, null);
                intent.SetData(uri);
                Application.Context.StartActivity(intent);

            }
            catch (Exception ex)
            {

            }
        }

    }
}

