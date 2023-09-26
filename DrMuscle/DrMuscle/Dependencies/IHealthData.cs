using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrMuscle.Dependencies
{
    public interface IHealthData 
    {
        void FetchSteps(DateTime startDate, DateTime endDate, Action<double> completionHandler);
        void FetchStairsSteps(DateTime startDate, DateTime endDate, Action<double> completionHandler);
        void FetchMetersWalked(Action<double> completionHandler);
        void FetchActiveMinutes(Action<double> completionHandler);
        Task GetHealthPermissionAsync(Action<bool> completion);
        Task GetWeightPermissionAsync(Action<bool> completion);
        void FetchActiveEnergyBurned(Action<double> completionHandler);
        void SaveActiveMinutes(double minutes, int newrecord, int exercise, string volume, string totalStrength, string maxStrength, double calories, List<string> exercisesName, List<string> exercisesStrenggth);
        void FetchWeight(Action<double> completionHandler);
        void SetWeight(double bodyweight);
    }
}
