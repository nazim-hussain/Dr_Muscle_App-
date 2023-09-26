using System;
namespace DrMuscleWebApiSharedModel
{
    public class DeviceModel : BaseModel
    {
        public DeviceModel()
        {
        }

        public string DeviceToken { get; set; }
        public string UserId { get; set; }
        public string Platform { get; set; }
        public System.DateTime UpdatedAt { get; set; }

    }
}
