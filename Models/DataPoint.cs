using System;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a data point for charts
    /// </summary>
    public struct DataPoint
    {
        /// <summary>
        /// Gets the time of the data point
        /// </summary>
        public DateTime Time { get; }
        
        /// <summary>
        /// Gets the value of the data point
        /// </summary>
        public double Value { get; }
        
        /// <summary>
        /// Initializes a new instance of the DataPoint struct
        /// </summary>
        /// <param name="time">The time of the data point</param>
        /// <param name="value">The value of the data point</param>
        public DataPoint(DateTime time, double value)
        {
            Time = time;
            Value = value;
        }
    }
}