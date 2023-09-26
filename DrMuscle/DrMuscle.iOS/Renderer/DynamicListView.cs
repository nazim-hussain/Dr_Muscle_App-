using System;
using System.ComponentModel;
using CoreGraphics;
using DrMuscle.Cells;
using DrMuscle.Layout;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DrMuscleListViewCache), typeof(Controls.ListViewRenderer))]
namespace Controls
{


    public class ListViewRenderer : Xamarin.Forms.Platform.iOS.ListViewRenderer
    {
        public double lastScrollPoint = 0;
        public ListViewRenderer()
        {
            SetsCell.ViewCellSizeChangedEvent += UpdateTableView;
        }


        private void UpdateTableView()
        {
            //if (Control != null)
            //{ 

            //    if (tv == null) return;
            //    tv.BeginUpdates();
            //    tv.EndUpdates();
            try
            {
                var tv = Control as UITableView;

                if (tv != null)
                {
                    //tv.ReloadData();
                    //tv.SetNeedsFocusUpdate();
                    //tv.LayoutIfNeeded();

                }
            }
            catch (Exception ex)
            {

            }

            //}
        }



        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            try
            {

                if (Control != null && e.PropertyName.Equals("IsCellUpdated"))
                {
                    ((UITableView)Control).SeparatorStyle = UITableViewCellSeparatorStyle.None;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Control != null)
                        {
                            var offset = ((UITableView)Control).ContentOffset;
                            ((UITableView)Control).BeginUpdates();
                            ((UITableView)Control).EndUpdates();
                            ((UITableView)Control).ReloadData();
                            ((UITableView)Control).ContentOffset = offset;
                        }
                    
                });
                }
                var tv = Control as UITableView;
                if (Control != null && !e.PropertyName.Equals("SelectedItem"))
                {
                    
                    var offset = ((UITableView)Control).ContentOffset;
                    UIView.AnimateAsync(0, () => {
                        tv.ReloadData();
                    });
                    ((UITableView)Control).ContentOffset = offset;
                }
                if (Control != null && e.PropertyName.Equals("LastOffset"))
                {
                    lastScrollPoint = ((UITableView)Control).ContentOffset.Y;
                }
                if (Control != null && e.PropertyName.Equals("SetLastOffset"))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(50);
                        ((UITableView)Control).ContentOffset = new CGPoint(0, lastScrollPoint);
                    });
                }


            }
            catch (Exception ex)
            {

            }
            if (Control != null && e.PropertyName.Equals("IsScrolled"))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        DrMuscleListViewCache list = (DrMuscleListViewCache)Element;
                        var position = list.ItemPosition > 0 ? list.ItemPosition - 1 : list.ItemPosition;
                        ((UITableView)Control).ContentOffset = new CGPoint(0, 115 * position);
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }

                    //if (Control != null && e.PropertyName.Equals("IsScrolled"))
                    //{
                    //    Device.BeginInvokeOnMainThread(() =>
                    //    {
                    //        // DrMuscleListViewCache list = (DrMuscleListViewCache)Element;

                    //        // ((UITableView)Control).ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    //        //((UITableView)Control).ReloadData();

                    //        // ((UITableView)Control).ReloadRows(new NSIndexPath[] { NSIndexPath.FromItemSection(0, list.ItemPosition) }, UITableViewRowAnimation.None);
                    //        //((UITableView)Control).BeginUpdates();
                    //        //((UITableView)Control).EndUpdates();
                    //        // Control.SetContentOffset(Control.cont)
                    //        // Control.SetContentOffset(new CGPoint(0, Control.ContentOffset.Y), true);
                    //        //Control.ScrollRectToVisible(new CGRect(0, 0, size.Width, size.Height),true);
                    //        try
                    //        {
                    //            //DrMuscleListViewCache list = (DrMuscleListViewCache)Element;
                    //            //((UITableView)Control).ReloadRows(((UITableView)Control).IndexPathsForVisibleRows, UITableViewRowAnimation.None);
                    //        }
                    //        catch (Exception ex)
                    //        {

                    //        }
                    //    });
                    //}


                }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            // Invoke base
            base.OnElementChanged(e);


            //TableView.RowHeight = UITableView.AutomaticDimension;
            if (Control != null)
            {
                var view = (DrMuscleListViewCache)Element;

                view.EventScrollToTop += View_EventScrollToTop; ;
                ((UITableView)Control).SectionFooterHeight = 0.1f;
                ((UITableView)Control).EstimatedRowHeight = 70;  
                ((UITableView)Control).RowHeight = UITableView.AutomaticDimension;
                
                ((UITableView)Control).SeparatorStyle = UITableViewCellSeparatorStyle.None;
                
                ((UITableView)Control).ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
        }

        private void View_EventScrollToTop(object sender, EventArgs e)
        {
            try
            {
                if (Control != null)
                    Control.ScrollRectToVisible(new CoreGraphics.CGRect(0, 0, 1, 1), true);
            }
            catch (Exception ex)
            {

            }
        }

       
    }
}