using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a log entry from a MikroTik router
    /// </summary>
    public class LogEntry : INotifyPropertyChanged
    {
        private string _id;
        private DateTime _time;
        private string _topics;
        private string _message;
        private LogSeverity _severity;

        /// <summary>
        /// Gets or sets the unique ID of the log entry
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the time of the log entry
        /// </summary>
        public DateTime Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        /// <summary>
        /// Gets or sets the topics of the log entry
        /// </summary>
        public string Topics
        {
            get => _topics;
            set => SetProperty(ref _topics, value);
        }

        /// <summary>
        /// Gets or sets the message of the log entry
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// Gets or sets the severity of the log entry
        /// </summary>
        public LogSeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        /// <summary>
        /// Gets the time of the log entry as a formatted string
        /// </summary>
        public string TimeFormatted => Time.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// Gets the severity of the log entry as a string
        /// </summary>
        public string SeverityText => Severity.ToString();

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">Reference to the backing field</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the value was changed, otherwise false</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            
            // Notify when dependent properties might have changed
            if (propertyName == nameof(Time))
                OnPropertyChanged(nameof(TimeFormatted));
                
            if (propertyName == nameof(Severity))
                OnPropertyChanged(nameof(SeverityText));
            
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents the severity of a log entry
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Debug level severity
        /// </summary>
        Debug,
        
        /// <summary>
        /// Information level severity
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning level severity
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error level severity
        /// </summary>
        Error,
        
        /// <summary>
        /// Critical level severity
        /// </summary>
        Critical
    }
}