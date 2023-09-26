using System;
using DrMuscle.Dependencies;
using DrMuscle.iOS;
using DrMuscle.iOS.Service;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationsInterface))]
namespace DrMuscle.iOS.Service
{
    public class NotificationsInterface : INotificationsInterface
    {
        public NotificationsInterface()
        {
        }

        public bool registeredForNotifications()
        {
            UIUserNotificationType types = UIApplication.SharedApplication.CurrentUserNotificationSettings.Types;
            if (types.HasFlag(UIUserNotificationType.Alert))
            {
                return true;
            }
            return false;
        }
    }
}

