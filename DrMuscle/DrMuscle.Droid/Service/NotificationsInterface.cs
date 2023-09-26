using System;
using Android.Support.V4.App;
using DrMuscle.Dependencies;
using DrMuscle.Droid.Service;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationsInterface))]
namespace DrMuscle.Droid.Service
{
    public class NotificationsInterface : INotificationsInterface
    {
        public NotificationsInterface()
        {
        }

        public bool registeredForNotifications()
        {
            try
            {
                var nm = NotificationManagerCompat.From(Android.App.Application.Context);
                bool enabled = nm.AreNotificationsEnabled();
                return enabled;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

