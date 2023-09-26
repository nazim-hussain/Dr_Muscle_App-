using System;
namespace DrMuscle.Entity
{
    public class UserProfile
    {
        public UserProfile()
        {
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public Uri Picture { get; set; }
    }
}
