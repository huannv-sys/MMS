using System;
using System.Collections.Generic;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    /// <summary>
    /// Service for processing and analyzing statistics data
    /// </summary>
    public class StatisticsService
    {
        private readonly Dictionary<string, DateTime> _lastInterfaceUpdateTimes = new Dictionary<string, DateTime>();
        
        /// <summary>
        /// Update resource statistics for a router
        /// </summary>
        /// <param name="router">The router to update statistics for</param>
        public void UpdateResourceStatistics(RouterDevice router)
        {
            if (router == null || router.ResourceUsage == null)
                return;
            
            // Update last update time
            router.ResourceUsage.LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Update interface statistics for a router
        /// </summary>
        /// <param name="router">The router to update statistics for</param>
        public void UpdateInterfaceStatistics(RouterDevice router)
        {
            if (router == null || router.Interfaces == null)
                return;
            
            // Process each interface
            foreach (var iface in router.Interfaces)
            {
                string ifaceKey = $"{router.Id}_{iface.Id}";
                
                // Calculate bandwidth
                if (_lastInterfaceUpdateTimes.TryGetValue(ifaceKey, out DateTime lastUpdate))
                {
                    TimeSpan elapsed = DateTime.Now - lastUpdate;
                    double seconds = elapsed.TotalSeconds;
                    
                    if (seconds > 0)
                    {
                        // Calculate RX bandwidth
                        if (iface.RxBytes > iface.PreviousRxBytes)
                        {
                            iface.RxBytesPerSecond = (iface.RxBytes - iface.PreviousRxBytes) / seconds;
                        }
                        
                        // Calculate TX bandwidth
                        if (iface.TxBytes > iface.PreviousTxBytes)
                        {
                            iface.TxBytesPerSecond = (iface.TxBytes - iface.PreviousTxBytes) / seconds;
                        }
                    }
                }
                
                // Store current values for next calculation
                iface.PreviousRxBytes = iface.RxBytes;
                iface.PreviousTxBytes = iface.TxBytes;
                
                // Update last update time
                _lastInterfaceUpdateTimes[ifaceKey] = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Update traffic charts for a router
        /// </summary>
        /// <param name="router">The router to update traffic charts for</param>
        public void UpdateTrafficCharts(RouterDevice router)
        {
            if (router == null || router.Interfaces == null)
                return;
            
            // Calculate total traffic
            double totalRxBps = 0;
            double totalTxBps = 0;
            
            foreach (var iface in router.Interfaces)
            {
                // Only count running interfaces
                if (iface.IsRunning)
                {
                    totalRxBps += iface.RxBytesPerSecond;
                    totalTxBps += iface.TxBytesPerSecond;
                }
            }
            
            // TODO: Update traffic charts with totalRxBps and totalTxBps
        }
        
        /// <summary>
        /// Generate a bandwidth snapshot for a specific time range
        /// </summary>
        /// <param name="router">The router to generate the snapshot for</param>
        /// <param name="interfaceId">The ID of the interface, or null for all interfaces</param>
        /// <param name="startTime">The start time of the range</param>
        /// <param name="endTime">The end time of the range</param>
        /// <returns>A bandwidth snapshot object</returns>
        public BandwidthSnapshot GenerateBandwidthSnapshot(RouterDevice router, string interfaceId, DateTime startTime, DateTime endTime)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
            
            // Create snapshot
            var snapshot = new BandwidthSnapshot
            {
                RouterId = router.Id,
                RouterName = router.Name,
                InterfaceId = interfaceId,
                StartTime = startTime,
                EndTime = endTime
            };
            
            // TODO: Generate bandwidth data points
            
            return snapshot;
        }
    }
    
    /// <summary>
    /// Represents a bandwidth snapshot for a specific time range
    /// </summary>
    public class BandwidthSnapshot
    {
        /// <summary>
        /// Gets or sets the router ID
        /// </summary>
        public string RouterId { get; set; }
        
        /// <summary>
        /// Gets or sets the router name
        /// </summary>
        public string RouterName { get; set; }
        
        /// <summary>
        /// Gets or sets the interface ID, or null for all interfaces
        /// </summary>
        public string InterfaceId { get; set; }
        
        /// <summary>
        /// Gets or sets the interface name
        /// </summary>
        public string InterfaceName { get; set; }
        
        /// <summary>
        /// Gets or sets the start time of the range
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the range
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the RX data points
        /// </summary>
        public List<DataPoint> RxDataPoints { get; set; } = new List<DataPoint>();
        
        /// <summary>
        /// Gets or sets the TX data points
        /// </summary>
        public List<DataPoint> TxDataPoints { get; set; } = new List<DataPoint>();
        
        /// <summary>
        /// Gets the average RX bytes per second
        /// </summary>
        public double AverageRxBytesPerSecond => CalculateAverage(RxDataPoints);
        
        /// <summary>
        /// Gets the average TX bytes per second
        /// </summary>
        public double AverageTxBytesPerSecond => CalculateAverage(TxDataPoints);
        
        /// <summary>
        /// Gets the maximum RX bytes per second
        /// </summary>
        public double MaxRxBytesPerSecond => CalculateMax(RxDataPoints);
        
        /// <summary>
        /// Gets the maximum TX bytes per second
        /// </summary>
        public double MaxTxBytesPerSecond => CalculateMax(TxDataPoints);
        
        /// <summary>
        /// Calculate the average value from a list of data points
        /// </summary>
        /// <param name="dataPoints">The list of data points</param>
        /// <returns>The average value</returns>
        private double CalculateAverage(List<DataPoint> dataPoints)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return 0;
                
            double sum = 0;
            foreach (var point in dataPoints)
            {
                sum += point.Value;
            }
            
            return sum / dataPoints.Count;
        }
        
        /// <summary>
        /// Calculate the maximum value from a list of data points
        /// </summary>
        /// <param name="dataPoints">The list of data points</param>
        /// <returns>The maximum value</returns>
        private double CalculateMax(List<DataPoint> dataPoints)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return 0;
                
            double max = double.MinValue;
            foreach (var point in dataPoints)
            {
                if (point.Value > max)
                    max = point.Value;
            }
            
            return max;
        }
    }
}