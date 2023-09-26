using CoreText;
using Foundation;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using DrMuscle.iOS.Renderers;
using DrMuscle;
using DrMuscle.iOS.Controls;
using System.ComponentModel;
using DrMuscle.Controls;

[assembly: ExportRenderer(typeof(HyperlinkLabel), typeof(HyperlinkLabelRenderer))]
namespace DrMuscle.iOS.Renderers
{
    public class HyperlinkLabelRenderer : ViewRenderer<HyperlinkLabel, HyperlinkUIView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<HyperlinkLabel> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != e.OldElement)
            {
                if (e.OldElement != null)
                    e.OldElement.PropertyChanged -= Element_PropertyChanged;

                if (e.NewElement != null)
                    e.NewElement.PropertyChanged += Element_PropertyChanged;
            }
            try
            {
                if (e.NewElement != null)
                { 
                //if (Control != null)
                //{
                foreach (var item in this.Subviews)
                {
                    item.RemoveFromSuperview();
                }
                    var textView = new HyperlinkUIView();
                    SetNativeControl(textView);
                //textView.DataDetectorTypes = UIDataDetectorType.All;
                    SetText();
                    //}
                }
            }
            catch (System.Exception ex)
            {

            }

        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null)
            {
                if (e.PropertyName == HyperlinkLabel.RawTextProperty.PropertyName)
                    SetText();
            }
        }

        private void SetText()
        {
            try
            {

            CTStringAttributes attrs = new CTStringAttributes();
            string text = Element.GetText(out List<HyperlinkLabelLink> links);
            if (text != null)
            {
                var str = new NSMutableAttributedString(text);
                str.AddAttribute(UIStringAttributeKey.Font, Element.Font.ToUIFont(), new NSRange(0, str.Length));
                var textColor = (Color)Element.GetValue(Label.TextColorProperty);
                str.AddAttribute(UIStringAttributeKey.ForegroundColor, textColor.ToUIColor(Color.Black),
                    new NSRange(0, str.Length));


                foreach (var item in links)
                {
                    str.AddAttribute(UIStringAttributeKey.Link, new NSUrl(item.Link), new NSRange(item.Start, item.Text.Length));
                        str.AddAttribute(UIStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), new NSRange(item.Start, item.Text.Length));
                    }
                Control.AttributedText = str;


            }

            }
            catch (System.Exception ex)
            {

            }
        }
    }
}