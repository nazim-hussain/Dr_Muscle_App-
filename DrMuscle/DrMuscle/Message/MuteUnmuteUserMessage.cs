using System;
namespace DrMuscle.Message
{
    public class MuteUnmuteUserMessage
    {
        public bool IsMuted { get; set; }
        public string UserId { get; set; }
        public MuteUnmuteUserMessage()
        {
        }
    }
}
