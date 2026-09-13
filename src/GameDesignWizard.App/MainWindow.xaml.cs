using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameDesignWizard.App.ViewModels;
using Microsoft.Win32;

namespace GameDesignWizard.App;

public partial class MainWindow : Window
{
    public MainWindow()
        : this(new MainWindowViewModel())
    {
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void AvailableTopic_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        TransferTopic(sender, add: true);
        e.Handled = true;
    }

    private void SelectedTopic_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        TransferTopic(sender, add: false);
        e.Handled = true;
    }

    private void AvailableTopic_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TransferTopic(sender, add: true);
            e.Handled = true;
        }
    }

    private void SelectedTopic_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TransferTopic(sender, add: false);
            e.Handled = true;
        }
    }

    private void TransferTopic(object sender, bool add)
    {
        if (sender is not ListBoxItem { DataContext: CatalogOptionViewModel topic }
            || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var command = add ? viewModel.AddTopicCommand : viewModel.RemoveTopicCommand;
        if (command.CanExecute(topic))
        {
            command.Execute(topic);
        }
    }

    private async void ImportCatalog_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var fileDialog = new OpenFileDialog
        {
            Title = "Choose a catalog file",
            Filter = "Catalog files (*.txt;*.xlsx)|*.txt;*.xlsx|UTF-8 text (*.txt)|*.txt|Excel workbook (*.xlsx)|*.xlsx",
            CheckFileExists = true,
            Multiselect = false
        };
        if (fileDialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var preview = await viewModel.CreateCatalogImportPreviewAsync(fileDialog.FileName);
            var previewWindow = new CatalogImportPreviewWindow(preview) { Owner = this };
            if (previewWindow.ShowDialog() == true)
            {
                await viewModel.ApplyCatalogImportAsync(preview);
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "Catalog import",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
