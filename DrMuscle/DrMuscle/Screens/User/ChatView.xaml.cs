using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Controls;
using DrMuscle.Dependencies;
using DrMuscle.Effects;
using DrMuscle.Entity;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using DrMuscle.Screens.Subscription;
using DrMuscle.Services;
using DrMuscleWebApiSharedModel;
using FFImageLoading;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Screens.User
{
    public partial class ChatView : ContentView
    {
        
        bool IsLoading = false;
        bool IsLoadMore = false;
        bool IsAdmin = false;
        bool IsSupportMessagesLoaded = false;
        string supportUrl = "";
        public ObservableCollection<Messages> groupChannelsList = new ObservableCollection<Messages>();
        public ObservableCollection<Messages> messageList = new ObservableCollection<Messages>();

        public ChatView()
        {
            InitializeComponent();
            //Live
            
            //Test
            //SendBirdClient.Init("05F82C36-1159-4179-8C49-5910C7F51D7D");
            //lstView.ItemsSource = groupChannelsList;
            //lstView.ItemTapped += LstView_ItemTapped;
           
        }

    }
    public class MutedList
    {
        public object phone_number { get; set; }
        public int remaining_duration { get; set; }
        public bool require_auth_for_profile_image { get; set; }
        public string description { get; set; }
        public int end_at { get; set; }
        public string user_id { get; set; }
        public string nickname { get; set; }
        public string profile_url { get; set; }
        public int seconds { get; set; } = -1;
        public Metadata metadata { get; set; }
    }

    public class Muted
    {
        public List<MutedList> muted_list { get; set; }
        public string next { get; set; }
    }
}
