using System;
using System.Threading.Tasks;
using DrMuscle.Models;

namespace DrMuscle.Services
{
    public interface IAppleSignInService
    {
        bool IsAvailable { get; }
        Task<AppleSignInCredentialState> GetCredentialStateAsync(string userId);
        Task<AppleAccount> SignInAsync();
    }

}
