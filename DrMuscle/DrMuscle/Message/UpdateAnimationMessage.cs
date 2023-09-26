using System;
namespace DrMuscle.Message
{
    public class UpdateAnimationMessage
    {
        public bool ShouldAnimateFirstDemo { get; set; }
        public bool ShouldAnimate { get; set; }
        public UpdateAnimationMessage()
        {
        }
    }
}
