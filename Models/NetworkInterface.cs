using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a network interface on a MikroTik router
    /// </summary>
    public class NetworkInterface : INotifyPropertyChanged
    {
        private const int MAX_HISTORY_POINTS = 300; // 5 minutes at 1 second intervals
        
        private string _id;
        private string _name;
        private string _type;
        private string _macAddress;
        private bool _isEnabled;
        private bool _isRunning;
        private string _status;
        private string _comment;
        private string _defaultName;
        private int _mtu;
        private string _ipAddressCidr;
        private long _rxBytes;
        private long _txBytes;
        private long _rxPackets;
        private long _txPackets;
        private long _rxErrors;
        private long _txErrors;
        private long _rxDrops;
        private long _txDrops;
        private long _previousRxBytes;
        private long _previousTxBytes;
        private double _rxBytesPerSecond;
        private double _txBytesPerSecond;
        
        // History for charts
        private readonly Queue<DataPoint> _rxHistory = new Queue<DataPoint>();
        private readonly Queue<DataPoint> _txHistory = new Queue<DataPoint>();
        
        /// <summary>
        /// Gets or sets the ID of the interface
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        
        /// <summary>
        /// Gets or sets the name of the interface
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        /// <summary>
        /// Gets or sets the type of the interface
        /// </summary>
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        
        /// <summary>
        /// Gets or sets the MAC address of the interface
        /// </summary>
        public string MacAddress
        {
            get => _macAddress;
            set => SetProperty(ref _macAddress, value);
        }
        
        /// <summary>
        /// Gets or sets whether the interface is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        
        /// <summary>
        /// Gets or sets whether the interface is running
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }
        
        /// <summary>
        /// Gets or sets the status of the interface
        /// </summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        
        /// <summary>
        /// Gets or sets the comment for the interface
        /// </summary>
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }
        
        /// <summary>
        /// Gets or sets the default name of the interface
        /// </summary>
        public string DefaultName
        {
            get => _defaultName;
            set => SetProperty(ref _defaultName, value);
        }
        
        /// <summary>
        /// Gets or sets the MTU of the interface
        /// </summary>
        public int Mtu
        {
            get => _mtu;
            set => SetProperty(ref _mtu, value);
        }
        
        /// <summary>
        /// Gets or sets the IP address of the interface in CIDR notation
        /// </summary>
        public string IpAddressCidr
        {
            get => _ipAddressCidr;
            set
            {
                if (SetProperty(ref _ipAddressCidr, value))
                {
                    OnPropertyChanged(nameof(IpAddress));
                    OnPropertyChanged(nameof(SubnetMask));
                }
            }
        }
        
        /// <summary>
        /// Gets the IP address of the interface
        /// </summary>
        public string IpAddress
        {
            get
            {
                if (string.IsNullOrEmpty(IpAddressCidr))
                    return string.Empty;
                    
                return IpAddressCidr.Split('/')[0];
            }
        }
        
        /// <summary>
        /// Gets the subnet mask of the interface
        /// </summary>
        public string SubnetMask
        {
            get
            {
                if (string.IsNullOrEmpty(IpAddressCidr))
                    return string.Empty;
                    
                string[] parts = IpAddressCidr.Split('/');
                if (parts.Length != 2)
                    return string.Empty;
                    
                if (!int.TryParse(parts[1], out int prefix))
                    return string.Empty;
                    
                return CidrToSubnetMask(prefix);
            }
        }
        
        /// <summary>
        /// Gets or sets the received bytes
        /// </summary>
        public long RxBytes
        {
            get => _rxBytes;
            set => SetProperty(ref _rxBytes, value);
        }
        
        /// <summary>
        /// Gets or sets the transmitted bytes
        /// </summary>
        public long TxBytes
        {
            get => _txBytes;
            set => SetProperty(ref _txBytes, value);
        }
        
        /// <summary>
        /// Gets or sets the received packets
        /// </summary>
        public long RxPackets
        {
            get => _rxPackets;
            set => SetProperty(ref _rxPackets, value);
        }
        
        /// <summary>
        /// Gets or sets the transmitted packets
        /// </summary>
        public long TxPackets
        {
            get => _txPackets;
            set => SetProperty(ref _txPackets, value);
        }
        
        /// <summary>
        /// Gets or sets the received errors
        /// </summary>
        public long RxErrors
        {
            get => _rxErrors;
            set => SetProperty(ref _rxErrors, value);
        }
        
        /// <summary>
        /// Gets or sets the transmitted errors
        /// </summary>
        public long TxErrors
        {
            get => _txErrors;
            set => SetProperty(ref _txErrors, value);
        }
        
        /// <summary>
        /// Gets or sets the received drops
        /// </summary>
        public long RxDrops
        {
            get => _rxDrops;
            set => SetProperty(ref _rxDrops, value);
        }
        
        /// <summary>
        /// Gets or sets the transmitted drops
        /// </summary>
        public long TxDrops
        {
            get => _txDrops;
            set => SetProperty(ref _txDrops, value);
        }
        
        /// <summary>
        /// Gets or sets the previous received bytes
        /// </summary>
        public long PreviousRxBytes
        {
            get => _previousRxBytes;
            set => SetProperty(ref _previousRxBytes, value);
        }
        
        /// <summary>
        /// Gets or sets the previous transmitted bytes
        /// </summary>
        public long PreviousTxBytes
        {
            get => _previousTxBytes;
            set => SetProperty(ref _previousTxBytes, value);
        }
        
        /// <summary>
        /// Gets or sets the received bytes per second
        /// </summary>
        public double RxBytesPerSecond
        {
            get => _rxBytesPerSecond;
            set
            {
                if (SetProperty(ref _rxBytesPerSecond, value))
                {
                    // Add to history
                    AddToRxHistory(new DataPoint(DateTime.Now, value));
                    
                    // Update derived properties
                    OnPropertyChanged(nameof(RxKilobitsPerSecond));
                    OnPropertyChanged(nameof(RxMegabitsPerSecond));
                    OnPropertyChanged(nameof(RxSpeedFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the transmitted bytes per second
        /// </summary>
        public double TxBytesPerSecond
        {
            get => _txBytesPerSecond;
            set
            {
                if (SetProperty(ref _txBytesPerSecond, value))
                {
                    // Add to history
                    AddToTxHistory(new DataPoint(DateTime.Now, value));
                    
                    // Update derived properties
                    OnPropertyChanged(nameof(TxKilobitsPerSecond));
                    OnPropertyChanged(nameof(TxMegabitsPerSecond));
                    OnPropertyChanged(nameof(TxSpeedFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets the received kilobits per second
        /// </summary>
        public double RxKilobitsPerSecond => RxBytesPerSecond * 8 / 1000;
        
        /// <summary>
        /// Gets the transmitted kilobits per second
        /// </summary>
        public double TxKilobitsPerSecond => TxBytesPerSecond * 8 / 1000;
        
        /// <summary>
        /// Gets the received megabits per second
        /// </summary>
        public double RxMegabitsPerSecond => RxKilobitsPerSecond / 1000;
        
        /// <summary>
        /// Gets the transmitted megabits per second
        /// </summary>
        public double TxMegabitsPerSecond => TxKilobitsPerSecond / 1000;
        
        /// <summary>
        /// Gets the formatted received speed
        /// </summary>
        public string RxSpeedFormatted => FormatSpeed(RxBytesPerSecond);
        
        /// <summary>
        /// Gets the formatted transmitted speed
        /// </summary>
        public string TxSpeedFormatted => FormatSpeed(TxBytesPerSecond);
        
        /// <summary>
        /// Gets the RX history for charts
        /// </summary>
        public IEnumerable<DataPoint> RxHistory => _rxHistory;
        
        /// <summary>
        /// Gets the TX history for charts
        /// </summary>
        public IEnumerable<DataPoint> TxHistory => _txHistory;
        
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
        
        /// <summary>
        /// Convert a CIDR prefix to a subnet mask
        /// </summary>
        /// <param name="prefix">The CIDR prefix</param>
        /// <returns>The subnet mask as a string</returns>
        private string CidrToSubnetMask(int prefix)
        {
            // Convert CIDR prefix to subnet mask
            uint mask = 0xffffffff;
            mask <<= (32 - prefix);
            
            byte[] bytes = BitConverter.GetBytes(mask);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
                
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
        }
        
        /// <summary>
        /// Format a speed value in bytes per second to a human-readable string
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
        
        /// <summary>
        /// Add a data point to the RX history
        /// </summary>
        /// <param name="point">The data point to add</param>
        private void AddToRxHistory(DataPoint point)
        {
            _rxHistory.Enqueue(point);
            
            // Trim history to maximum size
            while (_rxHistory.Count > MAX_HISTORY_POINTS)
                _rxHistory.Dequeue();
                
            OnPropertyChanged(nameof(RxHistory));
        }
        
        /// <summary>
        /// Add a data point to the TX history
        /// </summary>
        /// <param name="point">The data point to add</param>
        private void AddToTxHistory(DataPoint point)
        {
            _txHistory.Enqueue(point);
            
            // Trim history to maximum size
            while (_txHistory.Count > MAX_HISTORY_POINTS)
                _txHistory.Dequeue();
                
            OnPropertyChanged(nameof(TxHistory));
        }
    }
}