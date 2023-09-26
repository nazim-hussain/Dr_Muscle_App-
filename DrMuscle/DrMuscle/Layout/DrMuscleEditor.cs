using System;
using Xamarin.Forms;
namespace DrMuscle.Layout
{
    public class DrMuscleEditor : Editor
    {
        public DrMuscleEditor()
        {
            TextChanged += DrMuscleEditor_TextChanged;
        }

        void DrMuscleEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        ~DrMuscleEditor()
        {
            TextChanged -= DrMuscleEditor_TextChanged;
        }

    }
}
