using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Acr.UserDialogs;
using SegmentedControl.FormsPlugin.Android;
using Android.Content;
using DrMuscle.Dependencies;
using Xamarin.Facebook;
using Firebase.Analytics;
using Xamarin.Forms.Platform;
using Xamarin.Forms.Platform.Android;
//using Plugin.FirebasePushNotification;
using DrMuscle.Droid.Renderer;
using DrMuscle.Message;
using SendBird.SBJson;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Plugin.GoogleClient;
using DrMuscle.Screens.Workouts;
using Plugin.InAppBilling;
using Android.Util;
using Android.Support.V4.Content;
using Android.Gms.Common.Apis;
//using Android.Gms.Wearable;
using Newtonsoft.Json;
using DrMuscleWebApiSharedModel;
using Android.Gms.Common;
using Plugin.FirebasePushNotification;
using Plugin.CurrentActivity;
using System.Diagnostics;
using System.IO;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;
using Xamarin.Essentials;
using Android.Content.Res;
using Sentry;

namespace DrMuscle.Droid
{
    [Activity( ResizeableActivity = false, Label = "Dr. Muscle", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.FontScale, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop | LaunchMode.SingleTask,  WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.AdjustResize, Exported = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static ICallbackManager CallbackManager { get; private set; }

        public static Activity _currentActivity;
        public static DisplayMetrics displayMetrics;
        GoogleApiClient _client;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            CrossCurrentActivity.Current.Init(this, bundle);

            
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            displayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            _currentActivity = this;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                // Kill status bar underlay added by FormsAppCompatActivity
                // Must be done before calling FormsAppCompatActivity.OnCreate()
                var statusBarHeightInfo = typeof(FormsAppCompatActivity).GetField("statusBarHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (statusBarHeightInfo == null)
                {
                    statusBarHeightInfo = typeof(FormsAppCompatActivity).GetField("_statusBarHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                statusBarHeightInfo?.SetValue(this, 0);
            }
           
            Xamarin.Essentials.Platform.Init(this, bundle);
            base.OnCreate(bundle);
            DisplayCrashReport();

            App.ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density);
            App.ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);
            Android.Views.View decorView = Window.DecorView;
            //adjustFontScale(Forms.Context, Resources.Configuration);
            if ((int)Android.OS.Build.VERSION.SdkInt >= 19)// Check build version for Nav bar
            {
                var uiOptions = (int)decorView.SystemUiVisibility;
                var newUiOptions = (int)uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                newUiOptions |= (int)SystemUiFlags.Immersive;
                newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
                decorView.SystemUiVisibilityChange += (o,e) => {
                    if (App.IsNUX)//|| App.IsNewUser
                        decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
                };
            }

            

            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.SetFlags(WindowManagerFlags.HardwareAccelerated, WindowManagerFlags.HardwareAccelerated);

            App.StatusBarHeight = 0;// GetStatusBarHeight();

            Rg.Plugins.Popup.Popup.Init(this);
            //#if DEBUG
            //            GoogleClientManager.Initialize(this, null, "922219678053-9r7rci0tp6uvof04iplsp5bia2v6hpk3.apps.googleusercontent.com");
            //#else
            GoogleClientManager.Initialize(this);

            //#endif
            //GoogleClientManager.Initialize(this, null, "835317142650-moj1otrg06eplrfcs6oldm3molpouoo1.apps.googleusercontent.com");
            //
         //   Forms.SetFlags("FastRenderers_Experimental");
            global::Xamarin.Forms.Forms.Init(this, bundle);
            SegmentedControlRenderer.Init();
            DependencyService.Register<ScreenshotService>();

            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            UserDialogs.Init(() => (Activity)Forms.Context);
            FacebookSdk.SdkInitialize(this);

            MessagingCenter.Subscribe<ExerciseVideoPage>(this, "AllowLandscape", sender =>
{
    RequestedOrientation = ScreenOrientation.Landscape;
});

            MessagingCenter.Subscribe<ExerciseVideoPage>(this, "PreventLandscape", sender =>
            {
                RequestedOrientation = ScreenOrientation.Portrait;
            });

            this.Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn);
            LoadApplication(new App());
            try
            {
               
                HandleNotificationTapped(Intent);

            }
            catch (Exception ex)
            {

            }
            //Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            //AndroidBug5497WorkaroundForXamarinAndroid.assistActivity(this);

           

            IntentFilter filter = new IntentFilter(Intent.ActionSend);
            MessageReciever receiver = new MessageReciever(this);
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(receiver, filter);


           
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager =
           (NotificationManager)Forms.Context.GetSystemService(Context.NotificationService);

                NotificationChannel channel3 = new NotificationChannel(
                                                    "122",
                                                    "Workout time!",
                                                    NotificationImportance.High);
                manager.CreateNotificationChannel(channel3);

                NotificationChannel channel2 = new NotificationChannel(
                                                    "124",
                                                    "Stay on track",
                                                    NotificationImportance.High);
                manager.CreateNotificationChannel(channel2);
                manager.DeleteNotificationChannel("123");
                NotificationChannel channel1 = new NotificationChannel(
                                                    "123",
                                                    "1-on-1 chat support",
                                                    NotificationImportance.Default);
                manager.CreateNotificationChannel(channel1);

                
            }
            //try
            //{
            //    if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this) != ConnectionResult.Success)
            //    { 
            //    _client = new GoogleApiClient.Builder(this.ApplicationContext)
            //        .AddApi(WearableClass.Api)
            //        .Build();
            //    _client.Connect();
            //    }
            //}
            //catch (Exception ex)
            //{

