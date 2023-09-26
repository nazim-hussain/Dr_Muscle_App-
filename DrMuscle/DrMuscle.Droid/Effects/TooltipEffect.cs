using System;
using System.ComponentModel;
using Android.Views;
using DrMuscle.Droid.Effects;
using DrMuscle.Effects;
using IT.Sephiroth.Android.Library.Tooltip;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
//using Com.Tooltip;

//[assembly: ResolutionGroupName("CrossGeeks")]
[assembly: ExportEffect(typeof(DroidTooltipEffect), nameof(TooltipEffect))]
namespace DrMuscle.Droid.Effects
{
    public class DroidTooltipEffect : PlatformEffect 
    {
        //Tooltip.Builder _builder;
        public DroidTooltipEffect()
        {
            
        }
        void Show()
        {
            try
            {

            var control = Control ?? Container;

            var text = TooltipEffect.GetText(Element);
            
            if (!string.IsNullOrEmpty(text))
            {
                var parentContent = control.RootView;
                var position = TooltipEffect.GetPosition(Element);
                

                switch(position)
                {
                    case TooltipPosition.Top:
                        //var build = new Tooltip.Builder(control);
                        var b = new Tooltip.Builder(101)
                                .Anchor(control, Tooltip.Gravity.Top)
                                .ClosePolicy(Tooltip.ClosePolicy.TouchAnywhereNoConsume, 40000)
                                .Text(text)
                                .FloatingAnimation(Tooltip.AnimationBuilder.Slow)
                                .FadeDuration(200)
                               .WithStyleId( TooltipEffect.GetBackgroundColor(Element).Equals(Xamarin.Forms.Color.FromHex("#97D2F3")) ? Resource.Style.ToolTipLayoutCustomStyleReys : Resource.Style.ToolTipLayoutCustomStyle)
                                .FitToScreen(false)
                                .MaxWidth(600)
                                .ShowDelay(200)
                                .ToggleArrow(true)
                                
                                .Build();
                        
                        Tooltip.Make(MainActivity._currentActivity, b).Show();
                        break;

                    case TooltipPosition.Bottom:
                        var b2 = new Tooltip.Builder(101)
                                .Anchor(control, Tooltip.Gravity.Bottom)
                                .ClosePolicy(Tooltip.ClosePolicy.TouchAnywhereNoConsume, 40000)
                                .Text(text)
                                .FadeDuration(200)
                                .FloatingAnimation(Tooltip.AnimationBuilder.Slow)
                                .WithStyleId(TooltipEffect.GetBackgroundColor(Element).Equals(Xamarin.Forms.Color.FromHex("#97D2F3")) ? Resource.Style.ToolTipLayoutCustomStyleReys : Resource.Style.ToolTipLayoutCustomStyle)
                                .FitToScreen(false)
                               .MaxWidth(600)
                                .ShowDelay(200)
                                .ToggleArrow(true)
                                .Build();
                        Tooltip.Make(MainActivity._currentActivity, b2).Show();
                        
                        break;
                    case TooltipPosition.Left:
                        var b3 = new Tooltip.Builder(101)
                                .Anchor(control, Tooltip.Gravity.Left)
                                .ClosePolicy(Tooltip.ClosePolicy.TouchAnywhereNoConsume, 40000)
                                .Text(text)
                                .FadeDuration(200)
                                .FloatingAnimation(Tooltip.AnimationBuilder.Slow)
                                .WithStyleId(TooltipEffect.GetBackgroundColor(Element).Equals(Xamarin.Forms.Color.FromHex("#97D2F3")) ? Resource.Style.ToolTipLayoutCustomStyleReys : Resource.Style.ToolTipLayoutCustomStyle)
                                .FitToScreen(false)
                               .MaxWidth(600)
                                .ShowDelay(200)
                                .ToggleArrow(true)
                                .Build();
                        Tooltip.Make(MainActivity._currentActivity, b3).Show();

                        break;
                }
            }

            }
            catch (Exception ex)
            {

            }
        }
        void OnTap(object sender, EventArgs e)
        {
            var control = Control ?? Container;

            var text = TooltipEffect.GetText(Element);

            if (!string.IsNullOrEmpty(text))
            {
               //ToolTip.Builder builder;
                var parentContent = control.RootView;

                var position = TooltipEffect.GetPosition(Element);
                //switch (position)
                //{
                //    case TooltipPosition.Top:
                //        _builder = new Tooltip.Builder(control);
                //        _builder.SetText(text).SetCancelable(true).SetDismissOnClick(true);
                //        _builder.SetCornerRadius((float)12);
                //        _builder.SetPadding((float)20);
                //        _builder.SetBackgroundColor(TooltipEffect.GetBackgroundColor(Element).ToAndroid());
                //        _builder.SetTextColor(Xamarin.Forms.Color.White.ToAndroid());
                //        _builder.SetGravity((int)GravityFlags.Top);
                //        _builder.Show();
                    
                //        break;
                    
                //    default:
                //        _builder = new Tooltip.Builder(control);
                //        _builder.SetText(text).SetCancelable(true).SetDismissOnClick(true);
                //        _builder.SetCornerRadius((float)12);
                //        _builder.SetPadding((float)20);
                //        _builder.SetBackgroundColor(TooltipEffect.GetBackgroundColor(Element).ToAndroid());
                //        _builder.SetTextColor(Xamarin.Forms.Color.White.ToAndroid());
                //        //_builder.SetPadding(3);
                //        _builder.SetGravity((int)GravityFlags.Bottom);
                //        _builder.Show();
                //        break;
                //}
            }

        }
        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
            if (args.PropertyName == TooltipEffect.HasShowTooltipProperty.PropertyName)
            {
                if (TooltipEffect.GetHasShowTooltip(Element))
                    Show();
            }
        }

        protected override void OnAttached()
        {
            var control = Control ?? Container;
            //control.Click += OnTap;
        }


        protected override void OnDetached()
        {
            var control = Control ?? Container;
            //control.Click -= OnTap;
            
           // _toolTipsManager.FindAndDismiss(control);
        }

        
    }
}
