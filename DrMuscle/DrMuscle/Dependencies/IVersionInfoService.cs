using System;
namespace DrMuscle.Dependencies
{
    public interface IVersionInfoService
    {
        int GetVersionInfo();
        string GetDeviceUniqueId();
       
    }
}
