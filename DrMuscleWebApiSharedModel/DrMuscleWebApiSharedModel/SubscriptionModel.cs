using System;
namespace DrMuscleWebApiSharedModel
{
    public class SubscriptionModel : BaseModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PurchaseToken { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.DateTime> MealPlanExpiryDate { get; set; }
        public string ProductId { get; set; }
        public string OrderId { get; set; }
        public int Platform { get; set; }
        


    }
}
