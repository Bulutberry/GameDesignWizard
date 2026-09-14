using System.IO;
using System.Windows;
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

    private void AddMedia_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var fileDialog = new OpenFileDialog
        {
            Title = "Choose images or audio",
            Filter = "Supported media|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp;*.mp3;*.wav;*.m4a;*.ogg;*.flac|Images|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp|Audio|*.mp3;*.wav;*.m4a;*.ogg;*.flac",
            CheckFileExists = true,
            Multiselect = true
        };
        if (fileDialog.ShowDialog(this) == true)
        {
            viewModel.AddMediaFiles(fileDialog.FileNames);
        }
    }

    private async void ExportIdeaPdf_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel { SelectedSavedIdea: { } selectedIdea } viewModel)
        {
            return;
        }

        var fileDialog = new SaveFileDialog
        {
            Title = "Export game design document",
            Filter = "PDF document (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = CreateSafeFileName($"{selectedIdea.Name}-GDD.pdf")
        };
        if (fileDialog.ShowDialog(this) == true)
        {
            await viewModel.ExportSelectedIdeaPdfAsync(fileDialog.FileName);
        }
    }

    private async void EditIdea_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.LoadSelectedIdeaForEditingAsync();
        }
    }

    private async void DeleteIdea_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel { SelectedSavedIdea: { } selectedIdea } viewModel)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Delete {selectedIdea.Name}? This also removes its managed image and audio files.",
            "Delete game idea",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (result == MessageBoxResult.Yes)
        {
            await viewModel.DeleteSelectedIdeaAsync();
        }
    }

    private async void ExportAllIdeasWorkbook_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var fileDialog = new SaveFileDialog
        {
            Title = "Export all game ideas",
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            DefaultExt = ".xlsx",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = "GameDesignWizard-Idea-Pool.xlsx"
        };
        if (fileDialog.ShowDialog(this) == true)
        {
            await viewModel.ExportAllIdeasWorkbookAsync(fileDialog.FileName);
        }
    }

    private async void ExportFilteredIdeasWorkbook_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var fileDialog = new SaveFileDialog
        {
            Title = "Export filtered game ideas",
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            DefaultExt = ".xlsx",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = "GameDesignWizard-Filtered-Ideas.xlsx"
        };
        if (fileDialog.ShowDialog(this) == true)
        {
            await viewModel.ExportFilteredIdeasWorkbookAsync(fileDialog.FileName);
        }
    }

    private static string CreateSafeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeName = new string(value.Select(character =>
            invalidCharacters.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(safeName) ? "Game-Idea-GDD.pdf" : safeName;
    }
}
