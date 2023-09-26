using System;
namespace DrMuscleWebApiSharedModel
{
    public class UnlockCodeResponseModel : BaseModel
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }
}
