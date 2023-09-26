using System;
using Android.OS;
using DrMuscle.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(DrMuscle.Droid.Service.NotificationRequestService))]

namespace DrMuscle.Droid.Service
{
    public class NotificationRequestService : INotificationRequestService
    {
        public NotificationRequestService()
        {
        }

        public void RequestNotificationRequest()
        {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                MainActivity._currentActivity.ShouldShowRequestPermissionRationale("android.permission.POST_NOTIFICATIONS");
            }
        }
    }
}

