using System;
using Android.Widget;
using Android.OS;
using Android.Support.Wearable.Views;
using Java.Interop;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using System.Linq;
using Android.Support.Wearable.Activity;
using Android.App;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;
using static Android.Gms.Wearable.DataClient;
using System.Collections;

namespace DrMuscleWear
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : WearableActivity, IOnDataChangedListener, GoogleApiClient.IOnConnectionFailedListener, GoogleApiClient.IConnectionCallbacks
    {
        TextView textView;
        Button button;
        private GoogleApiClient _client;
        LinearLayout GroupSaveset, groupTimer, groupPicker;
        PhoneToWatchModel PhoneToWatch;
        Spinner spinner;
        CountDown countDownTimer;
        int RIR = 0;
        Button btnSaveSet, btnHide, btnSave, btnRepsLess, btnRepsMore, btnWeightLess, btnWeightMore, btnFinishExercise, btnFinishAndSaveWorkout, btnNextExercise;
        TextView txtReps, txtWeight, txtTimer, txtExerciseName, txtWaiting, txtLoading;
        const string _syncPath = "/DrMuscleWear/Data";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _client = new GoogleApiClient.Builder(this)
                             .AddApi(WearableClass.Api)
                             .Build();
            SetContentView(Resource.Layout.activity_main);
            var v = FindViewById<WatchViewStub>(Resource.Id.watch_view_stub);
            v.LayoutInflated += delegate
            {

                GroupSaveset = FindViewById<LinearLayout>(Resource.Id.GroupSaveset);
                groupTimer = FindViewById<LinearLayout>(Resource.Id.GroupTimer);
                groupPicker = FindViewById<LinearLayout>(Resource.Id.groupPicker);
                btnSaveSet = FindViewById<Button>(Resource.Id.btnSaveSet);

                btnRepsLess = FindViewById<Button>(Resource.Id.BtnRepsLess);
                btnRepsMore = FindViewById<Button>(Resource.Id.BtnRepsMore);
                btnWeightLess = FindViewById<Button>(Resource.Id.BtnWeightLess);
                btnWeightMore = FindViewById<Button>(Resource.Id.BtnWeightMore);

                btnHide = FindViewById<Button>(Resource.Id.btnHide);
                btnNextExercise = FindViewById<Button>(Resource.Id.btnNextExercise);
                btnFinishExercise = FindViewById<Button>(Resource.Id.btnfinishExercise);
                btnFinishAndSaveWorkout = FindViewById<Button>(Resource.Id.btnfinishandSaveWorkout);
                btnSave = FindViewById<Button>(Resource.Id.btnSave);

                txtReps = FindViewById<TextView>(Resource.Id.txtRepsValue);
                txtWeight = FindViewById<TextView>(Resource.Id.txtWeightValue);
                txtTimer = FindViewById<TextView>(Resource.Id.txtTimer);
                txtExerciseName = FindViewById<TextView>(Resource.Id.txtExerciseName);
                txtLoading = FindViewById<TextView>(Resource.Id.txtLoading);
                txtWaiting = FindViewById<TextView>(Resource.Id.txtWaiting);

                spinner = FindViewById<Spinner>(Resource.Id.spinner1);



                GroupSaveset.Visibility = Android.Views.ViewStates.Gone;
                groupTimer.Visibility = Android.Views.ViewStates.Gone;
                txtWaiting.Visibility = Android.Views.ViewStates.Visible;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
                txtLoading.Visibility = Android.Views.ViewStates.Gone;

                //
                //Setup spinner
                var list = new ArrayList();
                list.Add("That was very, very hard");
                list.Add("I could have done 1-2 more");
                list.Add("I could have done 3-4 more");
                list.Add("I could have done 5-6 more");
                list.Add("I could have done 7+ more");
                ArrayAdapter arrayAdapter = new ArrayAdapter(this, Resource.Layout.custom_spinner_adapter, list);
                arrayAdapter.SetDropDownViewResource(Resource.Layout.custom_spinner_adapter);
                spinner.Adapter = arrayAdapter;

                btnRepsLess.Click += delegate
                {
                    BtnRepsLess();
                };

                btnRepsMore.Click += delegate
                {
                    BtnRepsMore();
                };
                btnWeightLess.Click += delegate
                {
                    BtnWeightLess();
                };
                btnWeightMore.Click += delegate
                {
                    BtnWeightMore();
                };

                btnSave.Click += delegate
                {
                    BtnHidePickerTapped();
                };
                btnSaveSet.Click += delegate
                {
                    BtnSavesetclicked();
                };
                btnHide.Click += delegate
                {
                    BtnHideTimerClicked();
                };
                btnNextExercise.Click += delegate
                {
                    BtnNextExerrciseClicked();
                };
                btnFinishExercise.Click += delegate
                {
                    BtnFinishExerciseClicked();
                };
                btnFinishAndSaveWorkout.Click += delegate
                {
                    BtnFinishandSaveClicked();
                };
            };
            SetAmbientEnabled();
        }


        [Obsolete]
        public void SendData()
        {
            try
            {
                PhoneToWatch = new PhoneToWatchModel();
                //var request = PutDataMapRequest.Create(_syncPath);
                //var map = request.DataMap;
                //map.PutString("Message", "Vinz says Hello from Wearable!");
                //map.PutLong("UpdatedAt", DateTime.UtcNow.Ticks);
                //WearableClass.DataApi.PutDataItem(_client, request.AsPutDataRequest());
                PhoneToWatch.WatchMessageType = WatchMessageType.EndTimer;
                PhoneToWatch.Id = 0;
                PhoneToWatch.Label = "test";

                //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
                SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");

            }
            finally
            {
                //_client.Disconnect();
            }

        }
        protected override void OnStart()
        {
            base.OnStart();
            _client.Connect();
        }
        public void OnConnected(Bundle p0)
        {
            WearableClass.DataApi.AddListener(_client, this);
        }

        public void OnConnectionSuspended(int reason)
        {
            Android.Util.Log.Error("GMS", "Connection suspended " + reason);
            WearableClass.DataApi.RemoveListener(_client, this);
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            Android.Util.Log.Error("GMS", "Connection failed " + result.ErrorCode);
        }

        protected override void OnStop()
        {
            base.OnStop();
            _client.Disconnect();
        }


        protected override void OnResume()
        {
            base.OnResume();
            WearableClass.GetDataClient(this).AddListener(this);
        }
        protected override void OnPause()
        {
            base.OnPause();
            WearableClass.GetDataClient(this).RemoveListener(this);
        }
        public void OnDataChanged(DataEventBuffer dataEvents)
        {
            var dataEvent = Enumerable.Range(0, dataEvents.Count)
                                      .Select(i => dataEvents.Get(i).JavaCast<IDataEvent>())
                                      .FirstOrDefault(x => x.Type == DataEvent.TypeChanged && x.DataItem.Uri.Path.Equals("/DrMuscleWear/Data"));
            if (dataEvent == null)
                return;

            //do stuffs here
            txtLoading.Visibility = Android.Views.ViewStates.Gone;
            var dataMapItem = DataMapItem.FromDataItem(dataEvent.DataItem);
            var map = dataMapItem.DataMap;
            string message = dataMapItem.DataMap.GetString("Message");

            var phoneModel = JsonConvert.DeserializeObject<PhoneToWatchModel>(message);
            if (phoneModel != null && phoneModel.SenderPlatform == Platform.Watch)
                return;
            txtWaiting.Visibility = Android.Views.ViewStates.Gone;
            btnFinishExercise.Text = "Finish exercise";

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NewSet)
            {
                PhoneToWatch = phoneModel;
                Console.WriteLine($"Application context update received : {phoneModel.Weight}");
                txtReps.Text = $"{phoneModel.Reps}";
                txtWeight.Text = $"{phoneModel.Weight}";
                txtExerciseName.Text = phoneModel.Label;
                txtExerciseName.Visibility = Android.Views.ViewStates.Visible;
                GroupSaveset.Visibility = Android.Views.ViewStates.Visible;
                groupTimer.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                //GroupPicker.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.StartTimer)
            {
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                TimerReset(phoneModel.Seconds);
                groupTimer.Visibility = Android.Views.ViewStates.Visible;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.EndTimer)
            {
                groupTimer.Visibility = Android.Views.ViewStates.Gone;
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NewSetBehind)
            {
                PhoneToWatch = phoneModel;
                Console.WriteLine($"Application context update received : {phoneModel.Weight}");
                txtReps.Text = $"{phoneModel.Reps}";
                txtWeight.Text = $"{phoneModel.Weight}";
                txtExerciseName.Text = phoneModel.Label;
                txtExerciseName.Visibility = Android.Views.ViewStates.Visible;
                GroupSaveset.Visibility = Android.Views.ViewStates.Visible;
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishExercise)
            {
                phoneModel.WatchMessageType = WatchMessageType.FinishExercise;
                PhoneToWatch = phoneModel;
                countDownTimer?.Cancel();

                groupTimer.Visibility = Android.Views.ViewStates.Gone;
                GroupSaveset.Visibility = Android.Views.ViewStates.Gone;

                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Visible;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NextExercise)
            {
                groupTimer.Visibility = Android.Views.ViewStates.Gone;
                GroupSaveset.Visibility = Android.Views.ViewStates.Gone;
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Visible;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;
            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishSide1)
            {
                phoneModel.WatchMessageType = WatchMessageType.FinishSide1;
                PhoneToWatch = phoneModel;
                GroupSaveset.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Visible;

                btnFinishExercise.Text = "Finish side 1";

                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Gone;

            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishSaveWorkout)
            {
                GroupSaveset.Visibility = Android.Views.ViewStates.Gone;
                groupTimer.Visibility = Android.Views.ViewStates.Gone;
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnNextExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Visible;

                groupPicker.Visibility = Android.Views.ViewStates.Gone;
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.RIR)
            {
                txtWaiting.Visibility = Android.Views.ViewStates.Gone;
                btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
                btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
                groupPicker.Visibility = Android.Views.ViewStates.Visible;
                spinner.Focusable = true;
            }

        }

        void TimerReset(int interval)
        {
            // A method to reset timer to 0 and start timer
            if (countDownTimer != null)
            {
                countDownTimer.Cancel();
                countDownTimer.Tick -= CountDownTimer_Tick;
                countDownTimer.Finish -= CountDownTimer_Finish;
            }

            countDownTimer = new CountDown(interval * 1000, (long)1000);
            countDownTimer.Start();
            countDownTimer.Tick += CountDownTimer_Tick;
            countDownTimer.Finish += CountDownTimer_Finish;

        }

        private void CountDownTimer_Finish()
        {
            groupTimer.Visibility = Android.Views.ViewStates.Gone;
        }

        private void CountDownTimer_Tick(long millisUntilFinished)
        {
            txtTimer.Text = $"{millisUntilFinished / 1000}";
        }

        void BtnHideTimerClicked()
        {

            groupTimer.Visibility = Android.Views.ViewStates.Gone;
            if (PhoneToWatch == null)
                PhoneToWatch = new PhoneToWatchModel();
            PhoneToWatch.WatchMessageType = WatchMessageType.EndTimer;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void TimerGestureTapped()
        {
            groupTimer.Visibility = Android.Views.ViewStates.Gone;
            PhoneToWatch.WatchMessageType = WatchMessageType.EndTimer;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }


        void BtnSavesetclicked()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.SaveSet;
            GroupSaveset.Visibility = Android.Views.ViewStates.Gone;
            groupTimer.Visibility = Android.Views.ViewStates.Gone;
            txtWaiting.Visibility = Android.Views.ViewStates.Gone;
            groupPicker.Visibility = Android.Views.ViewStates.Gone;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void BtnWeightMore()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.WeightMore;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }
        void BtnWeightLess()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.WeightLess;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void BtnRepsLess()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.RepsLess;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }
        void BtnRepsMore()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.RepsMore;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void BtnFinishandSaveClicked()
        {
            btnFinishAndSaveWorkout.Visibility = Android.Views.ViewStates.Gone;
            txtLoading.Visibility = Android.Views.ViewStates.Visible;
            PhoneToWatch.WatchMessageType = WatchMessageType.FinishSaveWorkout;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void BtnFinishExerciseClicked()
        {
            btnFinishExercise.Visibility = Android.Views.ViewStates.Gone;
            txtLoading.Visibility = Android.Views.ViewStates.Visible;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        void BtnNextExerrciseClicked()
        {
            btnNextExercise.Visibility = Android.Views.ViewStates.Gone; ;
            txtLoading.Visibility = Android.Views.ViewStates.Visible;
            PhoneToWatch.WatchMessageType = WatchMessageType.NextExercise;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }
        void RIRPickerSelected(Object value)
        {
            spinner.Focusable = true;
            RIR = int.Parse(value.ToString());
        }

        void BtnHidePickerTapped()
        {
            RIR = spinner.SelectedItemPosition;
            groupPicker.Visibility = Android.Views.ViewStates.Gone;
            PhoneToWatch.WatchMessageType = WatchMessageType.RIR;
            if (RIR > 0)
                PhoneToWatch.RIR = RIR;
            else
                PhoneToWatch.RIR = 0;
            PhoneToWatch.SenderPlatform = Platform.Watch;
            //WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
            SendMessageToPhone($"{JsonConvert.SerializeObject(PhoneToWatch)}");
        }

        [Obsolete]
        public void SendMessageToPhone(string msg)
        {
            try
            {
                var request = PutDataMapRequest.Create(_syncPath);
                var map = request.DataMap;
                map.PutString("Message", msg);
                map.PutLong("UpdatedAt", DateTime.UtcNow.Ticks);
                WearableClass.DataApi.PutDataItem(_client, request.AsPutDataRequest());

            }
            finally
            {
                //_client.Disconnect();
            }

        }


    }


    public delegate void TickEvent(long millisUntilFinished);
    public delegate void FinishEvent();

    public class CountDown : CountDownTimer
    {
        public event TickEvent Tick;
        public event FinishEvent Finish;

        public CountDown(long totaltime, long interval)
            : base(totaltime, interval)
        {
        }

        public override void OnTick(long millisUntilFinished)
        {
            if (Tick != null)
                Tick(millisUntilFinished);
        }

        public override void OnFinish()
        {
            if (Finish != null)
                Finish();
        }
    }

}


