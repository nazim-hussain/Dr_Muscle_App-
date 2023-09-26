using System.Threading.Tasks;

namespace DrMuscle.Services
{
    public interface IRemoteConfigurationService
    {
        Task FetchAndActivateAsync();
        Task<FeatureConfiguration> GetAsync<FeatureConfiguration>(string key);
    }
}
