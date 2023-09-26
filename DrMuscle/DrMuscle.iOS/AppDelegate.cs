using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using SegmentedControl.FormsPlugin.iOS;
using MonoTouch.Dialog;
using Facebook.CoreKit;
using Firebase.Analytics;
using Xamarin.Forms;
using DrMuscle.Message;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using Plugin.Vibrate;
using Microsoft.AppCenter;
using XFShapeView.iOS;
using System.Threading.Tasks;
using Plugin.FirebasePushNotification;
using Rg.Plugins.Popup.Services;
using MediaPlayer;
using UserNotifications;
using Plugin.GoogleClient;
using Microsoft.AppCenter.Crashes;
using Firebase.Crashlytics;
using ObjCRuntime;
using BranchXamarinSDK;
using DrMuscleWatch;
using FFImageLoading.Transformations;
using Sentry;

namespace DrMuscle.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IBranchBUOSessionInterface
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        bool _isIOS13, _isMute = false;
        AVAudioPlayer _player, player;
        AVAudioPlayer _backgroundplayer;
        NSTimer timer;
        public bool allowRotation;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            GoogleClientManager.Initialize();
            FormsMaterial.Init();
            OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
            Facebook.CoreKit.Settings.AppId = "1865252523754972";
            Facebook.CoreKit.Settings.DisplayName = "Dr. Muscle";
            
            
            
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(4000);
                
                SegmentedControlRenderer.Init();
            });
            SlideOverKit.iOS.SlideOverKit.Init();
            
            
            //Crashlytics.Configure();
            //Fabric.Fabric.SharedSdk.Debug = false;
            App.StatusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
            ShapeRenderer.Init();
            Rg.Plugins.Popup.Popup.Init();
            Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            // set Debug mode
            var ignore = new CornersTransformation();

            BranchIOS.Debug = true;
            BranchIOS.DelayInitToCheckForSearchAds();
            BranchIOS.Init("key_live_neM4shDTT8ro8C0fJFXbidkaCwlAYzm7", options, this);
            App.ScreenWidth = UIScreen.MainScreen.Bounds.Size.Width;
            App.ScreenHeight = UIScreen.MainScreen.Bounds.Size.Height;
            LoadApplication(new App());
            UIApplication.SharedApplication.IdleTimerDisabled = true;
            _isIOS13 = UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
            
            
            MessagingCenter.Subscribe<StartTimerMessage>(this, "StartTimerMessage", async message =>
            {
                nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("TimerStartTask", OnExpiration);
                Timer.Instance.PCLStartTimer();
                while (Timer.Instance.State == "RUNNING")
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
                }
                //if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                    UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
            MessagingCenter.Subscribe<StopTimerMessage>(this, "StopTimerMessageOff", async message =>
            {
                    nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("TimerStopTask", OnExpiration);
                    await Timer.Instance.PCLStopTimer();
                    while (Timer.Instance.State == "RUNNING")
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    //if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                    UIApplication.SharedApplication.EndBackgroundTask(taskId);   
            });

            MessagingCenter.Subscribe<StopTimerMessage>(this, "StopTimerMessage", async message =>
            {
                    nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("TimerStopTask", OnExpiration);
                    await Timer.Instance.PCLStopTimer();
                    while (Timer.Instance.State == "RUNNING")
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    UIApplication.SharedApplication.EndBackgroundTask(taskId);
                
            });

            MessagingCenter.Subscribe<PlayAudioFileMessage>(this, "PlayAudioFileMessage", async message =>
            {
                //nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("PlaySoundTask", OnExpiration);

                NSError error;
                AVAudioSession instance = AVAudioSession.SharedInstance();
                instance.SetCategory(new NSString("AVAudioSessionCategoryPlayback"), AVAudioSessionCategoryOptions.MixWithOthers, out error);

                instance.SetMode(new NSString("AVAudioSessionModeDefault"), out error);
                                instance.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);
                if ( message.IsEmptyAudio)
                {
                    var soundFile = "emptyAudio.mp3";

                    string sFilePath = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(soundFile), Path.GetExtension(soundFile));
                    var url = NSUrl.FromString(sFilePath);
                    _player = AVAudioPlayer.FromUrl(url);
                    _player.FinishedPlaying += (object sender, AVStatusEventArgs e) =>
                    {
                        _player = null;
                    };
                    //if (!_isMute)
                    _player.Play();
                    return;
                }

                if (message.Is321)
                {
                    var soundFile = "timer123.mp3";

                    string sFilePath = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(soundFile), Path.GetExtension(soundFile));
                    var url = NSUrl.FromString(sFilePath);
                    player = AVAudioPlayer.FromUrl(url);
                    player.FinishedPlaying += (object sender, AVStatusEventArgs e) =>
                    {
                        player = null;
                    };
                    //if (!_isMute)
                    player.Play();
                    return;
                }

                if (LocalDBManager.Instance.GetDBSetting("timer_sound").Value == "true" || LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value == "true")
                {
                    var soundFile = "alarma.mp3";
                    if (Timer.Instance.NextRepsCount <= 0 || Timer.Instance.NextRepsCount>60)
                    {

                    }
                    else if(LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value == "true")
                    {
                        soundFile = $"reps{Timer.Instance.NextRepsCount}.mp3";
                    }

                    string sFilePath = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(soundFile), Path.GetExtension(soundFile));
                    var url = NSUrl.FromString(sFilePath);
                    _player = AVAudioPlayer.FromUrl(url);
                    _player.FinishedPlaying += (object sender, AVStatusEventArgs e) =>
                    {
                        _player = null;
                    };
                    //if (!_isMute)
                    _player.Play();
                }

                

                if (LocalDBManager.Instance.GetDBSetting("timer_vibrate").Value == "true")
                    CrossVibrate.Current.Vibration(TimeSpan.FromSeconds(5));

                while (Timer.Instance.State == "RUNNING")
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                }
                //if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                //    UIApplication.SharedApplication.EndBackgroundTask(taskId);

                
            });
            if (options != null && options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
            {
                HandleNoticationTapped();
            }
            if (options != null && options.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
            {
                var localNotification = options[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
                if (localNotification != null)
                {
                    if (localNotification.UserInfo != null && localNotification.UserInfo.ContainsKey(new NSString($"DrMuscleNotification1151")))
                    {
                        CurrentLog.Instance.IsRecoveredWorkout = true;
                        HandleLocalNoticationTapped();
                    }

                    if (localNotification.UserInfo != null && localNotification.UserInfo.ContainsKey(new NSString($"DrMuscleNotification1352")))
                    {
                        var workoutId = localNotification.UserInfo["Extra"];
                        CurrentLog.Instance.IsRecoveredWorkout = true;
                        HandleWorkoutLocalNoticationTapped(Convert.ToString(workoutId));
                    }
                }
                
                
            }

UINavigationBar.Appearance.SetBackgroundImage(UIImage.FromFile("topNav.png"), UIBarMetrics.Default);
            UINavigationBar.Appearance.SetBackgroundImage(UIImage.FromFile("topNav.png"), UIBarMetrics.Compact);
            UINavigationBar.Appearance.Translucent = false;


            //AppDomain.CurrentDomain.UnhandledException += async (sender, e) => {

            //};
            //TaskScheduler.UnobservedTaskException += async (sender, e) => { };
            WCSessionManager.SharedManager.StartSession();
           // BackgroundWorkTimer();

            return base.FinishedLaunching(app, options);
        }


        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
        {
            if (allowRotation == true)
            {
                return UIInterfaceOrientationMask.Landscape;
            }

            else
            {
                return UIInterfaceOrientationMask.Portrait;
            }
        }
        [Export("applicationDidEnterBackground:")]
        public void DidEnterBackground(UIApplication application)
        {

            nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("BackgroundWorkTimerInBackground", OnExpiration);
            if (_isIOS13 && Timer.Instance.Remaining > 0 && Timer.Instance.State=="RUNNING")
            {
                LocalDBManager.Instance.SetDBSetting("LastBackgroundTimee", DateTime.Now.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("LastBackgroundTimerTime", Timer.Instance.Remaining.ToString());
                RegisterNotification(Timer.Instance.Remaining);
                Timer.Instance.StopAllTimer();
                Timer.Instance.TimerDone();
            }


            UIApplication.SharedApplication.EndBackgroundTask(taskId);
        }

        [Export("applicationWillEnterForeground:")]
        public async void WillEnterForeground(UIApplication application)
        {
            try
            {

            if (_isIOS13)
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

                var notifications =  await UNUserNotificationCenter.Current.GetPendingNotificationRequestsAsync();
                    
                    if (notifications.Count() > 0)
                {
                        if (LocalDBManager.Instance.GetDBSetting("email") == null)
                            return;
                        var ticks = LocalDBManager.Instance.GetDBSetting("LastBackgroundTimee")?.Value;
                        //
                        var timerTime = LocalDBManager.Instance.GetDBSetting("LastBackgroundTimerTime")?.Value;
                        timerTime = timerTime == null ? "0" : timerTime;

                        var date = new DateTime(long.Parse(ticks == null ? "0" : ticks));
                        
                        var seconds  = (DateTime.Now - date).TotalSeconds;
                        var remaininTime = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value);
                        MessagingCenter.Send<EnterForegroundMessage>(new EnterForegroundMessage(), "EnterForegroundMessage");
                        if (seconds > double.Parse(timerTime))
                            Timer.Instance.Remaining = 0;
                        else
                            Timer.Instance.Remaining = (int)(double.Parse(timerTime) - seconds);
                        var remaining = Timer.Instance.Remaining;
                        _isMute = true;
                        if (remaining > 0)
                    {

                            _isMute = false;
                            Timer.Instance.stopRequest = true;
                        var val = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", remaining.ToString());
                        await Timer.Instance.StartTimer();
                        Timer.Instance.Remaining = remaining;
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", val);
                    }
                    else
                        {
                            await Task.Delay(2000);
                            _isMute = false;
                        }
                }
                //UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
                //UIApplication.SharedApplication.CancelAllLocalNotifications();

                }
            }
            catch (Exception ex)
            {

            }
        
        }

        void BackgroundWorkTimer()
        {
            //To stop suspend in background
            nint taskId = UIApplication.SharedApplication.BeginBackgroundTask("BackgroundWorkTimer", OnExpiration);
            timer = NSTimer.CreateRepeatingScheduledTimer(2, (obj) => { });
            //if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
        }

        public void RegisterNotification(long time)
        {
            UNUserNotificationCenter center = UNUserNotificationCenter.Current;

            //creat a UNMutableNotificationContent which contains your notification content
            UNMutableNotificationContent notificationContent = new UNMutableNotificationContent();
            
            notificationContent.Title = "Rest over";
            notificationContent.Body = "Get back to work!";
            if (Timer.Instance.NextRepsCount != 0)
            {
                notificationContent.Body = $"Get back to work, next {Timer.Instance.NextRepsCount} reps";
            }

            notificationContent.Sound = UNNotificationSound.Default;

            UNTimeIntervalNotificationTrigger trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(time, false);

            UNNotificationRequest request = UNNotificationRequest.FromIdentifier("RestTimer", notificationContent, trigger);


            center.AddNotificationRequest(request, (NSError obj) =>
            {



            });

        }

        void OnExpiration()
        {
            bool test = true;
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            AppEvents.Shared.ActivateApp();
            //if (_backgroundplayer != null)
            //    _backgroundplayer = null;
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            
        }



        [Export("application:openURL:options:")]
        public bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            System.Diagnostics.Debug.WriteLine($"options={options}");
            
            //var google = GoogleClientManager.OnOpenUrl(app, url, options);
            //if (google)
            //    return google;
            var openOptions = new UIApplicationOpenUrlOptions(options);
            var facebook = ApplicationDelegate.SharedInstance.OpenUrl(app, url, openOptions.SourceApplication, options);

            BranchIOS.getInstance().OpenUrl(url);


            return true;
        }

        [Export("application:openURL:sourceApplication:annotation:")]
        public bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
            BranchIOS.getInstance().OpenUrl(url);

            return true;
        }

        //[Export("application:openURL:sourceApplication:annotation:")]
        //public bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        //{
        //    return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        //}

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (e.ExceptionObject as Exception);
            System.Diagnostics.Debug.WriteLine($"MainDomain Unhandled exception : {exception.Message}, StackTrace : {exception.StackTrace}");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception innerException = e.Exception.InnerException;
            System.Diagnostics.Debug.WriteLine($"Unobservered Unhandled exception : {innerException?.Message}, StackTrace : {innerException?.StackTrace}");
        }

        //Notifications
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            //System.Diagnostics.Debug.WriteLine($"DeviceToken For iOS : {deviceToken}");
            //Config.RegisteredDeviceToken = deviceToken.Description.Replace("<","").Replace(">","").Replace(" ","");
           

        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            System.Diagnostics.Debug.WriteLine($"DeviceToken For Error : {error.DebugDescription}");
            
        }

        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.
            // If you disable method swizzling, you'll need to call this method. 
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            
            try
            {
                ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 1;
            }
            catch (Exception ex)
            {

            }
            System.Console.WriteLine($"Notification Tapped : {userInfo}, ApplicationState : {UIApplication.SharedApplication.ApplicationState}");
            //if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
            //HandleNoticationTapped(userInfo);
            Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage(""), "NavigationOnNotificationTappedMessage");
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            base.ReceivedRemoteNotification(application, userInfo);
            System.Diagnostics.Debug.WriteLine("Remote Notification received...");
            
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            base.ReceivedLocalNotification(application, notification);
            System.Diagnostics.Debug.WriteLine("Local Notification received...");
        }

       

        private void HandleNoticationTapped()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                //Add your code here.
                await Task.Delay(12000);
                Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage(""), "NavigationOnNotificationTappedMessage");
            }).ConfigureAwait(false);

        }

        private void HandleLocalNoticationTapped()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                //Add your code here.
                await Task.Delay(4000);
                Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Local"), "NavigationOnNotificationTappedMessage");
            }).ConfigureAwait(false);

        }

        private void HandleWorkoutLocalNoticationTapped(string workoutId)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                //Add your code here.
                await Task.Delay(4000);
                Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Workout", workoutId), "NavigationOnNotificationTappedMessage");
            }).ConfigureAwait(false);

        }
        public void InitSessionComplete(BranchUniversalObject buo, BranchLinkProperties blp)
        {

        }

        public void SessionRequestError(BranchError error)
        {
        }

        //[Export("application:configurationForConnectingSceneSession:options:")]
        //public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        //{
        //    return UISceneConfiguration.Create("Default Configuration", connectingSceneSession.Role);
        //}

        //[Export("application:didDiscardSceneSessions:")]
        //public void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        //{

        //}

    }
}
