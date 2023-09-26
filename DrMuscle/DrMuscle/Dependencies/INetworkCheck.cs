using System.Threading.Tasks;

namespace DrMuscle.Dependencies
{
    public interface INetworkCheck
    {
        Task<bool> IsNetworkAvailable();
    }
}