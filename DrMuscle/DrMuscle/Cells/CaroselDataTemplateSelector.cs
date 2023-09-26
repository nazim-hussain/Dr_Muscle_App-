using System;
using DrMuscle.Helpers;
using DrMuscle.OnBoarding;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public class CaroselDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate page1DataTemplate;
        private readonly DataTemplate page2DataTemplate;
        private readonly DataTemplate page3DataTemplate;
        private readonly DataTemplate page4DataTemplate;

        public CaroselDataTemplateSelector()
        {
            // Retain instances!
            this.page1DataTemplate = new DataTemplate(typeof(Page1));
            this.page2DataTemplate = new DataTemplate(typeof(Page2));
            this.page3DataTemplate = new DataTemplate(typeof(Page3));
            this.page4DataTemplate = new DataTemplate(typeof(Page4));

        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as string;
            if (messageVm == null)
                return null;
            try
            {
                if (messageVm.Equals("Page1"))
                    return this.page1DataTemplate;
                if (messageVm.Equals("Page2"))
                    return this.page2DataTemplate;
                if (messageVm.Equals("Page3"))
                    return this.page3DataTemplate;
                else
                    return this.page4DataTemplate;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}

