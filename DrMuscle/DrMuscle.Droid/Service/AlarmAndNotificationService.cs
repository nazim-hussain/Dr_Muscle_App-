using System;
using Android.App;
using Android.Content;
using Android.Icu.Util;
using DrMuscle.Services;
using Java.Util;
using DrMuscle.Droid.BroadcastReceiver;
using DrMuscle.Droid.Services;
using Xamarin.Forms;
using static Android.App.AlarmManager;

[assembly: Dependency(typeof(DrMuscle.Droid.Services.AlarmAndNotificationService))]
namespace DrMuscle.Droid.Services
{
    public class AlarmAndNotificationService : IAlarmAndNotificationService
    {
        public void ScheduleNotification(string title, string message, TimeSpan timespan, int notificationId, NotificationInterval interval = NotificationInterval.Day, string extra = "")
        {
            ScheduleLocalNotification(title, message, timespan, notificationId, interval,extra);
        }

        public void CancelNotification(int notificationId)
        {
            CancelScheduledNotification(notificationId);
        }

        private void ScheduleLocalNotification(string notificationTitle, string notificationMessage, TimeSpan timeSpan, int notificationId, NotificationInterval interval, string extra = "")
        {            
            DateTime utcDateTime = new DateTime(DateTime.Today.Ticks + timeSpan.Ticks).ToUniversalTime();
            Java.Util.Date nativeDate = DateTimeToNativeDate(utcDateTime);

            Intent alarmReciver = new Intent(Forms.Context, typeof(AlarmReceiver));
            alarmReciver.PutExtra("Title", notificationTitle);
            alarmReciver.PutExtra("Message", notificationMessage);
            alarmReciver.PutExtra("NotificationId", notificationId);
            alarmReciver.PutExtra("Time", nativeDate.ToString());
            alarmReciver.PutExtra("PageKey", "Local");
            alarmReciver.PutExtra("Extra", extra);
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Forms.Context, notificationId, alarmReciver, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            AlarmClockInfo alarmClockInfo = new AlarmClockInfo(nativeDate.Time, pendingIntent);

            var alarmManager = (AlarmManager)Forms.Context.GetSystemService(Context.AlarmService);

            if (interval == NotificationInterval.Day)
                alarmManager.SetAlarmClock(alarmClockInfo, pendingIntent);
            else if (interval == NotificationInterval.Hours)
                alarmManager.SetRepeating(AlarmType.RtcWakeup, nativeDate.Time, IntervalHour, pendingIntent);
            else if (interval == NotificationInterval.Week)
                alarmManager.SetRepeating(AlarmType.RtcWakeup, nativeDate.Time, IntervalDay * 7, pendingIntent);

            //alarmManager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, nativeDate.Time, AlarmManager.IntervalDay, pendingIntent);

            Forms.Context.RegisterReceiver(new BroadcastReceiver.AlarmReceiver(), new IntentFilter());
            
        }

        private void CancelScheduledNotification(int notificationId)
        {
            Intent alarmReciver = new Intent(Forms.Context, typeof(AlarmReceiver));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Forms.Context, notificationId, alarmReciver, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);

            if (pendingIntent != null)
            {
                var alarmManager = (AlarmManager)Forms.Context.GetSystemService(Context.AlarmService);
                alarmManager.Cancel(pendingIntent);
            }    
        }

        /// <summary>
        /// Converts a UTC datestamp to the local timezone
        /// </summary>
        /// <returns>The UTC to local time zone.</returns>
        /// <param name="dateTimeUtc">Date time UTC.</param>
        public DateTime ConvertUTCToLocalTimeZone(DateTime dateTimeUtc)
        {

            // get the UTC/GMT Time Zone
            Java.Util.TimeZone utcGmtTimeZone = Java.Util.TimeZone.GetTimeZone("UTC");

            // get the local Time Zone
            Java.Util.TimeZone localTimeZone = Java.Util.TimeZone.Default;

            // convert the DateTime to Java type
            Date javaDate = DateTimeToNativeDate(dateTimeUtc);

            // convert to new time zone
            Date timeZoneDate = ConvertTimeZone(javaDate, utcGmtTimeZone, localTimeZone);

            // convert to systwem.datetime
            DateTime timeZoneDateTime = NativeDateToDateTime(timeZoneDate);

            return timeZoneDateTime;
        }

