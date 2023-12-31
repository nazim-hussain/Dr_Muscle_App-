﻿using System.Threading.Tasks;
using Firebase.RemoteConfig;
using DrMuscle.Services;
using DrMuscle.iOS.Services;

[assembly: Xamarin.Forms.Dependency(typeof(MyRemoteConfigurationService))]
namespace DrMuscle.iOS.Services
{
    public class MyRemoteConfigurationService : IRemoteConfigurationService
    {
        public MyRemoteConfigurationService()
        {
            RemoteConfig.SharedInstance.SetDefaults("RemoteConfigDefaults");
            RemoteConfig.SharedInstance.ConfigSettings = new RemoteConfigSettings();
        }

        public async Task FetchAndActivateAsync()
        {
            var status = await RemoteConfig.SharedInstance.FetchAsync(0);
            if (status == RemoteConfigFetchStatus.Success)
            {
                RemoteConfig.SharedInstance.ActivateAsync();
            }
        }

        public async Task<FeatureConfiguration> GetAsync<FeatureConfiguration>(string key)
        {

            var settings = RemoteConfig.SharedInstance[key].StringValue;
            return await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<FeatureConfiguration>(settings));
        }
    }
}