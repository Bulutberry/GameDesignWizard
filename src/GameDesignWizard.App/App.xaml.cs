using System.Windows;
using GameDesignWizard.App.ViewModels;
using GameDesignWizard.Infrastructure.Catalog;
using GameDesignWizard.Infrastructure.Data;

namespace GameDesignWizard.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var repository = new SqliteCatalogRepository(new AppDbContextFactory());
            var viewModel = new MainWindowViewModel(repository);
            await viewModel.InitializeAsync();

            MainWindow = new MainWindow(viewModel);
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"GameDesignWizard could not open its local catalog.\n\n{exception.Message}",
                "Startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
