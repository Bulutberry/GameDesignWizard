using System.Windows.Controls;
using System.Windows.Input;
using GameDesignWizard.App.ViewModels;

namespace GameDesignWizard.App.Controls;

public partial class DualListPicker : UserControl
{
    public DualListPicker()
    {
        InitializeComponent();
    }

    private void AvailableItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Transfer(sender, add: true);
        e.Handled = true;
    }

    private void SelectedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Transfer(sender, add: false);
        e.Handled = true;
    }

    private void AvailableItem_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Transfer(sender, add: true);
            e.Handled = true;
        }
    }

    private void SelectedItem_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Transfer(sender, add: false);
            e.Handled = true;
        }
    }

    private void Transfer(object sender, bool add)
    {
        if (sender is not ListBoxItem { DataContext: CatalogOptionViewModel option }
            || DataContext is not DualListPickerViewModel viewModel)
        {
            return;
        }

        var command = add ? viewModel.AddCommand : viewModel.RemoveCommand;
        if (command.CanExecute(option))
        {
            command.Execute(option);
        }
    }
}
