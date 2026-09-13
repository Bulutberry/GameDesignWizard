using System.Windows;
using GameDesignWizard.App.ViewModels;

namespace GameDesignWizard.App;

public partial class CatalogImportPreviewWindow : Window
{
    public CatalogImportPreviewWindow(CatalogImportPreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Import_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
