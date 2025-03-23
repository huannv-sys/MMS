using System;
using System.Windows;
using MikroTikMonitor.ViewModels;

namespace MikroTikMonitor.Windows
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        private async void MainWindow_Closed(object sender, EventArgs e)
        {
            await _viewModel.CleanupAsync();
        }
    }
}