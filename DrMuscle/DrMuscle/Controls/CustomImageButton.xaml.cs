using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace DrMuscle.Controls
{
    public partial class CustomImageButton : ContentView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(CustomImageButton), null);
        public event EventHandler Clicked;
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomImageButton), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        //public static readonly BindableProperty TextStyleProperty = BindableProperty.Create(nameof(TextStyle), typeof(Style), typeof(CustomImageButton), Application.Current.Resources["BoldLabelBaseStyle"]);

        //public Style TextStyle
        //{
        //    get { return (Style)GetValue(TextStyleProperty); }
        //    set { SetValue(TextStyleProperty, value); }
        //}

        public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(CustomImageButton), null);

        public ICommand TapCommand
        {
            get { return (ICommand)GetValue(TapCommandProperty); }

            set { SetValue(TapCommandProperty, value); }
        }

        public static readonly BindableProperty TextFontSizeProperty = BindableProperty.Create(nameof(TextFontSize), typeof(double), typeof(CustomImageButton), 14.0D);

        public double TextFontSize
        {
            get { return (double)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }

        public static readonly BindableProperty TextFontColorProperty = BindableProperty.Create(nameof(TextFontColor), typeof(Color), typeof(CustomImageButton), Color.White);

        public Color TextFontColor
        {
            get { return (Color)GetValue(TextFontColorProperty); }
            set { SetValue(TextFontColorProperty, value); }
        }


        //public static readonly BindableProperty TextFontFamilyProperty = BindableProperty.Create(nameof(TextFontFamily), typeof(string), typeof(CustomImageButton), AppThemeConstants.MyriadProBold);

        //public string TextFontFamily
        //{
        //    get { return (string)GetValue(TextFontFamilyProperty); }
        //    set { SetValue(TextFontFamilyProperty, value); }
        //}

        public CustomImageButton()
        {
            InitializeComponent();
            ButtonImage.BindingContext = this;
            ButtonText.BindingContext = this;
            ButtonImage.SetBinding(Image.SourceProperty, nameof(Source));
            ButtonText.SetBinding(Label.TextProperty, nameof(Text));
            //ButtonText.SetBinding(Label.StyleProperty, nameof(TextStyle));
            ButtonText.SetBinding(Label.FontSizeProperty, nameof(TextFontSize));
            //ButtonText.SetBinding(Label.FontFamilyProperty, nameof(TextFontFamily));
            ButtonText.SetBinding(Label.TextColorProperty, nameof(TextFontColor));
        }

        private async void Button_Tapped(object sender, System.EventArgs e)
        {
            // await this.FadeTo(0.5, 50);
            TapCommand?.Execute(null);
            Clicked?.Invoke(sender, EventArgs.Empty);
            // await this.FadeTo(1, 50);
        }
    }
}
