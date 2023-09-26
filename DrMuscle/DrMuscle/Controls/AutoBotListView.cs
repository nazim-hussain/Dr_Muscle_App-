using System;
using Xamarin.Forms;

namespace DrMuscle.Controls
{
    public class AutoBotListView : ListView
    {
        public AutoBotListView()
        {
            this.ItemSelected += OnItemSelected;
            this.ItemTapped += OnItemTapped;
        }

        public bool IsOnBoarding
        {
            get => (bool)GetValue(IsOnBoardingProperty);
            set => SetValue(IsOnBoardingProperty, value);
        }

        public static readonly BindableProperty IsOnBoardingProperty =
            BindableProperty.Create(nameof(IsOnBoarding), typeof(bool), typeof(AutoBotListView), false);


        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = (AutoBotListView)sender;
            if (e == null) return;
            listView.SelectedItem = null;
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            SelectedItem = null;
        }
    }
}
