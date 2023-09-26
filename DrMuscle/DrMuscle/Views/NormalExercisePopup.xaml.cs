using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Screens.Workouts;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class NormalExercisePopup : PopupPage
    {
        bool _isbodyweight = false;
        public delegate void TaskCompletedCallBack(string taskResult);
        TaskCompletedCallBack _taskCompletedCallBack;
        public Page _kenkoPage { get; set; }
        ExerciseWorkSetsModel _exercise;
        public NormalExercisePopup(string videoUrl, string titleText, string popupText,string placeholder, ExerciseWorkSetsModel m, bool isBodyweight)
        {
            InitializeComponent();
            //_taskCompletedCallBack = taskCompletedCallBack;
            if (!string.IsNullOrEmpty(videoUrl))
            {
                videoPlayer.Source = videoUrl;
            }
            else
            {
                videoPlayer.IsVisible = false;
            }
            _exercise = m;
            LblTitle.Text = titleText;
            LblDesc.Text = popupText;
            EntryWeight.Placeholder = placeholder;
            if (isBodyweight)
                EntryWeight.TextChanged += RepsPopup_OnTextChanged;
            else
                EntryWeight.TextChanged += BodyweightPopup_OnTextChanged ;
            _isbodyweight = isBodyweight;
        }

        async void BtnDoneClicked(System.Object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(EntryWeight.Text) || string.IsNullOrWhiteSpace(EntryWeight.Text))
                return;
            try
            {
                var weight = int.Parse(EntryWeight.Text);
                if (weight < 1)
                {


                    if (_kenkoPage is KenkoSingleExercisePage)
                    {
                        await UserDialogs.Instance.AlertAsync(new AlertConfig()
                        {
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            Message = _isbodyweight ? "Please enter valid reps" : "Please enter valid weight",
                            Title = "Error"
                        });
                    }
                    if (_kenkoPage is KenkoChooseYourWorkoutExercisePage)
                    {
                        await PopupNavigation.Instance.PopAsync();
                    }
                        
                    return;
                }
            }
            catch (Exception ex)
            {

            }

            var bodyweight = EntryWeight.Text;
            await PopupNavigation.Instance.PopAsync();


                if (_kenkoPage is KenkoSingleExercisePage)
                    ((KenkoSingleExercisePage)(_kenkoPage)).FinishSetup(_exercise, bodyweight, _isbodyweight);

            //Xamarin.Forms.MessagingCenter.Send<BodyweightMessage>(new BodyweightMessage() { BodyWeight = bodyweight }, "BodyweightMessage");
        }

        void BtnCancelClicked(System.Object sender, System.EventArgs e)
        {
            if (_kenkoPage is KenkoSingleExercisePage)
                ((KenkoSingleExercisePage)(_kenkoPage)).CancelClick(_exercise);
            
            PopupNavigation.Instance.PopAsync();

        }

        protected void BodyweightPopup_OnTextChanged(object obj, TextChangedEventArgs args)
        {
            try
            {

                Entry entry = (Xamarin.Forms.Entry)obj;
                const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
                var text = entry.Text.Replace(",", ".");
                bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                if (IsValid == false && !string.IsNullOrEmpty(entry.Text))
                {
                    double result;
                    entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
                    double.TryParse(entry.Text, out result);
                    entry.Text = result.ToString();
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected void RepsPopup_OnTextChanged(object obj, TextChangedEventArgs args)
        {
            try
            {

                Entry entry = (Xamarin.Forms.Entry)obj;
                const string textRegex = @"^\d+(?:)?$";
                var text = entry.Text.Replace(",", ".");
                bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                if (IsValid == false && !string.IsNullOrEmpty(entry.Text))
                {
                    double result;
                    entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
                    double.TryParse(entry.Text, out result);
                    entry.Text = result.ToString();
                }

            }
            catch (Exception ex)
            {

            }
        }

        void EntryWeight_Completed(System.Object sender, System.EventArgs e)
        {
            BtnDoneClicked(sender, e);
        }
    }
}
