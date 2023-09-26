using System;
namespace DrMuscle.Message
{
    public class NavigationOnNotificationTappedMessage
    {
        public NavigationOnNotificationTappedMessage(string notificationType, string workoutId="")
        {
            NotificationType = notificationType;
            WorkoutId = workoutId;
        }
        public string NotificationType { get; set; }
        public string WorkoutId { get; set; }
    }
}
