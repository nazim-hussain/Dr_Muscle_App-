using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using Xamarin.Forms;
using System.Linq;
using Plugin.Connectivity;

namespace DrMuscle.Screens.User
{
    public partial class InboxPage : DrMusclePage, IActiveAware
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
     
        public InboxPage()
        {
            InitializeComponent();
           

        }
       
    }
}
