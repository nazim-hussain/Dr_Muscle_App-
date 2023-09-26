using System;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
	public class NoAnimationNavigationPage : NavigationPage, IActiveAware
	{
		public NoAnimationNavigationPage(Page startupPage) : base(startupPage)
		{
		}

        public event EventHandler IsActiveChanged;

        bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
