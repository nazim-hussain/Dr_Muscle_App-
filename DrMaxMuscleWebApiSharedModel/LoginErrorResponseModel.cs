using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class LoginErrorResponseModel : BaseModel
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
