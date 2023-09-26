using System;

using Foundation;
using WatchKit;
using WatchConnectivity;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;
using UIKit;

namespace DrMuscleWatch.WatchOSExtension
{
    public partial class InterfaceController : WKInterfaceController
    {

        protected InterfaceController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.

        }
        PhoneToWatchModel PhoneToWatch;
        //interval timer
        NSTimer intervalTimer;
        int RIR = 0;
        WKPickerItem[] WKPickerItems;
        public override void Awake(NSObject context)
        {
            base.Awake(context);
            WCSessionManager.SharedManager.StartSession();
            // Configure interface objects here.
            Console.WriteLine("{0} awake with context", this);
            WCSessionManager.SharedManager.ApplicationContextUpdated += DidReceiveApplicationContext;
            GroupReps.SetHidden(true);
            GroupWeight.SetHidden(true);
            BtnSaveSet.SetHidden(true);
            GroupTimer.SetHidden(true);
            GroupPicker.SetHidden(true);
            GroupPicker.SetBackgroundColor(UIColor.Black);
            LblWeighting.SetHidden(true);
            GroupTimer.SetBackgroundColor(UIColor.Black);
            BtnFinishAndSave.SetHidden(true);
            BtnFinishExercise.SetHidden(true);
            BtnNextExercise.SetHidden(true);
            LblLoading.SetHidden(true);
            BtnOpenWorkout.SetHidden(false);
            BtnOpenWorkout.SetTitle("Start workout");
            WKPickerItems = new WKPickerItem[] { 
                new WKPickerItem() {Caption ="How hard?", Title="" },
                new WKPickerItem() {Caption ="That was very hard", Title="Very hard" },
                new WKPickerItem() {Caption ="I could have done 1-2 more", Title="1-2" },
                new WKPickerItem() {Caption ="I could have done 3-4 more", Title="3-4" },
                new WKPickerItem() {Caption ="I could have done 5-6 more", Title="5-6" },
                new WKPickerItem() {Caption ="I could have done 7+ more", Title="7+" },
            };
            PickerRIR.SetItems(WKPickerItems);
            
        }


        public override void WillActivate()
        {
            // This method is called when the watch view controller is about to be visible to the user.
            Console.WriteLine("{0} will activate", this);
            PickerRIR.SetSelectedItem(0);
        }

        public override void DidDeactivate()
        {
            // This method is called when the watch view controller is no longer visible to the user.
            Console.WriteLine("{0} did deactivate", this);
            WCSessionManager.SharedManager.ApplicationContextUpdated -= DidReceiveApplicationContext;
        }


