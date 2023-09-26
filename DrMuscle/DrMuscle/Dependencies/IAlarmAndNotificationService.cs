using System;

namespace DrMuscle.Services
{
    public enum NotificationInterval
    {
        Day,
        Week,
        Hours
    }

    public interface IAlarmAndNotificationService
    {
        void ScheduleNotification(string title, string message, TimeSpan timespan, int notificationId, NotificationInterval interval = NotificationInterval.Day, string extra = "");

        void ScheduleOnceNotification(string title, string message, TimeSpan timespan, int notificationId, string extra = "");

        void CancelNotification(int notificationId);
    }
}
