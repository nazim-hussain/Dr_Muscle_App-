using System;
using System.Net.Http;

namespace DrMuscleWebApiSharedModel
{
    public class ErrorResponse : BaseModel
    {
        public Exception Ex { get; set; }
        public HttpResponseMessage Response { get; set; }
    }
}
