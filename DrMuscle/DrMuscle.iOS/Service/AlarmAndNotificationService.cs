
using System;
using System.Linq;
using Foundation;
using DrMuscle.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DrMuscle.iOS.Services.AlarmAndNotificationService))]
namespace DrMuscle.iOS.Services
{
    public class AlarmAndNotificationService : IAlarmAndNotificationService
    {
        private const string NotificationKey = "DrMuscleNotification";

        public void ScheduleNotification(string title, string message, TimeSpan timespan, int notificationId, NotificationInterval interval = NotificationInterval.Day, string extra = "")
        {
            CancelNotification(notificationId);
            ScheduleLocalNotification(title, message, timespan, notificationId, interval,extra);
        }

        public void CancelNotification(int notificationId)
        {
            CancelScheduledNotification(notificationId);
        }

        private void ScheduleLocalNotification(string notificationTitle, string notificationMessage, TimeSpan timeSpan, int notificationId, NotificationInterval interval,string extra="")
        {
            NSCalendarUnit calenderUnit = NSCalendarUnit.Day;
            switch (interval)
            {
                case NotificationInterval.Day:
                    calenderUnit = NSCalendarUnit.Day;
                    break;
                case NotificationInterval.Week:
                    calenderUnit = NSCalendarUnit.Week;
                    break;
                case NotificationInterval.Hours:
                    calenderUnit = NSCalendarUnit.Hour;
                    break;
                default:
                    calenderUnit = NSCalendarUnit.Day;
                    break;
            }

            UILocalNotification notification = new UILocalNotification();
            notification.AlertTitle = notificationTitle;
            notification.AlertBody = notificationMessage;
            notification.SoundName = UILocalNotification.DefaultSoundName;
            notification.RepeatInterval = calenderUnit;
            notification.ApplicationIconBadgeNumber = 1;

            var keys = new NSString[]
            {
                new NSString($"{NotificationKey}{notificationId}"),
                new NSString("Extra")
            };

            var values = new NSString[]
            {
                new NSString(notificationId.ToString()),
                new NSString(extra)
            };

            NSDictionary dictionary = new NSDictionary<NSString, NSString>(keys, values);
            notification.UserInfo = dictionary;
            DateTime utcDatetime = new DateTime(DateTime.Today.Ticks + timeSpan.Ticks).ToUniversalTime();

            NSDate nsDate = Extensions.DateTimeToNSDate(utcDatetime);
            notification.FireDate = nsDate;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        private void CancelScheduledNotification(int notificationId)
        {
            try
            {

            var allScheduledNotifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
            if (allScheduledNotifications.Count() == 0)
                return;

            foreach (var scheduledNotification in allScheduledNotifications)
            {
                if (scheduledNotification.UserInfo != null && scheduledNotification.UserInfo.ContainsKey(new NSString($"{NotificationKey}{notificationId}")))
                    UIApplication.SharedApplication.CancelLocalNotification(scheduledNotification);
            }

            }
            catch (Exception ex)
            {

            }
        }

        public void ScheduleOnceNotification(string notificationTitle, string notificationMessage, TimeSpan timeSpan, int notificationId, string extra="")
        {
            UILocalNotification notification = new UILocalNotification();
            notification.AlertTitle = notificationTitle;
            notification.AlertBody = notificationMessage;
            notification.SoundName = UILocalNotification.DefaultSoundName;
            notification.ApplicationIconBadgeNumber = 1;

            var keys = new NSString[]
            {
                new NSString($"{NotificationKey}{notificationId}"),
                new NSString($"Extra")
            };

            var values = new NSString[]
            {
                new NSString(notificationId.ToString()),
                new NSString(extra)
            };

           
            NSDictionary dictionary = new NSDictionary<NSString, NSString>(keys, values);
            notification.UserInfo = dictionary;
            DateTime utcDatetime = new DateTime(DateTime.Today.Ticks + timeSpan.Ticks).ToUniversalTime();

            NSDate nsDate = Extensions.DateTimeToNSDate(utcDatetime);
            notification.FireDate = nsDate;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }
    }

    public static class Extensions
    {
        public static NSDate DateTimeToNSDate(this DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);
            return (NSDate)date;
        }

        public static DateTime NSDateToDateTime(this NSDate date)
        {
            return ((DateTime)date).ToLocalTime();
        }
    }
}
