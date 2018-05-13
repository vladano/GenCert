using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace GenCert
{
    public class NewLineOnTabBehavior : Behavior<DataGrid>
    {
        private bool _monitorForTab;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.BeginningEdit += _EditStarting;
            AssociatedObject.CellEditEnding += _CellEnitEnding;
            AssociatedObject.PreviewKeyDown += _KeyDown;
        }

        private void _EditStarting(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == AssociatedObject.Columns.Count - 1)
                _monitorForTab = true;
        }

        private void _CellEnitEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _monitorForTab = false;
        }

        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (_monitorForTab && e.Key == Key.Tab)
            {
                AssociatedObject.CommitEdit(DataGridEditingUnit.Row, false);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.BeginningEdit -= _EditStarting;
            AssociatedObject.CellEditEnding -= _CellEnitEnding;
            AssociatedObject.PreviewKeyDown -= _KeyDown;
            _monitorForTab = false;
        }
    }
}