            //}


            MessagingCenter.Subscribe<SendWatchMessage>(this, "SendWatchMessage", (obj) => {
                SendWatchMessage(obj);
            });
        }
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            //ContextCompat.CheckSelfPermission(this, "android.permission.POST_NOTIFICATIONS");
            Android.Views.View decorView = Window.DecorView;
            if ((int)Android.OS.Build.VERSION.SdkInt >= 19 && hasFocus)// Check build version for Nav bar
            {
                var uiOptions = (int)decorView.SystemUiVisibility;
                var newUiOptions = (int)uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                newUiOptions |= (int)SystemUiFlags.Immersive;
                newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
               
            }
        }
        //To override system font size
        //public override void OnConfigurationChanged(Configuration newConfig)
        //{
        //    base.OnConfigurationChanged(newConfig);
        //    Configuration configuration = new Configuration(newConfig);
        //    adjustFontScale(Forms.Context, configuration);
        //}
        //public  void adjustFontScale(Context context, Configuration configuration)
        //{
        //    if (configuration.FontScale != 1)
        //    {
        //        configuration.FontScale = 1;
        //        DisplayMetrics metrics = Resources.DisplayMetrics;
        //        //var wm = context.GetSystemService(Context.WindowService);
        //        //wm.GetDefaultDisplay().getMetrics(metrics);
        //        metrics.ScaledDensity = configuration.FontScale * metrics.Density;
        //        Resources.UpdateConfiguration(configuration, metrics);
        //    }
        //}
        private async void GetNotificationPermission()
        {
            //CrossFirebasePushNotification.Current.
            //var readWritePermission = DependencyService.Get<IReadWritePermission>();
            //var status = await readWritePermission.CheckStatusAsync();
            //if (status != PermissionStatus.Granted)
            //{
            //    status = await readWritePermission.RequestAsync();
            //}
        }

        #region Error handling
        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // iOS: Environment.SpecialFolder.Resources
                var errorFilePath = Path.Combine(libraryPath, errorFileName);
                var errorMessage = String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                DateTime.Now, exception.ToString());
                File.WriteAllText(errorFilePath, errorMessage);

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
                var properties = new Dictionary<string, string>
                {
                    { "FinishExercise", $"{exception.StackTrace}" },
                    { "ErrorMessage", errorMessage}

                };
                Crashes.TrackError(exception, properties);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }

        /// <summary>
        // If there is an unhandled exception, the exception information is diplayed 
        // on screen the next time the app is started (only in debug configuration)
        /// </summary>
        [Conditional("DEBUG")]
        private void DisplayCrashReport()
        {
            const string errorFilename = "Fatal.log";
            var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var errorFilePath = Path.Combine(libraryPath, errorFilename);

            if (!File.Exists(errorFilePath))
            {
                return;
            }

            var errorText = File.ReadAllText(errorFilePath);
            new AlertDialog.Builder(this)
                .SetPositiveButton("Clear", (sender, args) =>
                {
                    File.Delete(errorFilePath);
                })
                .SetNegativeButton("Close", (sender, args) =>
                {
                    // User pressed Close.
                })
                .SetMessage(errorText)
                .SetTitle("Crash Report")
                .Show();
        }
