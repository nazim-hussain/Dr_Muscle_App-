using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Controls;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Screens.User
{
    public partial class SupportPage : DrMusclePage
    {

        bool IsLoading = false;
        bool IsLoadMore = false;
        bool IsAdmin = false;
        public ObservableCollection<Messages> messageList = new ObservableCollection<Messages>();


        public SupportPage()
        {
            InitializeComponent();
            
        }
        
    }
}
