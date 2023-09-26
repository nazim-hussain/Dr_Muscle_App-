using System;

namespace DrMuscle
{
    interface IActiveAware
    {
        bool IsActive { get; set; }
        event EventHandler IsActiveChanged;
    }
}
