using Xamarin.Forms;

namespace DrMuscle
{
    class LazyContentPageBehavior : LoadContentOnActivateBehavior<ContentPage>
    {
        protected override void SetContent(ContentPage page, View contentView)
        {
            page.Content = contentView;
        }
    }
    class LazyNavigationPageBehavior : LoadContentOnActivateBehavior<NavigationPage>
    {
        protected override void SetContent(NavigationPage element, View contentView)
        {
            ((ContentPage)element.RootPage).Content = contentView;
        }
    }

}
