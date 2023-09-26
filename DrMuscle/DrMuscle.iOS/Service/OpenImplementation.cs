using System;
using DrMuscle.Dependencies;
using DrMuscle.iOS.Services;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(OpenImplementation))]

namespace DrMuscle.iOS.Services
{
    public class OpenImplementation : IOpenManager
    {
        public void openMail()
        {
            try
            {

            NSUrl mailUrl = new NSUrl("message://");
            if (UIApplication.SharedApplication.CanOpenUrl(mailUrl))
            {
                UIApplication.SharedApplication.OpenUrl(mailUrl);
            }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
