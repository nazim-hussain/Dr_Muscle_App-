using System;
using System.Collections.Generic;
using DrMuscle.Helpers;
using Xamarin.Forms;
using DrMuscle.Resx;

namespace DrMuscle.Layout
{
    public partial class TimerOverlay : DrMusclePage
    {
        bool isPageVisible = false;
        public TimerOverlay()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            //Timer.Instance.OnTimerStop += Instance_OnTimerStop;
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });
        }

        private void RefreshLocalized()
        {
            LblRestFor.Text = AppResources.Restfor;
            HideButton.Text = AppResources.Hide;
            //SkipButton.Text = AppResources.Skip;
            LblSecondsText.Text = AppResources.Seconds;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            isPageVisible = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isPageVisible = false;
        }
        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            LblProgressSeconds.Text = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
            ProgressCircle.Progress = 0;
        }

        async void OnTimerDone()
        {
            if (isPageVisible)
                await PagesFactory.PopAsync(true);
        }
        void OnTimerChange(int remaining)
        {
            LblProgressSeconds.Text = remaining.ToString();
            var percentage = (float)remaining / Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) * 100.0;
            ProgressCircle.Progress = 100 - (float)percentage;
        }


        async void ButtonHide_Clicked(object sender, System.EventArgs e)
        {
            await PagesFactory.PopAsync(true);
        }

        async void ButtonSkip_Clicked(object sender, System.EventArgs e)
        {

            await PagesFactory.PopAsync(true);
            await Timer.Instance.StopTimer();
            if (ToolbarItems.Count > 0)
            {
                this.ToolbarItems.RemoveAt(0);
                timerToolbarItem = new ToolbarItem("", "stopwatch.png", SlideTimerAction, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Insert(0, timerToolbarItem);
            }
        }
    }
}
