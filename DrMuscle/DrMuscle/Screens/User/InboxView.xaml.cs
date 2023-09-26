using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using DrMuscle.Helpers;
using Xamarin.Forms;
using DrMuscle.Resx;
     

namespace DrMuscle.Screens.User
{
    public partial class InboxView : ContentView
    {


        bool IsLoading = false;
        bool IsLoadMore = false;
        string supportUrl = "";
        public ObservableCollection<GroupChannelType> groupChannelsList = new ObservableCollection<GroupChannelType>();

        public InboxView()
        {
            InitializeComponent();
            //Live
           
        }

    }
}