#endregion


        private double GetStatusBarHeight()
        {
            double statusBarHeight = -1;
            int resourceId = this.Resources.GetIdentifier("status_bar_height", "dimen", "android");

            if (resourceId > 0)
            {
                statusBarHeight = this.Resources.GetDimensionPixelSize(resourceId) / Resources.DisplayMetrics.Density;
            }

            return statusBarHeight;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {

            //if (PurchaseManager.Instance != null && PurchaseManager.Instance._serviceConnection.Connected)
            //    PurchaseManager.Instance._serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);


                base.OnActivityResult(requestCode, resultCode, data);
                //Plugin.InAppBilling.InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);

                GoogleClientManager.OnAuthCompleted(requestCode, resultCode, data);
            if (CallbackManager != null)
                CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            var manager = DependencyService.Get<IFacebookManager>();
            if (manager != null)
            {
                (manager as FacebookManager_Droid).CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            }
            
            }
            catch (Exception ex)
            {

            }
        }

        protected async override void OnResume()
        {
            base.OnResume();
            try
            {
                var ticks = LocalDBManager.Instance.GetDBSetting("LastBackgroundTimee")?.Value;
                //
                var timerTime = LocalDBManager.Instance.GetDBSetting("LastBackgroundTimerTime")?.Value;
                timerTime = timerTime == null ? "0" : timerTime;

                var date = new DateTime(long.Parse(ticks == null ? "0" : ticks));

                var seconds = (DateTime.Now - date).TotalSeconds;
                var remaininTime = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value);
                MessagingCenter.Send<EnterForegroundMessage>(new EnterForegroundMessage(), "EnterForegroundMessage");
                if (seconds > double.Parse(timerTime))
                    Timer.Instance.Remaining = 0;
                else
                    Timer.Instance.Remaining = (int)(double.Parse(timerTime) - seconds);
                var remaining = Timer.Instance.Remaining;
                if (remaining > 0)
                {
                    Timer.Instance.stopRequest = true;
                    var val = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", remaining.ToString());
                    await Timer.Instance.StartTimer();
                    Timer.Instance.Remaining = remaining;
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", val);
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (Timer.Instance.Remaining > 0 && Timer.Instance.State == "RUNNING")
            {
                LocalDBManager.Instance.SetDBSetting("LastBackgroundTimee", DateTime.Now.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("LastBackgroundTimerTime", Timer.Instance.Remaining.ToString());
                Timer.Instance.StopAllTimer();
                Timer.Instance.TimerDone();
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            //PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);

            string pageKey = intent.GetStringExtra("PageKey");
            //FirebasePushNotificationManager.ProcessIntent(this, intent);
            System.Diagnostics.Debug.WriteLine("#######Notification Tapped :");
            //HandleNotificationTapped(intent);
            if (!string.IsNullOrEmpty(pageKey))
            {
                if (pageKey.ToLower().Contains("chat"))
                {
                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage(""), "NavigationOnNotificationTappedMessage");
                }
                else if (pageKey.ToLower().Contains("local5th"))
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Local"), "NavigationOnNotificationTappedMessage");
                }
                else if (pageKey.ToLower().Contains("workout"))
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Workout", intent.GetStringExtra("Extra")), "NavigationOnNotificationTappedMessage");
                }
            }
        }

       
        private async void HandleNotificationTapped(Android.Content.Intent intent)
        {
            //int NotificationId = intent.GetIntExtra("NotificationId", 0);
            //string pageKey = intent.GetStringExtra("PageKey");
            //System.Diagnostics.Debug.WriteLine($"Main Activity NotificationId : {NotificationId}, PageKey : {pageKey}");
            //JsonParser.Parse()
            //JsonElement payload = new JsonParser().parse(remoteMessage.getData().get("sendbird"));
            //sendNotification(message, payload);
            //var extras = intent.Extras;
            //foreach (var key in extras.KeySet())
            //{
            //    System.Diagnostics.Debug.WriteLine($"#######Notification Tapped Data: key = {key}, val = {extras.GetString(key)}");
            //}

            //if (!string.IsNullOrEmpty(pageKey))
            //{
            //    //AppPages pageKeyToBeNavigate;
            //    //Enum.TryParse(pageKey, out pageKeyToBeNavigate);
            //    //System.Diagnostics.Debug.WriteLine($"PageKeyToBeNavigate : {pageKeyToBeNavigate}");
            string pageKey = intent.GetStringExtra("PageKey");
            if (!string.IsNullOrEmpty(pageKey))
            {
                if (pageKey.ToLower().Contains("chat"))
                {
                    await Task.Delay(16000);
                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage(""), "NavigationOnNotificationTappedMessage");
                }
                else if(pageKey.ToLower().Contains("local5th"))
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    await Task.Delay(10000);
                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Local"), "NavigationOnNotificationTappedMessage");
                }
                else if (pageKey.ToLower().Contains("workout"))
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    CurrentLog.Instance.IsWelcomePopup = true;
                    CurrentLog.Instance.IsWelcomeMessage = true;
                    await Task.Delay(10000); Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Workout", intent.GetStringExtra("Extra")), "NavigationOnNotificationTappedMessage");
                }
            }
            //}
        }
        public void ProcessMessage(Intent intent)
        {
            //_txtMsg.Text =
            try
            {

            
            var message = intent.GetStringExtra("WearMessage");
            var phoneModel = JsonConvert.DeserializeObject<PhoneToWatchModel>(message);
            if (phoneModel != null && phoneModel.SenderPlatform == DrMuscleWebApiSharedModel.Platform.Watch)
            {
                    App.IsConnectedToWatch = true;
                    MessagingCenter.Send<ReceivedWatchMessage>(new ReceivedWatchMessage() { PhoneToWatchModel = phoneModel }, "ReceivedWatchMessage");
            }

            }
            catch (Exception ex)
            {

            }
        }

        internal class MessageReciever : Android.Content.BroadcastReceiver
        {
            MainActivity _main;
            public MessageReciever(MainActivity owner) { this._main = owner; }
            public override void OnReceive(Context context, Intent intent)
            {
                _main.ProcessMessage(intent);
               // Toast.MakeText(MainActivity._currentActivity, "Meesage received", ToastLength.Long).Show();
            }
        }

        public void SendWatchMessage(SendWatchMessage emoji)
        {
            try
            {

            //if (_client.IsConnected)
            //    App.IsConnectedToWatch = true;

            var m = new PhoneToWatchModel() { WatchMessageType = emoji.WatchMessageType, Seconds = emoji.Seconds };
            if (emoji.SetModel != null)
            {
                m.Id = emoji.SetModel.Id; m.Reps = emoji.SetModel.Reps; m.Weight = emoji.SetModel.WeightDouble;
                m.Label = emoji.Label;
            }
            m.SenderPlatform = DrMuscleWebApiSharedModel.Platform.Phone;
            //var request = PutDataMapRequest.Create("/DrMuscleWear/Data");
            //var map = request.DataMap;
            //map.PutString("Message", JsonConvert.SerializeObject(m));
            //map.PutLong("UpdatedAt", DateTime.UtcNow.Ticks);
            //if (_client != null)
            //    WearableClass.DataApi.PutDataItem(_client, request.AsPutDataRequest());

            }
            catch (Exception ex)
            {

            }
        }
    }



    //Resize issue
    public class AndroidBug5497WorkaroundForXamarinAndroid
    {

        // For more information, see https://code.google.com/p/android/issues/detail?id=5497
        // To use this class, simply invoke assistActivity() on an Activity that already has its content view set.

        // CREDIT TO Joseph Johnson (http://stackoverflow.com/users/341631/joseph-johnson) for publishing the original Android solution on stackoverflow.com

        public static void assistActivity(Activity activity)
        {
            new AndroidBug5497WorkaroundForXamarinAndroid(activity);
        }

        private Android.Views.View mChildOfContent;
        private int usableHeightPrevious;
        private FrameLayout.LayoutParams frameLayoutParams;

        private AndroidBug5497WorkaroundForXamarinAndroid(Activity activity)
        {
            try
            {
                FrameLayout content = (FrameLayout)activity.FindViewById(Android.Resource.Id.Content);
                mChildOfContent = content.GetChildAt(0);
                ViewTreeObserver vto = mChildOfContent.ViewTreeObserver;
                vto.GlobalLayout += (object sender, EventArgs e) => {
                    possiblyResizeChildOfContent();
                };
                frameLayoutParams = (FrameLayout.LayoutParams)mChildOfContent.LayoutParameters;

            }
            catch (Exception ex)
            {

            }
        }

        private void possiblyResizeChildOfContent()
        {
            int usableHeightNow = computeUsableHeight();
            if (App.IsResizeScreen)
            { 
            if (usableHeightNow != usableHeightPrevious)
            {
                int usableHeightSansKeyboard = mChildOfContent.RootView.Height;
                int heightDifference = usableHeightSansKeyboard - usableHeightNow;

                frameLayoutParams.Height = usableHeightSansKeyboard - heightDifference;

                mChildOfContent.RequestLayout();
                usableHeightPrevious = usableHeightNow;
            }
            }
        }

        private int computeUsableHeight()
        {
            Android.Graphics.Rect r = new Android.Graphics.Rect();
            mChildOfContent.GetWindowVisibleDisplayFrame(r);
            return (r.Bottom - r.Top);
        }
    }    
}

