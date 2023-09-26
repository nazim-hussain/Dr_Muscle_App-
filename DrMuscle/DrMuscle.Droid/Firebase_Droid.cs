using System;
using Xamarin.Forms;
using DrMuscle.Droid;
using Firebase.Analytics;
using Android.OS;

[assembly: Dependency(typeof(Firebase_Droid))]
namespace DrMuscle.Droid
{
    public class Firebase_Droid : IFirebase
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