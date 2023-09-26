using System;
using System.ComponentModel;
using CoreGraphics;
using DrMuscle.Controls;
using DrMuscle.iOS.Renderer;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedLabel), typeof(ExtendedLabelRenderer))]
namespace DrMuscle.iOS.Renderer
{

    public class ExtendedLabelRenderer : LabelRenderer
    {
        UITextView uilabelleftside;
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var view = (ExtendedLabel)Element;
            if (view == null) return;
            Control.TextColor = UIColor.Clear;
            uilabelleftside = new UITextView();
            uilabelleftside.Text = view.Text;
            uilabelleftside.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            uilabelleftside.TextContainerInset = new UIEdgeInsets(0,0,0,0);
            if (view.FontAttributes == FontAttributes.Bold)
                uilabelleftside.Font = UIFont.SystemFontOfSize((float)view.FontSize, UIFontWeight.Semibold);
            else
                uilabelleftside.Font = UIFont.SystemFontOfSize((float)view.FontSize);
            //uilabelleftside.Selectable = false;
            uilabelleftside.Editable = false;
            uilabelleftside.TextColor = view.TextColor.ToUIColor();
            // Setting the data detector types mask to capture all types of link-able data
            uilabelleftside.DataDetectorTypes = UIDataDetectorType.All;
            uilabelleftside.BackgroundColor = UIColor.Clear;
            uilabelleftside.ScrollEnabled = false;
            uilabelleftside.TextContainer.LineFragmentPadding = 0;
            uilabelleftside.TranslatesAutoresizingMaskIntoConstraints = true;

            var dictionary = new NSDictionary(
                UIStringAttributeKey.ForegroundColor, Constants.AppThemeConstants.BlueLightColor.ToUIColor(),
                UIStringAttributeKey.UnderlineColor, Constants.AppThemeConstants.BlueLightColor.ToUIColor()
            );

            uilabelleftside.WeakLinkTextAttributes = dictionary;
            foreach (var item in this.Subviews)
            {
                if (item != null)
                    item.RemoveFromSuperview();
            }
            var label = new UILabel();
            label.Text = view.Text;
            CGRect frame = label.Frame;
            label.Frame = new CGRect(0, 0, view.Width, frame.Size.Height);
            label.Lines = 0;
            label.SizeToFit();
            uilabelleftside.ContentSize = new CGSize(label.Frame.Size.Width, label.Frame.Size.Height + 48);
            uilabelleftside.Frame = new CGRect(0, 0, label.Frame.Size.Width, label.Frame.Size.Height + 48);
            // overriding Xamarin Forms Label and replace with our native control
            AddSubview(uilabelleftside);


            //SetNativeControl(uilabelleftside);


        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            var view = (ExtendedLabel)Element;
            if (view == null) return;
            //if (e.PropertyName == VisualElement.WidthProperty.PropertyName ||
            //e.PropertyName == VisualElement.HeightProperty.PropertyName)
            //{
                if (Element != null && Element.Bounds.Height > 0 && Element.Bounds.Width > 0)
                {
                    var label = new UILabel();
                    label.Text = view.Text;
                    CGRect frame = label.Frame;
                    label.Frame = new CGRect(0, 0, view.Width, view.Height);
                    label.Lines = 0;
                    label.SizeToFit();

                    uilabelleftside.ContentSize = new CGSize(view.Width, view.Height + 50);
                    uilabelleftside.Frame = new CGRect(0, 0, view.Width, view.Height + 50);

                }
               
            //}
                    
        }
    }
}


