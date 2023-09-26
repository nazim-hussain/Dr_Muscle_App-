using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscle.Helpers
{
    public static class FacebookSettings
    {
        public static string[] ReadPermissions = new string[] {
            "email"
        };

        public static string AppName { get; } = "Dr. Muscle";
    }
}
;