        public void DidReceiveApplicationContext(WCSession session, Dictionary<string, object> applicationContext)
        {
            var message = (string)applicationContext["MessagePhone"];
            
            var phoneModel = JsonConvert.DeserializeObject<PhoneToWatchModel>(message);
            
            BtnOpenWorkout.SetHidden(true);
            LblLoading.SetHidden(true);
            BtnFinishExercise.SetTitle("Finish exercise");
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NewSet)
            {
                PhoneToWatch = phoneModel;
                Console.WriteLine($"Application context update received : {phoneModel.Weight}");
                LblRepsValue.SetText($"{phoneModel.Reps}");
                LblWeightValue.SetText($"{phoneModel.Weight}");
                LblExerciseName.SetText(phoneModel.Label);
                LblExerciseName.SetHidden(false);
                GroupReps.SetHidden(false);
                GroupWeight.SetHidden(false);
                BtnSaveSet.SetHidden(false);
                GroupTimer.SetHidden(true);
                LblWeighting.SetHidden(true);
                BtnFinishAndSave.SetHidden(true);
                BtnFinishExercise.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.StartTimer)
            {
                BtnFinishAndSave.SetHidden(true);
                BtnFinishExercise.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                TimerReset(phoneModel.Seconds);
                GroupTimer.SetHidden(false);
                GroupPicker.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.EndTimer)
            {
                GroupTimer.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NewSetBehind)
            {
                PhoneToWatch = phoneModel;
                Console.WriteLine($"Application context update received : {phoneModel.Weight}");
                LblExerciseName.SetText(phoneModel.Label);
                LblRepsValue.SetText($"{phoneModel.Reps}");
                LblWeightValue.SetText($"{phoneModel.Weight}");
                GroupReps.SetHidden(false);
                GroupWeight.SetHidden(false);
                LblExerciseName.SetHidden(false);
                BtnSaveSet.SetHidden(false);
                LblWeighting.SetHidden(true);
                BtnFinishAndSave.SetHidden(true);
                BtnFinishExercise.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishExercise)
            {
                phoneModel.WatchMessageType = WatchMessageType.FinishExercise;
                PhoneToWatch = phoneModel;
                timer.Stop();
                GroupTimer.SetHidden(true);
                GroupReps.SetHidden(true);
                LblExerciseName.SetHidden(true);
                GroupWeight.SetHidden(true);
                BtnSaveSet.SetHidden(true);
                BtnFinishExercise.SetHidden(false);
                BtnFinishAndSave.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.NextExercise)
            {
                GroupTimer.SetHidden(true);
                GroupReps.SetHidden(true);
                LblExerciseName.SetHidden(true);
                GroupWeight.SetHidden(true);
                BtnSaveSet.SetHidden(true);
                BtnNextExercise.SetHidden(false);
                BtnFinishAndSave.SetHidden(true);
                BtnFinishExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishSide1)
            {
                phoneModel.WatchMessageType = WatchMessageType.FinishSide1;
                PhoneToWatch = phoneModel;
                GroupTimer.SetHidden(true);
                GroupReps.SetHidden(true);
                GroupWeight.SetHidden(true);
                LblExerciseName.SetHidden(true);
                
                BtnSaveSet.SetHidden(true);
                BtnFinishExercise.SetHidden(false);
                BtnFinishExercise.SetTitle("Finish side 1");

                BtnFinishAndSave.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }

            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.FinishSaveWorkout)
            {
                GroupTimer.SetHidden(true);
                GroupReps.SetHidden(true);
                GroupWeight.SetHidden(true);
                LblExerciseName.SetHidden(true);
                BtnSaveSet.SetHidden(true);
                BtnFinishAndSave.SetHidden(false);
                BtnFinishExercise.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(true);
            }
            if (phoneModel != null && phoneModel.WatchMessageType == WatchMessageType.RIR)
            {
                BtnFinishAndSave.SetHidden(true);
                BtnFinishExercise.SetHidden(true);
                BtnNextExercise.SetHidden(true);
                GroupPicker.SetHidden(false);
                PickerRIR.Focus();
            }
        }

        void TimerReset(int interval)
        {
            // A method to reset timer to 0 and start timer


            timer.Stop();
            NSDate time = NSDate.FromTimeIntervalSinceNow(interval);
            timer.SetDate(time);
            timer.Start();

            if (intervalTimer != null)
                if (intervalTimer.IsValid) { intervalTimer.Invalidate(); }
            intervalTimer = NSTimer.CreateScheduledTimer(interval, (ttime) =>
            {
                intervalTimer.Invalidate();
                GroupTimer.SetHidden(true);


            }); //Repeat of not

        }

        partial void BtnHideTimerClicked()
        {
            GroupTimer.SetHidden(true);
            PhoneToWatch.WatchMessageType = WatchMessageType.EndTimer;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void TimerGestureTapped(NSObject sender)
        {
            GroupTimer.SetHidden(true);
            PhoneToWatch.WatchMessageType = WatchMessageType.EndTimer;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        //public void DidReceiveApplicationContext(WCSession session, Dictionary<string, object> applicationContext)
        //{
        //    //var message = (SendWatchMessage)applicationContext["MessagePhone"];
        //    //if (message != null)
        //    //{
        //    //    Console.WriteLine($"Application context update received : {message}");

        //    //}
        //    LblRepsValue.SetText("Received");
        //}

        partial void BtnSavesetclicked()
        {

            PhoneToWatch.WatchMessageType = WatchMessageType.SaveSet;
            GroupReps.SetHidden(true);
            GroupWeight.SetHidden(true);
            BtnSaveSet.SetHidden(true);
            GroupTimer.SetHidden(true);
            LblWeighting.SetHidden(true);
            GroupPicker.SetHidden(true);
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnWeightMore()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.WeightMore;            
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }
        partial void BtnWeightLess()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.WeightLess;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnRepsLess()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.RepsLess;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }
        partial void BtnRepsMore()
        {
            PhoneToWatch.WatchMessageType = WatchMessageType.RepsMore;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnFinishandSaveClicked()
        {
            BtnFinishAndSave.SetHidden(true);
            LblLoading.SetHidden(false);
            PhoneToWatch.WatchMessageType = WatchMessageType.FinishSaveWorkout;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnFinishExerciseClicked()
        {
            BtnFinishExercise.SetHidden(true);
            LblLoading.SetHidden(false);
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnNextExerrciseClicked()
        {
            BtnNextExercise.SetHidden(true);
            LblLoading.SetHidden(false);
            PhoneToWatch.WatchMessageType = WatchMessageType.NextExercise;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }
        partial void RIRPickerSelected(NSObject value)
        {
            PickerRIR.Focus();
            RIR = int.Parse(value.ToString());
        }

        partial void BtnHidePickerTapped()
        {
            GroupPicker.SetHidden(true);
            PhoneToWatch.WatchMessageType = WatchMessageType.RIR;
            if (RIR>0)
                PhoneToWatch.RIR = RIR-1;
            else
                PhoneToWatch.RIR = 0;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }

        partial void BtnOpenWorkoutClicked()
        {
            BtnOpenWorkout.SetHidden(true);
            LblWeighting.SetHidden(false);
            if (PhoneToWatch == null)
            PhoneToWatch = new PhoneToWatchModel();
            PhoneToWatch.WatchMessageType = WatchMessageType.StartWorkout;
            WCSessionManager.SharedManager.UpdateApplicationContext(new Dictionary<string, object>() { { "MessagePhone", $"{JsonConvert.SerializeObject(PhoneToWatch)}" } });
        }
    }
}

