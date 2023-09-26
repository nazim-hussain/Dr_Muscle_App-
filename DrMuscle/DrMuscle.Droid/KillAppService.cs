using System;
using DrMuscle.Droid.Services;
using DrMuscle.Dependencies;
using Xamarin.Forms;

[assembly:Dependency(typeof(KillAppService))]
namespace DrMuscle.Droid.Services
{
    public class KillAppService : IKillAppService
    {
        public void ExitApp()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}
