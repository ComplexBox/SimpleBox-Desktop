using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SimpleBox.Utils
{
    public static class OpenContextMenu
    {
        private static RoutedUICommand _openContextMenuCommand;

        public static RoutedUICommand OpenContextMenuCommand
        {
            get
            {
                if (!(_openContextMenuCommand is null)) return _openContextMenuCommand;
                _openContextMenuCommand = new RoutedUICommand();
                Application.Current.MainWindow?.CommandBindings.Add(
                    new CommandBinding(
                        _openContextMenuCommand,
                        (sender, args) =>
                        {
                            Button button = args.Source as Button;
                            if (button?.ContextMenu is null) return;
                            ContextMenu menu = button.ContextMenu;
                            string mode = args.Parameter as string ?? "Bottom";
                            Enum.TryParse(mode, true, out PlacementMode placementMode);
                            menu.Placement = placementMode;
                            menu.PlacementTarget = button;
                            menu.IsOpen = true;
                        },
                        (sender, args) =>
                        {
                            args.CanExecute = true;
                            args.Handled = true;
                        }));

                return _openContextMenuCommand;
            }
        }
    }
}
