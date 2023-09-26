using System;
using DrMuscle.Cells;
using DrMuscle.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(SetsCell), typeof(HeaderCellRenderer))]
namespace DrMuscle.iOS.Renderer
{
    public class HeaderCellRenderer : ViewCellRenderer
    {
        public HeaderCellRenderer()
        {
        }
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            //return base.GetCell(item, reusableCell, tv);
            UITableViewCell lCell = base.GetCell(item, reusableCell, tv);
            //if (lCell != null)
            //{
            //    SetBackgroundColor(lCell, item, UIColor.Clear);
            //}
            tv.RowHeight = UITableView.AutomaticDimension;
            tv.EstimatedRowHeight = 70;
            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                tv.SectionHeaderTopPadding = 0;
            }
            return lCell;
        }
        //public override UITableViewCell GetCell(Cell pCell, UITableViewCell pReusableCell, UITableView pTableView)
        //{
        //    UITableViewCell lCell = base.GetCell(pCell, pReusableCell, pTableView);
        //    if (lCell != null)
        //    {
        //        SetBackgroundColor(lCell, pCell, UIColor.Clear);
        //    }
        //    return lCell;
        //}
    }
}
