using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class UserInfosModel : BaseModel
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string MassUnit { get; set; }
        public string Password { get; set; }
		public DateTime CreationDate { get; set; }
    }
}
