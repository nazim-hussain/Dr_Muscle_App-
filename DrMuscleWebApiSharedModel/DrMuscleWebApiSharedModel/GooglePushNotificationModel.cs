using System;
namespace DrMuscleWebApiSharedModel
{
    public class GooglePushNotificationModel
    {
        public GooglePushNotificationModel()
        {
        }
        public string version { get; set; }
        public string packageName { get; set; }
        public string eventTimeMillis { get; set; }
        public SubscriptionNotification subscriptionNotification { get; set; }
    }

    public class SubscriptionNotification
    {
        public string version { get; set; }
        public int notificationType { get; set; }
        public string purchaseToken { get; set; }
        public string subscriptionId { get; set; }
    }
}
