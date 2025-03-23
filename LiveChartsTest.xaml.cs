using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Collections.ObjectModel;

namespace MikroTikMonitor
{
    public partial class LiveChartsTest : Window
    {
        public LiveChartsTest()
        {
            InitializeComponent();
            DataContext = this;
            
            // Initialize test series
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 1, 2, 3, 4, 5 },
                    Name = "Test Series"
                }
            };
            
            // Initialize axes
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "X Axis",
                    NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
                }
            };
            
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Y Axis",
                    NamePadding = new LiveChartsCore.Drawing.Padding(15, 0)
                }
            };
        }
        
        public ObservableCollection<ISeries> Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
    }
}