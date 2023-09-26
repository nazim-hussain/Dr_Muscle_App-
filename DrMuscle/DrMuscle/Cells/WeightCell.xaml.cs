using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscleWebApiSharedModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WeightCell : ViewCell
    {
        public WeightCell()
        {
            InitializeComponent();
        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await LoadSavedWeights();

            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.ForceUpdateSize();
                });
            }
        }

        private async Task LoadSavedWeights()
        {
            try
            {
                
                plotView3.Model = null;
                var weightList = await DrMuscleRestClient.Instance.GetUserWeights();
                if (weightList.Count == 0)
                    return;
                if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    return;
                if (LocalDBManager.Instance.GetDBSetting("massunit") == null)
                    return;
                var plotModel = new PlotModel
                {
                    Title = "Weights",
                    //Subtitle = "for the 3 last workouts",
                    Background = OxyColors.Transparent,
                    PlotAreaBackground = OxyColors.Transparent,
                    TitleColor = OxyColor.Parse("#23253A"),
                    TitleFontSize = 12,
                    TitleFontWeight = FontWeights.Bold,
                    PlotAreaBorderColor = OxyColor.Parse("#23253A"),

                };

                double minY;
                double maxY;

                switch (LocalDBManager.Instance.GetDBSetting("massunit")?.Value)
                {
                    default:
                    case "kg":
                        minY = (double)(Math.Floor(weightList.Min(o => o.Weight) / 10) * 10) - 100;
                        maxY = (double)(Math.Ceiling(weightList.Max(o => o.Weight) / 10) * 10) + 100;
                        break;
                    case "lb":
                        minY = (double)(Math.Floor(weightList.Min(o => new MultiUnityWeight((decimal)o.Weight, "kg").Lb) / 10) * 10) - 100;
                        maxY = (double)(Math.Ceiling(weightList.Max(o => new MultiUnityWeight((decimal)o.Weight, "kg").Lb) / 10) * 10) + 100;
                        break;
                }

                LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = minY - 5, Maximum = maxY + 5, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColor.Parse("#23253A") };
                LinearAxis xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColor.Parse("#23253A") };
                // xAxis.LabelFormatter = _formatter;
                xAxis.MinimumPadding = 5;
                xAxis.IsPanEnabled = false;
                xAxis.IsZoomEnabled = false;
                xAxis.Minimum = DateTimeAxis.ToDouble(weightList.Min(l => l.CreatedDate).AddDays(-1));
                xAxis.Maximum = DateTimeAxis.ToDouble(weightList.Max(l => l.CreatedDate).AddDays(1));

                yAxis.IsPanEnabled = false;
                yAxis.IsZoomEnabled = false;
                plotModel.Axes.Add(yAxis);
                plotModel.Axes.Add(xAxis);


                var s1 = new LineSeries()
                {
                    Color = OxyColor.Parse("#38418C"),
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 6,
                    MarkerStroke = OxyColor.Parse("#38418C"),
                    MarkerFill = OxyColor.Parse("#38418C"),
                    MarkerStrokeThickness = 1,
                    LabelFormatString = "{1:0}",
                    FontSize = 15,
                    TextColor = OxyColor.Parse("#38418C")
                };

                int i = 1;
                foreach (UserWeight m in weightList.OrderBy(w => w.CreatedDate))
                {

                    switch (LocalDBManager.Instance.GetDBSetting("massunit")?.Value)
                    {
                        default:
                        case "kg":


                            s1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(m.CreatedDate), Convert.ToDouble(m.Weight)));
                            break;
                        case "lb":

                            s1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(m.CreatedDate), Convert.ToDouble(new MultiUnityWeight((decimal)m.Weight, "kg").Lb)));
                            break;
                    }
                    i++;
                }

                plotModel.Series.Add(s1);
                Device.BeginInvokeOnMainThread(() =>
                {
                    plotView3.Model = plotModel;
                });
            }
            catch (Exception ex)
            {

            }
        }
    }
}
