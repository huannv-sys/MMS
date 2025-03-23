using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace MikroTikMonitor.ViewModels
{
    public class ChartViewModel : ViewModelBase
    {
        private ObservableCollection<ISeries> _cpuSeries;
        private ObservableCollection<ISeries> _memoryUsageSeries;
        private ObservableCollection<ISeries> _rxSeries;
        private ObservableCollection<ISeries> _txSeries;
        private Axis[] _timeAxis;
        private Axis[] _percentAxis;
        private Axis[] _speedAxis;

        public ChartViewModel()
        {
            // Initialize CPU Series
            _cpuSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "CPU",
                    Values = new ObservableCollection<ObservablePoint>(),
                    Stroke = new SolidColorPaint(SKColors.HotPink) { StrokeThickness = 2 },
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            // Initialize Memory Usage Series
            _memoryUsageSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "Memory",
                    Values = new ObservableCollection<ObservablePoint>(),
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            // Initialize Rx Series
            _rxSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "Rx",
                    Values = new ObservableCollection<ObservablePoint>(),
                    Stroke = new SolidColorPaint(SKColors.LimeGreen) { StrokeThickness = 2 },
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            // Initialize Tx Series
            _txSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "Tx",
                    Values = new ObservableCollection<ObservablePoint>(),
                    Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            };

            // Initialize Time Axis
            _timeAxis = new Axis[]
            {
                new Axis
                {
                    Name = "Time",
                    ShowLabels = false,
                    IsVisible = false
                }
            };

            // Initialize Percent Axis
            _percentAxis = new Axis[]
            {
                new Axis
                {
                    Name = "Percent",
                    MinLimit = 0,
                    MaxLimit = 100,
                    LabelFormatter = (value) => $"{value}%",
                }
            };

            // Initialize Speed Axis
            _speedAxis = new Axis[]
            {
                new Axis
                {
                    Name = "Speed",
                    LabelFormatter = (value) => FormattedSpeed(value),
                }
            };
        }

        public ObservableCollection<ISeries> CpuSeries
        {
            get => _cpuSeries;
            set => SetProperty(ref _cpuSeries, value);
        }

        public ObservableCollection<ISeries> MemoryUsageSeries
        {
            get => _memoryUsageSeries;
            set => SetProperty(ref _memoryUsageSeries, value);
        }

        public ObservableCollection<ISeries> RxSeries
        {
            get => _rxSeries;
            set => SetProperty(ref _rxSeries, value);
        }

        public ObservableCollection<ISeries> TxSeries
        {
            get => _txSeries;
            set => SetProperty(ref _txSeries, value);
        }

        public Axis[] TimeAxis
        {
            get => _timeAxis;
            set => SetProperty(ref _timeAxis, value);
        }

        public Axis[] PercentAxis
        {
            get => _percentAxis;
            set => SetProperty(ref _percentAxis, value);
        }

        public Axis[] SpeedAxis
        {
            get => _speedAxis;
            set => SetProperty(ref _speedAxis, value);
        }

        // Helper method to format speed values
        private string FormattedSpeed(double value)
        {
            string[] sizes = { "bps", "Kbps", "Mbps", "Gbps" };
            int order = 0;
            double speed = value;
            
            while (speed >= 1000 && order < sizes.Length - 1)
            {
                order++;
                speed = speed / 1000;
            }
            
            return $"{speed:0.##} {sizes[order]}";
        }

        // Update CPU chart data
        public void UpdateCpuData(double timestamp, double value)
        {
            var cpuValues = ((LineSeries<ObservablePoint>)CpuSeries[0]).Values as ObservableCollection<ObservablePoint>;
            cpuValues.Add(new ObservablePoint(timestamp, value));
            
            // Keep only last 60 points to prevent memory issues
            if (cpuValues.Count > 60)
            {
                cpuValues.RemoveAt(0);
            }
        }

        // Update Memory chart data
        public void UpdateMemoryData(double timestamp, double value)
        {
            var memoryValues = ((LineSeries<ObservablePoint>)MemoryUsageSeries[0]).Values as ObservableCollection<ObservablePoint>;
            memoryValues.Add(new ObservablePoint(timestamp, value));
            
            // Keep only last 60 points to prevent memory issues
            if (memoryValues.Count > 60)
            {
                memoryValues.RemoveAt(0);
            }
        }

        // Update Rx chart data
        public void UpdateRxData(double timestamp, double value)
        {
            var rxValues = ((LineSeries<ObservablePoint>)RxSeries[0]).Values as ObservableCollection<ObservablePoint>;
            rxValues.Add(new ObservablePoint(timestamp, value));
            
            // Keep only last 60 points to prevent memory issues
            if (rxValues.Count > 60)
            {
                rxValues.RemoveAt(0);
            }
        }

        // Update Tx chart data
        public void UpdateTxData(double timestamp, double value)
        {
            var txValues = ((LineSeries<ObservablePoint>)TxSeries[0]).Values as ObservableCollection<ObservablePoint>;
            txValues.Add(new ObservablePoint(timestamp, value));
            
            // Keep only last 60 points to prevent memory issues
            if (txValues.Count > 60)
            {
                txValues.RemoveAt(0);
            }
        }
        
        // Clear all chart data
        public void ClearChartData()
        {
            // Clear CPU data
            var cpuValues = ((LineSeries<ObservablePoint>)CpuSeries[0]).Values as ObservableCollection<ObservablePoint>;
            cpuValues.Clear();
            
            // Clear memory data
            var memoryValues = ((LineSeries<ObservablePoint>)MemoryUsageSeries[0]).Values as ObservableCollection<ObservablePoint>;
            memoryValues.Clear();
            
            // Clear Rx data
            var rxValues = ((LineSeries<ObservablePoint>)RxSeries[0]).Values as ObservableCollection<ObservablePoint>;
            rxValues.Clear();
            
            // Clear Tx data
            var txValues = ((LineSeries<ObservablePoint>)TxSeries[0]).Values as ObservableCollection<ObservablePoint>;
            txValues.Clear();
        }

        // INotifyPropertyChanged implementation is now handled by ViewModelBase
    }
}