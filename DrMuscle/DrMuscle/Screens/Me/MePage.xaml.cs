using System;
using System.Collections.Generic;
using DrMuscle.Dependencies;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using Xamarin.Forms;

namespace DrMuscle.Screens.Me
{
    public partial class MePage : DrMusclePage, IActiveAware
    {
        public event EventHandler IsActiveChanged;

        bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public MePage()
        {
            InitializeComponent();
           
           
        }
       
    }
}
