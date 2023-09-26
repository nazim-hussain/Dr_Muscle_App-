using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class LoginModel : BaseModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string NewPassword { get; set; }

        public string Validation { get; set; }
       
    }
}
