using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents resource usage information for a router
    /// </summary>
    public class ResourceUsage : INotifyPropertyChanged
    {
        private const int MAX_HISTORY_POINTS = 300; // 5 minutes at 1 second intervals
        
        private double _cpuUsage;
        private double _memoryUsage;
        private double _diskUsage;
        private DateTime _lastUpdated;
        
        // History for charts
        private readonly Queue<DataPoint> _cpuHistory = new Queue<DataPoint>();
        private readonly Queue<DataPoint> _memoryHistory = new Queue<DataPoint>();
        
        /// <summary>
        /// Gets or sets the CPU usage percentage
        /// </summary>
        public double CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }
        
        /// <summary>
        /// Gets or sets the memory usage percentage
        /// </summary>
        public double MemoryUsage
        {
            get => _memoryUsage;
            set => SetProperty(ref _memoryUsage, value);
        }
        
        /// <summary>
        /// Gets or sets the disk usage percentage
        /// </summary>
        public double DiskUsage
        {
            get => _diskUsage;
            set => SetProperty(ref _diskUsage, value);
        }
        
        /// <summary>
        /// Gets or sets when the usage was last updated
        /// </summary>
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }
        
        /// <summary>
        /// Gets the CPU usage history
        /// </summary>
        public ReadOnlyCollection<DataPoint> CpuHistory => new ReadOnlyCollection<DataPoint>(_cpuHistory.ToArray());
        
        /// <summary>
        /// Gets the memory usage history
        /// </summary>
        public ReadOnlyCollection<DataPoint> MemoryHistory => new ReadOnlyCollection<DataPoint>(_memoryHistory.ToArray());
        
        /// <summary>
        /// Adds a CPU usage data point to the history
        /// </summary>
        /// <param name="cpuUsage">The CPU usage percentage</param>
        public void AddCpuUsage(double cpuUsage)
        {
            _cpuHistory.Enqueue(new DataPoint(DateTime.Now, cpuUsage));
            
            // Trim history to maximum size
            while (_cpuHistory.Count > MAX_HISTORY_POINTS)
                _cpuHistory.Dequeue();
                
            OnPropertyChanged(nameof(CpuHistory));
        }
        
        /// <summary>
        /// Adds a memory usage data point to the history
        /// </summary>
        /// <param name="memoryUsage">The memory usage percentage</param>
        public void AddMemoryUsage(double memoryUsage)
        {
            _memoryHistory.Enqueue(new DataPoint(DateTime.Now, memoryUsage));
            
            // Trim history to maximum size
            while (_memoryHistory.Count > MAX_HISTORY_POINTS)
                _memoryHistory.Dequeue();
                
            OnPropertyChanged(nameof(MemoryHistory));
        }
        
        /// <summary>
        /// Event that is fired when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Set a property value and raise the PropertyChanged event if the value changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">The backing field for the property</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the value was changed, otherwise false</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;
                
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        /// <summary>
        /// Raise the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}