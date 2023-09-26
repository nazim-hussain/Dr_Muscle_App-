using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Plugin.Connectivity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LiftedCell : ViewCell
    {
        public LiftedCell()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<Message.BodyweightUpdateMessage>(this, "BodyweightUpdateMessage", (obj) =>
            {
                var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                var weights = new MultiUnityWeight(value, "kg");
                LblBodyweight.Text = string.Format("{0:0.00}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weights.Kg : weights.Lb);
            });
        }

        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = "Update body weight",
                MaxLength = 7,
                

                Placeholder = "Tap to enter your weight",
                OkText = AppResources.Ok,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (weightResponse.Ok)
                    {
                        if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                        {
                            return;
                        }
                        var weightText = weightResponse.Value.Replace(",", ".");
                        decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);

                        LocalDBManager.Instance.SetDBSetting("BodyWeight", new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().Replace(",", "."));
                        var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                        var weights = new MultiUnityWeight(value, "kg");
                        LblBodyweight.Text = string.Format("{0:0.00}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weights.Kg : weights.Lb);
                        await DrMuscleRestClient.Instance.SetUserBodyWeight(new UserInfosModel()
                        {
                            BodyWeight = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                        });
                        //Xamarin.Forms.MessagingCenter.Send<BodyweightUpdateMessage>(new BodyweightUpdateMessage() { }, "BodyweightUpdateMessage");
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                        {
                            IHealthData _healthService = DependencyService.Get<IHealthData>();
                            await _healthService.GetWeightPermissionAsync(async (r) =>
                            {
                                var a = r;
                                if (r)
                                {
                                    _healthService.SetWeight(LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (double)Math.Round(weights.Kg, 2) : (double)Math.Round(weights.Lb, 2));
                                }
                            });
                        }
                        return;
                    }

                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        protected void FirsttimeExercisePopup_OnTextChanged(PromptTextChangedArgs obj)
        {

            const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
            var text = obj.Value.Replace(",", ".");
            bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }
    }
}
