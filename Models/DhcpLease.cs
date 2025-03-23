using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a DHCP lease from a MikroTik router
    /// </summary>
    public class DhcpLease : INotifyPropertyChanged
    {
        private string _id;
        private string _address;
        private string _macAddress;
        private string _hostname;
        private string _comment;
        private bool _dynamic;
        private bool _active;
        private bool _blocked;
        private DateTime _lastSeen;
        private string _server;
        private DateTime _expiresAt;

        /// <summary>
        /// Gets or sets the unique ID of the lease
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the IP address of the lease
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// Gets or sets the MAC address of the client
        /// </summary>
        public string MacAddress
        {
            get => _macAddress;
            set => SetProperty(ref _macAddress, value);
        }

        /// <summary>
        /// Gets or sets the hostname of the client
        /// </summary>
        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        /// <summary>
        /// Gets or sets the comment for the lease
        /// </summary>
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        /// <summary>
        /// Gets or sets whether the lease is dynamic
        /// </summary>
        public bool Dynamic
        {
            get => _dynamic;
            set => SetProperty(ref _dynamic, value);
        }

        /// <summary>
        /// Gets or sets whether the lease is active
        /// </summary>
        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        /// <summary>
        /// Gets or sets whether the lease is blocked
        /// </summary>
        public bool Blocked
        {
            get => _blocked;
            set => SetProperty(ref _blocked, value);
        }

        /// <summary>
        /// Gets or sets when the client was last seen
        /// </summary>
        public DateTime LastSeen
        {
            get => _lastSeen;
            set => SetProperty(ref _lastSeen, value);
        }

        /// <summary>
        /// Gets or sets the DHCP server that issued the lease
        /// </summary>
        public string Server
        {
            get => _server;
            set => SetProperty(ref _server, value);
        }

        /// <summary>
        /// Gets or sets when the lease expires
        /// </summary>
        public DateTime ExpiresAt
        {
            get => _expiresAt;
            set => SetProperty(ref _expiresAt, value);
        }

        /// <summary>
        /// Gets the status of the lease
        /// </summary>
        public string Status
        {
            get
            {
                if (Blocked)
                    return "Blocked";
                if (Active)
                    return "Active";
                return "Inactive";
            }
        }

        /// <summary>
        /// Gets the time remaining before the lease expires
        /// </summary>
        public TimeSpan TimeRemaining => ExpiresAt - DateTime.Now;

        /// <summary>
        /// Gets the time remaining as a formatted string
        /// </summary>
        public string TimeRemainingFormatted
        {
            get
            {
                if (ExpiresAt == DateTime.MinValue)
                    return "N/A";
                    
                TimeSpan remaining = TimeRemaining;
                
                if (remaining.TotalSeconds <= 0)
                    return "Expired";
                    
                if (remaining.TotalDays > 1)
                    return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
                    
                return $"{remaining.Hours}h {remaining.Minutes}m";
            }
        }

        /// <summary>
        /// Gets the display name for the lease
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Hostname))
                    return Hostname;
                    
                if (!string.IsNullOrEmpty(Comment))
                    return Comment;
                    
                return MacAddress;
            }
        }

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
            if (propertyName == nameof(Active) || propertyName == nameof(Blocked))
                OnPropertyChanged(nameof(Status));
                
            if (propertyName == nameof(ExpiresAt))
            {
                OnPropertyChanged(nameof(TimeRemaining));
                OnPropertyChanged(nameof(TimeRemainingFormatted));
            }
                
            if (propertyName == nameof(Hostname) || propertyName == nameof(Comment) || propertyName == nameof(MacAddress))
                OnPropertyChanged(nameof(DisplayName));
            
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
}