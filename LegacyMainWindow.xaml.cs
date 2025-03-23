using System.Windows;
using MikroTikMonitor.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;

namespace MikroTikMonitor
{
    /// <summary>
    /// Interaction logic for LegacyMainWindow.xaml
    /// </summary>
    public partial class LegacyMainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Gets the view model for this window
        /// </summary>
        public MainViewModel ViewModel => _viewModel;

        /// <summary>
        /// Gets the formatter for percent values
        /// </summary>
        public Func<double, string> PercentFormatter => value => $"{value:0.0}%";

        /// <summary>
        /// Gets the formatter for speed values
        /// </summary>
        public Func<double, string> SpeedFormatter => FormatSpeed;

        /// <summary>
        /// Initializes a new instance of the LegacyMainWindow class
        /// </summary>
        /// <param name="viewModel">The view model for this window</param>
        public LegacyMainWindow(MainViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Format a speed value to a readable string
        /// </summary>
        /// <param name="bytesPerSecond">The speed in bytes per second</param>
        /// <returns>A formatted string</returns>
        private string FormatSpeed(double bytesPerSecond)
        {
            double bitsPerSecond = bytesPerSecond * 8;

            if (bitsPerSecond < 1000)
                return $"{bitsPerSecond:0.0} bps";

            if (bitsPerSecond < 1000000)
                return $"{bitsPerSecond / 1000:0.0} kbps";

            if (bitsPerSecond < 1000000000)
                return $"{bitsPerSecond / 1000000:0.0} Mbps";

            return $"{bitsPerSecond / 1000000000:0.0} Gbps";
        }
    }
}