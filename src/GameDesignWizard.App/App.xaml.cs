using System.Windows;
using GameDesignWizard.App.ViewModels;
using GameDesignWizard.Documents.Pdf;
using GameDesignWizard.Infrastructure.Catalog;
using GameDesignWizard.Infrastructure.Data;
using GameDesignWizard.Infrastructure.Ideas;

namespace GameDesignWizard.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var contextFactory = new AppDbContextFactory();
            var repository = new SqliteCatalogRepository(contextFactory);
            var ideaRepository = new SqliteGameIdeaRepository(contextFactory);
            var mediaStorage = new ManagedMediaStorage();
            var pdfExporter = new MigraDocGameIdeaPdfExporter(mediaStorage);
            var workbookExporter = new ClosedXmlGameIdeaWorkbookExporter();
            var viewModel = new MainWindowViewModel(
                repository,
                new CatalogFileReader(),
                ideaRepository,
                mediaStorage,
                pdfExporter,
                workbookExporter);
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
