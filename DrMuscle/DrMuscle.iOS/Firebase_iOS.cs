using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DrMuscle.Dependencies;
using DrMuscle.Entity;
using DrMuscle.iOS;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Facebook.ShareKit;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Firebase.Analytics;

[assembly: Dependency(typeof(Firebase_iOS))]
namespace DrMuscle.iOS
{
	public class Firebase_iOS : IFirebase
	{
		public void LogEvent(string key, string val)
		{
			
		}

        public void SetScreenName(string val)
        {
            

		}

        public void SetUserId(string name)
        {
			
        }

	}
}
