using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using DrMuscle.Entity;
using DrMuscle.Helpers;
using DrMuscle.Localize;
using System.Globalization;
using DrMuscle.Message;
using DrMuscle.Resx;

namespace DrMuscle.Layout
{
    public partial class LanguagesPage : DrMusclePage
    {
        public List<Language> languageItems = new List<Language>();
        public LanguagesPage()
        {
            InitializeComponent();
            LanguageListView.ItemsSource = languageItems;
            LanguageListView.ItemTapped += LanguageListView_ItemTapped; ;
        }

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            languageItems = Languages.GetAppLanguages();
            var selectedLanguage = languageItems.Where(x => x.LanguageCode.Equals(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value)).FirstOrDefault();
            if (selectedLanguage != null)
                selectedLanguage.IsSelected = true;
            LanguageListView.ItemsSource = languageItems;
            LblLanguage.Text = AppResources.LanguageLowercase;
        }

        void LanguageListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var language = (Language)e.Item;
            var oldSelected = languageItems.Where(x => x.IsSelected == true).FirstOrDefault();

            if (oldSelected != null)
                oldSelected.IsSelected = false;
            language.IsSelected = true;
            LocalDBManager.Instance.SetDBSetting("AppLanguage", language.LanguageCode);
            var localize = DependencyService.Get<ILocalize>();
            if (localize != null)
            {
                localize.SetLocale(new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value));
                MessagingCenter.Send(new LanguageChangeMessage(), "LocalizeUpdated");
            }
            LanguageListView.SelectedItem = null;
            Navigation.PopModalAsync(true);
        }

        private void Close_Tapped(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync(true);
        }
    }
}
