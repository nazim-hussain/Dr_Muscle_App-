
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace DrMuscle.iOS.Controls
{
    public class HyperlinkUIView : UITextView
    {
        public HyperlinkUIView()
        {
            Selectable = true;
            Editable = false;
            BackgroundColor = UIColor.Clear;
            var dictionary = new NSDictionary(
                UIStringAttributeKey.ForegroundColor, Constants.AppThemeConstants.BlueLightColor.ToUIColor(),
                UIStringAttributeKey.UnderlineColor, Constants.AppThemeConstants.BlueLightColor.ToUIColor() 
            );
            ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            TextContainerInset = new UIEdgeInsets(0, 0, 0, 0);

            TextContainer.LineFragmentPadding = 0;
            WeakLinkTextAttributes = dictionary;
            ScrollEnabled = false;
        }

        public override bool CanBecomeFirstResponder => false;

        public override bool GestureRecognizerShouldBegin(UIGestureRecognizer gestureRecognizer)
        {
            //Preventing standard actions on UITextView that are triggered after long press
            if (gestureRecognizer is UILongPressGestureRecognizer longpress 
                && longpress.MinimumPressDuration == .5)
                return false;

            return true;
        }

        public override bool CanPerform(Selector action, NSObject withSender) => false;

        public override void LayoutSubviews()
        {
            //Make the TextView as large as its content
            base.LayoutSubviews();
            var x = new CGSize(this.Frame.Size.Width, double.MaxValue);

            var fits = SizeThatFits(x);

            var frame = Frame;

            frame.Size = fits;
        }
    }
}