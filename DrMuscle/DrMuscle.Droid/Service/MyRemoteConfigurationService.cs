using System.Threading.Tasks;
using Firebase.RemoteConfig;
using DrMuscle.Services;
using DrMuscle.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(MyRemoteConfigurationService))]
namespace DrMuscle.Droid.Services
{
    public class MyRemoteConfigurationService : IRemoteConfigurationService
    {
        public MyRemoteConfigurationService()
        {
            FirebaseRemoteConfigSettings configSettings = new FirebaseRemoteConfigSettings.Builder()
                //.SetDeveloperModeEnabled(true)
                .Build();
            
            FirebaseRemoteConfig.Instance.SetConfigSettingsAsync(configSettings);
            
                 FirebaseRemoteConfig.Instance.SetDefaultsAsync(Resource.Xml.RemoteConfigDefaults);
        }

        public async Task FetchAndActivateAsync()
        {
            await FirebaseRemoteConfig.Instance.FetchAsync(0);
            FirebaseRemoteConfig.Instance.FetchAndActivate();
        }

        public async Task<TInput> GetAsync<TInput>(string key)
        {
            var settings = FirebaseRemoteConfig.Instance.GetString(key);
            return await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<TInput>(settings));
        }
    }
}