        /// <summary>
        /// Converts a System.DateTime to a Java DateTime
        /// </summary>
        /// <returns>The time to native date.</returns>
        /// <param name="date">Date.</param>
        public static Java.Util.Date DateTimeToNativeDate(DateTime date)
        {
            long dateTimeUtcAsMilliseconds = (long)date.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
            return new Date(dateTimeUtcAsMilliseconds);
        }

        /// <summary>
        /// Converts a java datetime to system.datetime
        /// </summary>
        /// <returns>The date to date time.</returns>
        /// <param name="date">Date.</param>
        public static DateTime NativeDateToDateTime(Java.Util.Date date)
        {
            long javaDateAsMilliseconds = date.Time;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromMilliseconds(javaDateAsMilliseconds));
            return dateTime;
        }

        /// <summary>
        /// Converts a date between time zones
        /// </summary>
        /// <returns>The date in the converted timezone.</returns>
        /// <param name="date">Date to convert</param>
        /// <param name="fromTZ">from Time Zone</param>
        /// <param name="toTZ">To Time Zone</param>
        public static Java.Util.Date ConvertTimeZone(Java.Util.Date date, Java.Util.TimeZone fromTZ, Java.Util.TimeZone toTZ)
        {
            long fromTZDst = 0;

            if (fromTZ.InDaylightTime(date))
            {
                fromTZDst = fromTZ.DSTSavings;
            }

            long fromTZOffset = fromTZ.RawOffset + fromTZDst;

            long toTZDst = 0;
            if (toTZ.InDaylightTime(date))
            {
                toTZDst = toTZ.DSTSavings;
            }

            long toTZOffset = toTZ.RawOffset + toTZDst;

            return new Java.Util.Date(date.Time + (toTZOffset - fromTZOffset));
        }

        public void ScheduleOnceNotification(string notificationTitle, string notificationMessage, TimeSpan timeSpan, int notificationId, string extra)
        {
            DateTime utcDateTime = new DateTime(DateTime.Today.Ticks + timeSpan.Ticks).ToUniversalTime();
            Java.Util.Date nativeDate = DateTimeToNativeDate(utcDateTime);

            Intent alarmReciver = new Intent(Forms.Context, typeof(AlarmReceiver));
            alarmReciver.PutExtra("Title", notificationTitle);
            alarmReciver.PutExtra("Message", notificationMessage);
            alarmReciver.PutExtra("NotificationId", notificationId);
            alarmReciver.PutExtra("Time", nativeDate.ToString());
            alarmReciver.PutExtra("PageKey", "Local");
            alarmReciver.PutExtra("Extra", extra);

            //System.Diagnostics.Debug.WriteLine($"Schedule LocalNotification : PageKey : {pageKey}, for time : {nativeDate.ToString()}");


            PendingIntent broadcast = PendingIntent.GetBroadcast(Forms.Context, notificationId, alarmReciver, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            AlarmClockInfo alarmClockInfo = new AlarmClockInfo(nativeDate.Time, broadcast);

            var alarmManager = (AlarmManager)Forms.Context.GetSystemService(Context.AlarmService);
            //if (interval == NotificationInterval.Day)
            //    alarmManager.SetAlarmClock(alarmClockInfo, pendingIntent);
            //else if (interval == NotificationInterval.Hours)
            //    alarmManager.SetRepeating(AlarmType.RtcWakeup, nativeDate.Time, IntervalHour, pendingIntent);
            //else if (interval == NotificationInterval.Week)
            //    alarmManager.SetRepeating(AlarmType.RtcWakeup, nativeDate.Time, IntervalDay * 7, pendingIntent);
            alarmManager.Set(AlarmType.RtcWakeup, nativeDate.Time, broadcast);
            Forms.Context.RegisterReceiver(new BroadcastReceiver.AlarmReceiver(), new IntentFilter());
        }
    }
}
