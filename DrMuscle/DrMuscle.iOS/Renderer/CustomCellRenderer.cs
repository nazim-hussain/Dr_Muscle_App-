using CoreAnimation;
using CoreGraphics;
using DrMuscle.Cells;
using DrMuscle.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomCell), typeof(CustomCellRenderer))]
namespace DrMuscle.iOS.Renderers
{
	public class CustomCellRenderer : ViewCellRenderer
	{
		public CustomCellRenderer()
		{

		}

        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);
            if (cell != null) cell.BackgroundColor = UIColor.White;
            // get native control (UITextField)


            // Create borders (bottom only)
            CALayer border = new CALayer();
            float width = 1.0f;
            border.BorderColor = new CoreGraphics.CGColor(0.73f, 0.7451f, 0.7647f);  // gray border color
            border.Frame = new CGRect(x: 0, y: (System.nfloat)(cell.Bounds.Height - width), width: cell.Bounds.Width, height: 1.0f);
            border.BorderWidth = width;
            border.BackgroundColor = Color.White.ToCGColor();
            //cell.Layer.AddSublayer(border);
            return cell;
        }


	}
}