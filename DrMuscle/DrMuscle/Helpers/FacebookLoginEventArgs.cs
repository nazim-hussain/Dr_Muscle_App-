using DrMuscle.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscle.Helpers
{
    public class FacebookLoginEventArgs : EventArgs
    {
        public FacebookUser User { get; set; }
    }
}
