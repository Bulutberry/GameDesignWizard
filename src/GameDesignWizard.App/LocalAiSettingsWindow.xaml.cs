using System.IO;
using System.Windows;
using GameDesignWizard.App.ViewModels;
using GameDesignWizard.Core.Ai;
using Microsoft.Win32;

namespace GameDesignWizard.App;

public partial class LocalAiSettingsWindow : Window
{
    private readonly LocalAiSettingsViewModel _viewModel;

    public LocalAiSettingsWindow(ILocalAiSettingsStore settingsStore)
    {
        InitializeComponent();
        _viewModel = new LocalAiSettingsViewModel(settingsStore);
        DataContext = _viewModel;
        Loaded += LoadSettingsAsync;
    }

    private async void LoadSettingsAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Local AI setup", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BrowseRuntime_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose the llama.cpp server executable",
            Filter = "Windows executable (*.exe)|*.exe",
            CheckFileExists = true,
            Multiselect = false,
            FileName = "llama-server.exe"
        };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.RuntimePath = dialog.FileName;
        }
    }

    private void BrowseModel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a GGUF language model",
            Filter = "GGUF model (*.gguf)|*.gguf",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ModelPath = dialog.FileName;
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.SaveAsync();
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(exception.Message, "Local AI setup", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
