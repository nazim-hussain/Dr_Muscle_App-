using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]

namespace DrMuscle.Droid.BroadcastReceiver
{
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND", Enabled = true, Exported = true)]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.REGISTRATION" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { "com.google.android.gcm.intent.RETRY" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { "android.intent.action.BOOT_COMPLETED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class AlarmReceiver : Android.Content.BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

            try
            {

                string ChannelName = "Workout time!";
                string ChannelId = "122";
                string Title = intent.GetStringExtra("Title");
            string Message = intent.GetStringExtra("Message");
            int NotificationId = intent.GetIntExtra("NotificationId", 0);
            string time = intent.GetStringExtra("Time");
            //string pageKey = intent.GetStringExtra("PageKey");
            Java.Util.Date nativeDate = null;
            if (!string.IsNullOrEmpty(time))
                nativeDate = new Java.Util.Date(time);

            DateTime currentTime = DateTime.Now;
            Bundle bundle = intent.Extras;
            var mainIntent = new Intent(context, typeof(MainActivity));
                mainIntent.PutExtra("NotificationId", NotificationId);
            try
            {
                string name = intent.Extras.GetString("sendbird");
                if (!string.IsNullOrEmpty(name))
                {
                        if (name.ToLower().Contains("support"))
                        {
                            ChannelId = "123";
                            ChannelName = "1-on-1 chat support";
                            mainIntent.PutExtra("PageKey", "chat");

                        }
                        else
                            mainIntent.PutExtra("PageKey", "Local");
                    }
                else if(NotificationId == 1151)
                    {
                        ChannelId = "124";
                        ChannelName = "Stay on track";
                        mainIntent.PutExtra("PageKey", "Local5th");
                    }
                else if (NotificationId == 1352)
                    {
                        ChannelId = "124";
                        ChannelName = "Stay on track";
                        mainIntent.PutExtra("PageKey", "Workout");
                        var workId = intent.GetStringExtra("Extra");
                        mainIntent.PutExtra("Extra", workId);
                    }
                else if (NotificationId == 1551 || NotificationId==1051)
                    { 
                        ChannelId = "124";
                        ChannelName = "Stay on track";
                    }
                else if(NotificationId == 1451 || NotificationId == 1651 || NotificationId == 1351)
                    {
                        ChannelId = "122";
                        ChannelName = "Workout time!";
                    }
                    else
                    mainIntent.PutExtra("PageKey", "Local");

                    //System.Diagnostics.Debug.WriteLine(intent.GetStringExtra("Workout"));
                    //System.Diagnostics.Debug.WriteLine(intent.Extras.GetString("Workout"));
                }
            catch (Exception ex)
            {
                mainIntent.PutExtra("PageKey", "Local");
            }
            

            var mainPendingIntent = PendingIntent.GetActivity(Application.Context, NotificationId, mainIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            //var manager = NotificationManagerCompat.From(context);
            var manager =
            (NotificationManager)context.GetSystemService(Context.NotificationService);
            if (!(nativeDate != null && nativeDate.Hours == currentTime.Hour && nativeDate.Minutes == currentTime.Minute))
                return;
            
            var style = new NotificationCompat.BigTextStyle();
            style.BigText(Message);

            //Generate a notification with just short text and small icon
            //var largeIcon = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.icon);
            var largeIcon = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.icon);
                //manager.DeleteNotificationChannel("123");

                var builder = new NotificationCompat.Builder(Application.Context)
                                                .SetLargeIcon(largeIcon)
                                                .SetSmallIcon(Resource.Drawable.icon_notification)
                                                .SetContentTitle(Title)
                                                .SetContentText(Message)
                                                .SetStyle(style)
                                                .SetPriority(NotificationCompat.PriorityHigh)
                                                .SetAutoCancel(false);
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                //NotificationChannel channel = new NotificationChannel(
                //                                    ChannelId,
                //                                    ChannelName,
                //                                    NotificationImportance.High);
                //manager.CreateNotificationChannel(channel);
                    //var Oldchannel = manager.GetNotificationChannel(ChannelId);
                builder.SetChannelId(ChannelId);
            }
            builder.SetDefaults((int)NotificationDefaults.Sound);
            builder.SetContentIntent(mainPendingIntent);

            var notification = builder.Build();
            manager.Notify(0, notification);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
