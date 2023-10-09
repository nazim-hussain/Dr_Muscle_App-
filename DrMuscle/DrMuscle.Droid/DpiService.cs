using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DrMuscle.Dependencies;
using DrMuscle.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(DpiService))]
namespace DrMuscle.Droid
{
    public class DpiService : IDpiService
    {
        public int GetDpi()
        {
            var metrics = Resources.System.DisplayMetrics;
            return (int)(metrics.Density * 160f);
        }
    }
}