using System.Threading.Tasks;

namespace DrMuscle.Dependencies
{
    public delegate void LifetimeAccessPurchased();
    public delegate void MonthlyAccessPurchased();
    public delegate void YearlyAccessPurchased();
    public delegate void MealPlanAccessPurchased();

    public interface IDrMuscleSubcription
    {
        bool IsActiveSubscriptions();
        bool IsActiveMealPlan();
        bool IsMonthlyAccessPuchased();
        bool IsYearlyAccessPuchased();
        bool IsMealPlanAccessPuchased();
        //Task<string> GetMonthlyPrice();
        Task<string> GetMonthlyButtonLabel();
        Task<string> GetYearlyButtonLabel();
        Task<string> GetYearlyPrice();
        Task<string> GetMealPlanLabel();
        event MonthlyAccessPurchased OnMonthlyAccessPurchased;
        event YearlyAccessPurchased OnYearlyAccessPurchased;
        event MealPlanAccessPurchased OnMealPlanAccessPurchased;
        Task BuyMonthlyAccess();
        Task BuyYearlyAccess();
        Task BuyMealPlanAccess();
        void Init();
        void RestorePurchases();
        string GetBuildVersion();
    }
}