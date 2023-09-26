using System;
using DrMuscle.Enums;
using Xamarin.Forms;

namespace DrMuscle.Message
{
    public class GeneralMessage
    {
        public GeneralPopupEnum PopupEnum { get; set; }
        public string GeneralText { get; set; }
        public bool IsCanceled { get; set; }

    }
}

