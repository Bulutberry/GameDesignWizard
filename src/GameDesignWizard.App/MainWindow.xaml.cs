using System.Windows;
using GameDesignWizard.App.ViewModels;

namespace GameDesignWizard.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
