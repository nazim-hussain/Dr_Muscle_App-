using System;

using Android.App;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using DrMuscle.Droid.BroadcastReceiver;
using Plugin.CurrentActivity;
using Plugin.FirebasePushNotification;
//using Plugin.FirebasePushNotification;

namespace DrMuscle.Droid
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                
                //Change for your default notification channel id here
                //FirebasePushNotificationManager.DefaultNotificationChannelId = "123";
                //FirebasePushNotificationManager.DefaultNotificationChannelImportance = NotificationImportance.High;
                ////Change for your default notification channel name here
                //FirebasePushNotificationManager.DefaultNotificationChannelName = "1-on-1 chat support";

            }
            RegisterActivityLifecycleCallbacks(this);

            CrossCurrentActivity.Current.Init(this);

            

            //A great place to initialize Xamarin.Insights and Dependency Services!
            //            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this) == ConnectionResult.Success)
            //            {
            //#if DEBUG

            ////               FirebasePushNotificationManager.Initialize(this, new CustomPushHandlerForFirebaseNotification(),false);
            ////#else
            ////            FirebasePushNotificationManager.Initialize(this, new CustomPushHandlerForFirebaseNotification(), false);
            //#endif
            //            }
            //If debug you should reset the token each time.
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true);
#else
              FirebasePushNotificationManager.Initialize(this,false);
#endif

            //Handle notification when app is closed here
            CrossFirebasePushNotification.Current.OnNotificationReceived += Current_OnNotificationReceived;
        }

        private void Current_OnNotificationReceived(object source, FirebasePushNotificationDataEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Notification Received : {e.Data}");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}