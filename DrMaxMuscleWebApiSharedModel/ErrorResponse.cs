using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class ErrorResponse : BaseModel
    {
        public Exception Ex { get; set; }
        public HttpResponseMessage Response { get; set; }
    }
}
