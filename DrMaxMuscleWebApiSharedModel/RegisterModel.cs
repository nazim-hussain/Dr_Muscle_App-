using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class RegisterModel : BaseModel
    {
        public string Firstname { get; set; }
        
        public string Lastname { get; set; }
        
        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string SelectedGender { get; set; }

        public string MassUnit { get; set; }
    }
}
