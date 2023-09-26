using System;
namespace DrMuscleWebApiSharedModel
{
    public class ChatModel : BaseModel
    {
        public ChatModel()
        {
        }
        public long Id { get; set; }
        public string SenderId { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        
        public bool IsWorkoutAnalytics { get; set; }
        public long ChatRoomId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsFromAI { get; set; }
        public System.DateTime CreatedAt { get; set; }

    }
}
