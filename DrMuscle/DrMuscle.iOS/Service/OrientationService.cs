using System;
using DrMuscle.Dependencies;
using DrMuscle.iOS.Service;
using Foundation;
using UIKit;
[assembly: Xamarin.Forms.Dependency(typeof(OrientationService))]

namespace DrMuscle.iOS.Service
{
   
    public class OrientationService : IOrientationService
    {
        public void Landscape()
        {
            AppDelegate appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            appDelegate.allowRotation = true;

            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.LandscapeLeft), new NSString("orientation"));
            //((AppDelegate)UIApplication.SharedApplication.Delegate).CurrentOrientation = UIInterfaceOrientationMask.Landscape;
            //UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.LandscapeLeft, false);
        }

        public void Portrait()
        {
            AppDelegate appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            appDelegate.allowRotation = false;

            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));
            //((AppDelegate)UIApplication.SharedApplication.Delegate).CurrentOrientation = UIInterfaceOrientationMask.Portrait;
            //UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.Portrait, false);
        }
    }
}
