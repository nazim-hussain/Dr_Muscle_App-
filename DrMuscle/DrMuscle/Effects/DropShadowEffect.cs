﻿using System;
using System.Linq;
using Xamarin.Forms;

namespace DrMuscle.Effects
{
	public class ViewShadowEffect : RoutingEffect
	{
		public float Radius { get; set; }

		public Color Color { get; set; }

		public float DistanceX { get; set; }

		public float DistanceY { get; set; }

		public ViewShadowEffect() : base("CubiSoft.DropShadowEffect")
		{
		}
	}

    public static class Sorting
    {
        public static readonly BindableProperty IsSortableProperty =
            BindableProperty.CreateAttached("IsSortable", typeof(bool), typeof(ListViewSortableEffect), false,
                propertyChanged: OnIsSortableChanged);

        public static bool GetIsSortable(BindableObject view)
        {
            return (bool)view.GetValue(IsSortableProperty);
        }

        public static void SetIsSortable(BindableObject view, bool value)
        {
            view.SetValue(IsSortableProperty, value);
        }

        static void OnIsSortableChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as ListView;
            if (view == null)
            {
                return;
            }

            if (!view.Effects.Any(item => item is ListViewSortableEffect))
            {
                view.Effects.Add(new ListViewSortableEffect());
            }
        }

        class ListViewSortableEffect : RoutingEffect
        {
            public ListViewSortableEffect() : base("CubiSoft.ListViewSortableEffect")
            {

            }
        }
    }
}
