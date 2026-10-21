using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace MathTutor.WinUI.Navigation;

internal static class VerticalListNavigation
{
    public static void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not ListView list ||
            e.Key is not (VirtualKey.Up or VirtualKey.Down or VirtualKey.Left or VirtualKey.Right))
            return;

        e.Handled = true;
        if (e.Key is VirtualKey.Left or VirtualKey.Right || list.Items.Count == 0)
            return;

        int index = Math.Clamp(
            list.SelectedIndex + (e.Key == VirtualKey.Down ? 1 : -1),
            0, list.Items.Count - 1);
        list.SelectedIndex = index;
        list.ScrollIntoView(list.SelectedItem);
        list.UpdateLayout();
        if (list.ContainerFromIndex(index) is ListViewItem item)
        {
            item.Focus(FocusState.Keyboard);
        }
    }
}
