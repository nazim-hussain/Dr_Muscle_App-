using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class LoginModel : BaseModel
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
