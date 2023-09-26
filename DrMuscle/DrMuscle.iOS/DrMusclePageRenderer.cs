using System;
using Xamarin.Forms;
using DrMuscle.Layout;
using DrMuscle.iOS.Renderers;
using UIKit;
using SlideOverKit.iOS;
using DrMuscle.Screens.User;
using DrMuscle.iOS.Service;
using System.Linq;
using DrMuscle.Screens.Workouts;
using DrMuscleWatch;
using WatchConnectivity;
using System.Collections.Generic;
using DrMuscle.Message;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;
using Foundation;

[assembly: ExportRenderer(typeof(DrMusclePage), typeof(DrMusclePageRenderer))]
namespace DrMuscle.iOS.Renderers
{
    public class DrMusclePageRenderer : MenuContainerPageiOSRenderer
    {
        UIImageView ImageView;
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            ImageView = new UIImageView(new CoreGraphics.CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height));

            ImageView.ContentMode = UIViewContentMode.ScaleToFill;

            this.View.AddSubview(ImageView);
            this.View.SendSubviewToBack(ImageView);
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                //if(this.NavigationController != null)
                //{
                //    this.NavigationController.NavigationBar.BarTintColor = UIColor.FromPatternImage(UIImage.FromBundle("nav.png"));
                //}
            }
            this.AutomaticallyAdjustsScrollViewInsets = false;

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            BackgroundImageChanged(null);

           // MessagingCenter.Subscribe<SettingsPage>(this, "BackgroundImageUpdated", BackgroundImageChanged);
           //
            WCSessionManager.SharedManager.ApplicationContextUpdated += DidReceiveApplicationContext;
            
            MessagingCenter.Subscribe<SendWatchMessage>(this, "SendWatchMessage", (obj) => {
                SendMessage(obj);
            });
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.Portrait)), new NSString("orientation"));

            //foreach (var item in UIFont.FamilyNames)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Fonts:{item}");
            //    foreach (var item2 in UIFont.FontNamesForFamilyName(item))
            //    {
            //        System.Diagnostics.Debug.WriteLine($"/t{item2}");
            //    }
            //}
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }
        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            this.EdgesForExtendedLayout = UIRectEdge.None;
            this.ExtendedLayoutIncludesOpaqueBars = false;
            try
            {
                var txtFont = new UITextAttributes()
                {
                    Font = UIFont.SystemFontOfSize(18)
                };
                foreach (var item in this.TabBarController?.ViewControllers)
                {
                    item.TabBarItem.SetTitleTextAttributes(txtFont, UIControlState.Normal); 
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            WCSessionManager.SharedManager.ApplicationContextUpdated -= DidReceiveApplicationContext;
            //MessagingCenter.Unsubscribe<SettingsView>(this, "BackgroundImageUpdated");
            if (App.Current.MainPage.Navigation.NavigationStack.Last() is ExerciseVideoPage)
                UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.LandscapeLeft)), new NSString("orientation"));
            try
            {
                this.TabBarController.TabBar.Translucent = true;
            }
            catch (Exception ex)
            {

            }

            MessagingCenter.Unsubscribe<SendWatchMessage>(this, "SendWatchMessage");
        }



        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            
        }

        public override bool ShouldAutorotate()
        {
            return false;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Portrait;
        }
        //public override void ViewDidDisappear(bool animated)
        //{
        //    base.ViewDidDisappear(animated);
        //    if (App.Current.MainPage.Navigation.NavigationStack.Last() is ExerciseVideoPage)
        //        new OrientationService().Landscape();
        //    else
        //        new OrientationService().Portrait();
        //}

        void BackgroundImageChanged(SettingsPage obj)
        {
            ImageView.Image = null;
            return;
            if (LocalDBManager.Instance.GetDBSetting("BackgroundImage") != null)
            {
                ImageView.ContentMode = LocalDBManager.Instance.GetDBSetting("BackgroundImage").Value == "Background2.png" ? UIViewContentMode.ScaleAspectFill : UIViewContentMode.ScaleAspectFit;
                ImageView.Image = UIImage.FromFile(LocalDBManager.Instance.GetDBSetting("BackgroundImage").Value);
            }
            else
                ImageView.Image = null;
        }

        //Handle watch app
        public void DidReceiveApplicationContext(WCSession session, Dictionary<string, object> applicationContext)
        {
            App.IsConnectedToWatch = true;
            var message = (string)applicationContext["MessagePhone"];
            var phoneModel = JsonConvert.DeserializeObject<PhoneToWatchModel>(message);
            if (phoneModel != null)
            {
                MessagingCenter.Send<ReceivedWatchMessage>(new ReceivedWatchMessage() { PhoneToWatchModel = phoneModel  }, "ReceivedWatchMessage");
            }
        }

        public void SendMessage(SendWatchMessage emoji)
        {
            
            var m = new PhoneToWatchModel() {WatchMessageType = emoji.WatchMessageType, Seconds = emoji.Seconds };
            if (emoji.SetModel != null)
            {
                m.Id = emoji.SetModel.Id; m.Reps = emoji.SetModel.Reps; m.Weight = emoji.SetModel.WeightDouble;
                m.Label = emoji.Label;
            }
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(m)}" } });
        }
    }
}
