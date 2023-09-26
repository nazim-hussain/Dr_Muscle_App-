using Xamarin.Forms;

namespace DrMuscle
{
    class LazyContentViewBehavior : LoadContentOnActivateBehavior<ContentView>
    {
        protected override void SetContent(ContentView element, View contentView)
        {
            element.Content = contentView;
        }
    }
}
