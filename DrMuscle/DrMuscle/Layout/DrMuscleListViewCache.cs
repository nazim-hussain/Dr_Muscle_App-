using System;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
    public class DrMuscleListViewCache : ListView
    {
        public DrMuscleListViewCache() : this(Device.RuntimePlatform == Device.Android ? ListViewCachingStrategy.RecycleElement : ListViewCachingStrategy.RecycleElement)
        {
            ItemTapped += DrMuscleListView_ItemTapped;
        }
        public DrMuscleListViewCache(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
        }
        public static event Action ViewCellSizeChangedEvent;
        public event EventHandler EventScrollToTop;
        

        private void DrMuscleListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Analytics.TrackEvent($"Item [{e.Item.ToString()}] tapped");
        }

        public void onScrolltoTop()
        {
            
        }

        public bool IsCellUpdated
        {
            get => (bool)GetValue(IsCellUpdatedProperty);
            set => SetValue(IsCellUpdatedProperty, value);
        }

        public static readonly BindableProperty IsCellUpdatedProperty =
            BindableProperty.Create(nameof(IsCellUpdated), typeof(bool), typeof(DrMuscleListViewCache), false);

        public bool IsScrolled
        {
            get => (bool)GetValue(IsScrolledProperty);
            set => SetValue(IsScrolledProperty, value);
        }

        public bool LastOffset
        {
            get => (bool)GetValue(titleTextProperty);
            set => SetValue(titleTextProperty, value);
        }

        public static readonly BindableProperty titleTextProperty =
            BindableProperty.Create(nameof(LastOffset), typeof(bool), typeof(DrMuscleListViewCache), false);


        public bool SetLastOffset
        {
            get => (bool)GetValue(settitleTextProperty);
            set => SetValue(settitleTextProperty, value);
        }

        public static readonly BindableProperty settitleTextProperty =
            BindableProperty.Create(nameof(SetLastOffset), typeof(bool), typeof(DrMuscleListViewCache), false);

        public static readonly BindableProperty IsScrolledProperty =
            BindableProperty.Create(nameof(IsScrolled), typeof(bool), typeof(DrMuscleListViewCache), false);

        public bool ScrollToTop
        {
            get => (bool)GetValue(ScrollToTopProperty);
            set => SetValue(ScrollToTopProperty, value);
        }

        public static readonly BindableProperty ScrollToTopProperty =
            BindableProperty.Create(nameof(ScrollToTop), typeof(bool), typeof(DrMuscleListViewCache), false);


        public int ItemPosition
        {
            get => (int)GetValue(ItemPositionProperty);
            set => SetValue(ItemPositionProperty, value);
        }

        public static readonly BindableProperty ItemPositionProperty =
            BindableProperty.Create(nameof(ItemPosition), typeof(int), typeof(DrMuscleListViewCache), 0);

        public void OnScrollToTop(bool animate = true)
        //-------------------------------------------------------------------
        {
            //bool animate is not used at this stage, it's always animated.
            EventScrollToTop?.Invoke(this, EventArgs.Empty);
        }

        /// </remarks>
        public void refresh()
        {
            BeginRefresh();
            EndRefresh();
        }

    }
}
