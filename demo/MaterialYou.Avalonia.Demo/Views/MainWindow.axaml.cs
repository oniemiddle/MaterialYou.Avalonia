using Avalonia.Controls;
using MaterialYou.Avalonia.Demo.ViewModels;

namespace MaterialYou.Avalonia.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
