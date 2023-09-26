using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using System.Threading.Tasks;
using Android.Content.PM;
using Plugin.FirebasePushNotification;

namespace DrMuscle.Droid
{
    [Activity(Label = "Dr. Muscle", ScreenOrientation = ScreenOrientation.Portrait, Icon = "@drawable/icon", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
           // FirebasePushNotificationManager.ProcessIntent(this, Intent);
            //SetContentView(Resource.Layout.Launch);
            View decorView = Window.DecorView;
            if ((int)Android.OS.Build.VERSION.SdkInt >= 19)// Check build version for Nav bar
            {
                var uiOptions = (int)decorView.SystemUiVisibility;
				var newUiOptions = (int)uiOptions;

				newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                newUiOptions |= (int)SystemUiFlags.Immersive;
                decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            }

			this.Window.AddFlags(WindowManagerFlags.Fullscreen);
			this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            FirebasePushNotificationManager.ProcessIntent(this, intent);
        }
        protected override void OnResume()
        {
            base.OnResume();
            
            Task startupWork = new Task(() =>
            {
                //Task.Delay(2000);  // Simulate a bit of startup work.
            });

            try
            {
                startupWork.ContinueWith(t =>
                {
                    var mainIntent = new Intent(Application.Context, typeof(MainActivity));
                    //if (Intent.Extras != null)
                    //{
                    //    mainIntent.PutExtras(Intent.Extras);
                    //}
                    StartActivity(mainIntent);
                    FirebasePushNotificationManager.ProcessIntent(this, this.Intent);
                }, TaskScheduler.FromCurrentSynchronizationContext());

                startupWork.Start();
            }
            catch (Exception ex)
            {

            }

            
        }
    }
}