using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using DrMuscle.Dependencies;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using DrMuscle.Droid;
//using DrMuscleWebApiSharedModel;
//using Xamarin.InAppBilling;
using Plugin.InAppBilling;
//using Plugin.InAppBilling.Abstractions;

[assembly: Dependency(typeof(DrMuscleSubscription_Droid))]
namespace DrMuscle.Droid
{
    public class DrMuscleSubscription_Droid : IDrMuscleSubcription
    {
        public DrMuscleSubscription_Droid()
        {
          
        }


        private bool _hasIntroductoryPrice = false;
      
        void Instance_OnMonthlyAccessPurchased()
        {
            
        }

        void Instance_OnYearlyAccessPurchased()
        {
           
        }

        void Instance_OnMealPlanAccessPurchased()
        {
            
        }

        public event MealPlanAccessPurchased OnMealPlanAccessPurchased;

        public async Task BuyMonthlyAccess()
        {
           
        }

        public async Task BuyYearlyAccess()
        {
            
        }

        public async Task BuyMealPlanAccess()
        {
            
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

        public event MonthlyAccessPurchased OnMonthlyAccessPurchased;
        public event YearlyAccessPurchased OnYearlyAccessPurchased;

        public async Task<string> GetYearlyPrice()
        {
            return "";
        }
        public async Task<string> GetYearlyButtonLabel()
        {
           
            return "";
        }

        public bool IsActiveSubscriptions()
        {
           
            return false;
        }

        public bool IsActiveMealPlan()
        {
           
            return false;
        }
        public bool IsMealPlanAccessPuchased()
        {
            
            return false;
        }

        public bool IsMonthlyAccessPuchased()
        {
            
            return false;
        }

        public bool IsYearlyAccessPuchased()
        {
            

            return false;
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

        private void SendDataToServer(InAppBillingPurchase purchase, string msg)
        {
            //TimeSpan time = TimeSpan.FromMilliseconds(purchase.PurchaseTime);
            
        }
    }
}
