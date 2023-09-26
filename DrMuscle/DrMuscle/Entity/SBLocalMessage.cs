using System;
namespace DrMuscle.Entity
{
    public class SBLocalMessage
    {
        public string message_type { get; set; } = "MESG";
        public string user_id { get; set; }
        public string message { get; set; }
        public bool send_push { get; set; } = false;
    }
}
