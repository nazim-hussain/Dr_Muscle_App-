using System;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using DrMuscle.Dependencies;
using DrMuscle.Droid.Service;
using Xamarin.Forms;
using static Android.Provider.Settings;

[assembly: Dependency(typeof(VersionInfoService))]
namespace DrMuscle.Droid.Service
{
    public class VersionInfoService : IVersionInfoService
	{
        string id = string.Empty;

        public VersionInfoService()
        {
        }

        public string GetDeviceUniqueId()
        {
            if (!string.IsNullOrWhiteSpace(id))
                return id;

            id = Android.OS.Build.Serial;
            if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
            {
                try
                {
                    var context = Android.App.Application.Context;
                    id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
                }
            }
            return id;
        }

        public int GetVersionInfo()
        {
            Context context = Forms.Context;
            PackageManager manager = context.PackageManager;
            PackageInfo i = manager.GetPackageInfo(context.PackageName, 0);
            return i.VersionCode;
        }
    }
}
