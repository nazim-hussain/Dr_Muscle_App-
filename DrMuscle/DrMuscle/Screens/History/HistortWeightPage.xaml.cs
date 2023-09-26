using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Screens.History
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistortWeightPage : DrMusclePage
    {
        private List<UserWeight> allWeights = new List<UserWeight>();
        private List<YearlyUserWeight> yearlyWeights = new List<YearlyUserWeight>();
        private StackLayout contextMenu = null;


        public HistortWeightPage()
        {
            InitializeComponent();
            
            Title = "Body weight history";
        }

    }
    
    public class YearlyUserWeight : List<UserWeight>
    {
        public string YearDate { get; set; }
    }
    
    public class HistoryWeightDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HistoryDateTemplate { get; set; }
        public DataTemplate HistorySetTemplate { get; set; }
        

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var we = ((UserWeight)item);
            if (we.Weight == 0)
            {
                return HistoryDateTemplate;
            }

            return HistorySetTemplate;
        }
    }
}