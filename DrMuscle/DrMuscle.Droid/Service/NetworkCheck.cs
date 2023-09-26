using System.Threading.Tasks;
using Android.OS;
using DrMuscle.Dependencies;
using Java.Lang;
using Java.Net;
using Xamarin.Facebook.AppEvents.ML;
using Xamarin.Forms;
using Exception = System.Exception;
using Object = Java.Lang.Object;

[assembly: Dependency(typeof(DrMuscle.Droid.Service.NetworkCheck))]
namespace DrMuscle.Droid.Service
{
    public class NetworkCheck : INetworkCheck
    {
        public async Task<bool> IsNetworkAvailable()
        {
           
            try
            {
                // var task = new ConnectionCheckAsyncTask();
                // task.Execute();
                // return (bool)await task.GetAsync();  
                var p = Java.Lang.Runtime.GetRuntime().Exec("ping -c 1 8.8.8.8");
                int exitCode = await p.WaitForAsync();
                return (exitCode == 0);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

       
    }
    public class ConnectionCheckAsyncTask : AsyncTask
    {
        private bool isInternetReachable()
        {
            try
            {
                var address = InetAddress.GetByName("google.com");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        protected override Object DoInBackground(params Object[] @params)
        {
            var res = isInternetReachable();
            return res;
        }
    }
}