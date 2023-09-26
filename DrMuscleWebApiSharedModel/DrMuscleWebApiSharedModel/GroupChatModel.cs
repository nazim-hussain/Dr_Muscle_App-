using System;
namespace DrMuscleWebApiSharedModel
{
    public class GroupChatModel : BaseModel
    {
        public GroupChatModel()
        {
        }

        public long Id { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }

        public System.DateTime UpdatedAt { get; set; }

    }
}
