using System;
using Xamarin.Forms;

namespace DrMuscle.Services
{
    public class AlarmAndNotificationService : IAlarmAndNotificationService
    {
        private readonly IAlarmAndNotificationService _platformAlarmService;

        public AlarmAndNotificationService()
        {
            _platformAlarmService = DependencyService.Get<IAlarmAndNotificationService>();
        }

        public void CancelNotification(int notificationId)
        {
            _platformAlarmService.CancelNotification(notificationId);
        }

        public void ScheduleNotification(string title, string message, TimeSpan timespan, int notificationId, NotificationInterval interval = NotificationInterval.Day, string extra="")
        {
            _platformAlarmService.ScheduleNotification(title, message, timespan, notificationId, interval, extra);
        }

        public void ScheduleOnceNotification(string title, string message, TimeSpan timespan, int notificationId, string extra="")
        {
            _platformAlarmService.ScheduleOnceNotification(title, message, timespan, notificationId, extra);
        }
    }
}
