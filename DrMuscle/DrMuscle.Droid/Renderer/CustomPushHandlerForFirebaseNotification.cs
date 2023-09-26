using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.App;
//using Plugin.FirebasePushNotification;

namespace DrMuscle.Droid.BroadcastReceiver
{
    //public class CustomPushHandlerForFirebaseNotification : DefaultPushNotificationHandler
    //{

    //    public override void OnBuildNotification(Android.Support.V4.App.NotificationCompat.Builder notificationBuilder, System.Collections.Generic.IDictionary<string, object> parameters)
    //    {
    //        base.OnBuildNotification(notificationBuilder, parameters);
    //        //string stringData = string.Empty;
    //        //foreach(var item in parameters)
    //        //    stringData += $"{item.Key}: {item.Value}\n";

    //        //string pageKey = string.Empty;
    //        //if(parameters.ContainsKey("PageKey"))
    //        //    pageKey = (string)parameters["PageKey"];

    //        //System.Diagnostics.Debug.WriteLine($"Notification Data : {stringData}");
    //        var mainIntent = new Intent(Application.Context, typeof(MainActivity));
    //        mainIntent.PutExtra("PageKey", "chat");

    //        var mainPendingIntent = PendingIntent.GetActivity(Application.Context, 0, mainIntent, PendingIntentFlags.UpdateCurrent);

    //        var largeIcon = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.icon);

    //        notificationBuilder.SetLargeIcon(largeIcon)
    //                           .SetSmallIcon(Resource.Drawable.icon_notification)
    //                           .SetAutoCancel(true)
    //                           .SetDefaults((int)NotificationDefaults.Sound)
    //                           .SetContentIntent(mainPendingIntent);
    //        try
    //        {
    //            //var notificationManager = NotificationManagerCompat.From(MainActivity._currentActivity);
    //           // notificationManager.Notify(0, notificationBuilder.Build());
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }

    //}
}
