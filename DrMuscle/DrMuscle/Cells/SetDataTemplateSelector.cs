using System;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public class SetDataTemplateSelector : DataTemplateSelector
    {
        

        private readonly DataTemplate setNoralCell;
        private readonly DataTemplate setsNextCell;
        private readonly DataTemplate setsCell;
        private readonly DataTemplate setsCloseCell;
        private readonly DataTemplate setDemoCell;
        private readonly DataTemplate setNewDemo1Cell;
        private readonly DataTemplate setCompletedCell;
        private readonly DataTemplate setNewDemo2Cell;
        public SetDataTemplateSelector()
        {
            // Retain instances!
            this.setNoralCell = new DataTemplate(typeof(SetDisplayCell));
            this.setsCell = new DataTemplate(typeof(SetsCell));
            this.setsNextCell = new DataTemplate(typeof(SetsNextCell));
            this.setCompletedCell = new DataTemplate(typeof(WelcomeCell));
            this.setDemoCell = new DataTemplate(typeof(SetsDemoCell));
            this.setNewDemo1Cell = new DataTemplate(typeof(NewDemo1Cell));
            this.setNewDemo2Cell = new DataTemplate(typeof(NewDemo2Cell));
            this.setsCloseCell = new DataTemplate(typeof(SetCloseCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (App.IsNUX)
            {
                if (App.IsIntro)
                {
                    var m = item as WorkoutLogSerieModelRef;
                    if (m.IsNext)
                        return this.setsNextCell;
                    else
                        return this.setsCell;
                }
                if (CurrentLog.Instance.IsDemoRunningStep2)
                    return this.setNewDemo2Cell;
                else
                    return this.setNewDemo1Cell;
                
            }

            //var workoutLogItem = item as WorkoutLogSerieModelRef;
            //if (workoutLogItem.IsNext)

            var model = item as WorkoutLogSerieModelRef;
            if (model.IsNext)
            return this.setsNextCell;
            else
            {
                if (model.IsHeaderCell)
                    return this.setsCell;
                if (!model.IsFirstWorkSet && !model.IsLastSet)
                    return this.setsCloseCell;
                return this.setsCell;
            }
            //else
            //    return this.setNoralCell;


        }
    }
}
