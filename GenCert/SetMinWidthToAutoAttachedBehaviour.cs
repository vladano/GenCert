using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GenCert
{
    public class SetMinWidthToAutoAttachedBehaviour
    {
        public static bool GetSetMinWidthToAuto(DependencyObject obj)
        {
            return (bool)obj.GetValue(SetMinWidthToAutoProperty);
        }

        public static void SetSetMinWidthToAuto(DependencyObject obj, bool value)
        {
            obj.SetValue(SetMinWidthToAutoProperty, value);
        }

        // Using a DependencyProperty as the backing store for SetMinWidthToAuto.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetMinWidthToAutoProperty =
            DependencyProperty.RegisterAttached("SetMinWidthToAuto", typeof(bool), typeof(SetMinWidthToAutoAttachedBehaviour), new UIPropertyMetadata(false, WireUpLoadedEvent));

        public static void WireUpLoadedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (DataGrid)d;

            var doIt = (bool)e.NewValue;

            if (doIt)
            {
                grid.Loaded += SetMinWidths;
            }
        }

        public static void SetMinWidths(object source, EventArgs e)
        {
            var grid = (DataGrid)source;

            foreach (var column in grid.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
    }
}
