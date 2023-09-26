using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.iOS;
using Foundation;
using DrMuscle.Dependencies;

[assembly: Xamarin.Forms.Dependency(typeof(DrMuscleSubscription_iOS))]
namespace DrMuscle.iOS
{
    public class DrMuscleSubscription_iOS : IDrMuscleSubcription
    {
        public DrMuscleSubscription_iOS()
        {
            //PurchaseManager.Instance.OnLifetimeAccessPurchased += Instance_OnLifetimeAccessPurchased;
         
        }

        public event MonthlyAccessPurchased OnMonthlyAccessPurchased;
        public event YearlyAccessPurchased OnYearlyAccessPurchased;
        public event MealPlanAccessPurchased OnMealPlanAccessPurchased;

        void Instance_OnMonthlyAccessPurchased()
        {
            
        }

        void Instance_OnYearlyAccessPurchased()
        {
            
        }

        void Instance_OnMealPlanAccessPurchased()
        {
            
        }

        public bool IsActiveSubscriptions()
        {
            return true;
        }

        public bool IsActiveMealPlan()
        {
            return true;
        }

        public void Init()
        {
        }

        public void RestorePurchases()
        {
            
        }

        public string GetBuildVersion()
        {
            return "1.0";
        }

        public bool IsMonthlyAccessPuchased()
        {
            return true;
        }

        public bool IsYearlyAccessPuchased()
        {
            return true;
        }

        public bool IsMealPlanAccessPuchased()
        {
            return true;
        }

        public async Task<string> GetMonthlyPrice()
        {
           
            return "";
        }

        public async Task<string> GetMonthlyButtonLabel()
        {
            return "";
        }

        public async Task<string> GetMealPlanLabel()
        {
            return "";
        }
        public async Task<string> GetYearlyPrice()
        {
            return "";
        }

        public async Task<string> GetYearlyButtonLabel()
        {
            return "";
        }


        public async Task BuyMonthlyAccess()
        {
        }

        public async Task BuyYearlyAccess()
        {
        }

        public async Task BuyMealPlanAccess()
        {
        }
    }
}
