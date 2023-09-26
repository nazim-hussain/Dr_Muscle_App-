using System;
namespace DrMuscleWebApiSharedModel
{
    public class ChatRoomModel : BaseModel
    {
        public ChatRoomModel()
        {
        }

        public long Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverEmail { get; set; }
        public bool IsBothReplied { get; set; }
        public ChatModel ChatModel { get; set; } 
        public System.DateTime UpdatedAt { get; set; }
        public bool IsV1user { get; set; }
    }
}
