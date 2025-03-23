using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class ResourceUsage : INotifyPropertyChanged
    {
        private DateTime _timestamp;
        private double _cpuUsage;
        private double _memoryUsage;
        private double _diskUsage;
        private double _temperature;
        private int _voltage;

        public DateTime Timestamp 
        { 
            get => _timestamp; 
            set => SetProperty(ref _timestamp, value); 
        }
        
        public double CpuUsage 
        { 
            get => _cpuUsage; 
            set => SetProperty(ref _cpuUsage, value); 
        }
        
        public double MemoryUsage 
        { 
            get => _memoryUsage; 
            set => SetProperty(ref _memoryUsage, value); 
        }
        
        public double DiskUsage 
        { 
            get => _diskUsage; 
            set => SetProperty(ref _diskUsage, value); 
        }
        
        public double Temperature 
        { 
            get => _temperature; 
            set => SetProperty(ref _temperature, value); 
        }
        
        public int Voltage 
        { 
            get => _voltage; 
            set => SetProperty(ref _voltage, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class NetworkInterface : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _type;
        private string _macAddress;
        private bool _isEnabled;
        private bool _isUp;
        private string _ipAddress;
        private string _subnetMask;
        private string _defaultGateway;
        private long _rxBytes;
        private long _txBytes;
        private long _rxPackets;
        private long _txPackets;
        private long _rxErrors;
        private long _txErrors;
        private long _rxDrops;
        private long _txDrops;
        private ObservableCollection<InterfaceTraffic> _trafficHistory = new ObservableCollection<InterfaceTraffic>();
        private DateTime _lastUpdated;
        private long _speed;
        private bool _isDynamic;
        private string _comment;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value); 
        }
        
        public string Type 
        { 
            get => _type; 
            set => SetProperty(ref _type, value); 
        }
        
        public string MacAddress 
        { 
            get => _macAddress; 
            set => SetProperty(ref _macAddress, value); 
        }
        
        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => SetProperty(ref _isEnabled, value); 
        }
        
        public bool IsUp 
        { 
            get => _isUp; 
            set => SetProperty(ref _isUp, value); 
        }
        
        public string IpAddress 
        { 
            get => _ipAddress; 
            set => SetProperty(ref _ipAddress, value); 
        }
        
        public string SubnetMask 
        { 
            get => _subnetMask; 
            set => SetProperty(ref _subnetMask, value); 
        }
        
        public string DefaultGateway 
        { 
            get => _defaultGateway; 
            set => SetProperty(ref _defaultGateway, value); 
        }
        
        public long RxBytes 
        { 
            get => _rxBytes; 
            set => SetProperty(ref _rxBytes, value); 
        }
        
        public long TxBytes 
        { 
            get => _txBytes; 
            set => SetProperty(ref _txBytes, value); 
        }
        
        public long RxPackets 
        { 
            get => _rxPackets; 
            set => SetProperty(ref _rxPackets, value); 
        }
        
        public long TxPackets 
        { 
            get => _txPackets; 
            set => SetProperty(ref _txPackets, value); 
        }
        
        public long RxErrors 
        { 
            get => _rxErrors; 
            set => SetProperty(ref _rxErrors, value); 
        }
        
        public long TxErrors 
        { 
            get => _txErrors; 
            set => SetProperty(ref _txErrors, value); 
        }
        
        public long RxDrops 
        { 
            get => _rxDrops; 
            set => SetProperty(ref _rxDrops, value); 
        }
        
        public long TxDrops 
        { 
            get => _txDrops; 
            set => SetProperty(ref _txDrops, value); 
        }
        
        public ObservableCollection<InterfaceTraffic> TrafficHistory 
        { 
            get => _trafficHistory; 
            set => SetProperty(ref _trafficHistory, value); 
        }
        
        public DateTime LastUpdated 
        { 
            get => _lastUpdated; 
            set => SetProperty(ref _lastUpdated, value); 
        }
        
        public long Speed 
        { 
            get => _speed; 
            set => SetProperty(ref _speed, value); 
        }
        
        public bool IsDynamic 
        { 
            get => _isDynamic; 
            set => SetProperty(ref _isDynamic, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class InterfaceTraffic : INotifyPropertyChanged
    {
        private DateTime _timestamp;
        private long _rxBytes;
        private long _txBytes;
        private long _rxPackets;
        private long _txPackets;
        private long _rxErrors;
        private long _txErrors;
        private long _rxDrops;
        private long _txDrops;

        public DateTime Timestamp 
        { 
            get => _timestamp; 
            set => SetProperty(ref _timestamp, value); 
        }
        
        public long RxBytes 
        { 
            get => _rxBytes; 
            set => SetProperty(ref _rxBytes, value); 
        }
        
        public long TxBytes 
        { 
            get => _txBytes; 
            set => SetProperty(ref _txBytes, value); 
        }
        
        public long RxPackets 
        { 
            get => _rxPackets; 
            set => SetProperty(ref _rxPackets, value); 
        }
        
        public long TxPackets 
        { 
            get => _txPackets; 
            set => SetProperty(ref _txPackets, value); 
        }
        
        public long RxErrors 
        { 
            get => _rxErrors; 
            set => SetProperty(ref _rxErrors, value); 
        }
        
        public long TxErrors 
        { 
            get => _txErrors; 
            set => SetProperty(ref _txErrors, value); 
        }
        
        public long RxDrops 
        { 
            get => _rxDrops; 
            set => SetProperty(ref _rxDrops, value); 
        }
        
        public long TxDrops 
        { 
            get => _txDrops; 
            set => SetProperty(ref _txDrops, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class DhcpLease : INotifyPropertyChanged
    {
        private string _id;
        private string _address;
        private string _macAddress;
        private string _clientId;
        private string _hostName;
        private string _comment;
        private bool _isDynamic;
        private DateTime _expiryTime;
        private DateTime _lastSeen;
        private string _server;
        private string _status;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Address 
        { 
            get => _address; 
            set => SetProperty(ref _address, value); 
        }
        
        public string MacAddress 
        { 
            get => _macAddress; 
            set => SetProperty(ref _macAddress, value); 
        }
        
        public string ClientId 
        { 
            get => _clientId; 
            set => SetProperty(ref _clientId, value); 
        }
        
        public string HostName 
        { 
            get => _hostName; 
            set => SetProperty(ref _hostName, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }
        
        public bool IsDynamic 
        { 
            get => _isDynamic; 
            set => SetProperty(ref _isDynamic, value); 
        }
        
        public DateTime ExpiryTime 
        { 
            get => _expiryTime; 
            set => SetProperty(ref _expiryTime, value); 
        }
        
        public DateTime LastSeen 
        { 
            get => _lastSeen; 
            set => SetProperty(ref _lastSeen, value); 
        }
        
        public string Server 
        { 
            get => _server; 
            set => SetProperty(ref _server, value); 
        }
        
        public string Status 
        { 
            get => _status; 
            set => SetProperty(ref _status, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class LogEntry : INotifyPropertyChanged
    {
        private string _id;
        private DateTime _time;
        private string _topics;
        private string _message;
        private string _facility;
        private string _severity;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public DateTime Time 
        { 
            get => _time; 
            set => SetProperty(ref _time, value); 
        }
        
        public string Topics 
        { 
            get => _topics; 
            set => SetProperty(ref _topics, value); 
        }
        
        public string Message 
        { 
            get => _message; 
            set => SetProperty(ref _message, value); 
        }
        
        public string Facility 
        { 
            get => _facility; 
            set => SetProperty(ref _facility, value); 
        }
        
        public string Severity 
        { 
            get => _severity; 
            set => SetProperty(ref _severity, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class FirewallRule : INotifyPropertyChanged
    {
        private string _id;
        private string _chain;
        private string _action;
        private string _protocol;
        private string _srcAddress;
        private string _dstAddress;
        private string _srcPort;
        private string _dstPort;
        private bool _disabled;
        private string _comment;
        private int _position;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Chain 
        { 
            get => _chain; 
            set => SetProperty(ref _chain, value); 
        }
        
        public string Action 
        { 
            get => _action; 
            set => SetProperty(ref _action, value); 
        }
        
        public string Protocol 
        { 
            get => _protocol; 
            set => SetProperty(ref _protocol, value); 
        }
        
        public string SrcAddress 
        { 
            get => _srcAddress; 
            set => SetProperty(ref _srcAddress, value); 
        }
        
        public string DstAddress 
        { 
            get => _dstAddress; 
            set => SetProperty(ref _dstAddress, value); 
        }
        
        public string SrcPort 
        { 
            get => _srcPort; 
            set => SetProperty(ref _srcPort, value); 
        }
        
        public string DstPort 
        { 
            get => _dstPort; 
            set => SetProperty(ref _dstPort, value); 
        }
        
        public bool Disabled 
        { 
            get => _disabled; 
            set => SetProperty(ref _disabled, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }
        
        public int Position 
        { 
            get => _position; 
            set => SetProperty(ref _position, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class VpnTunnel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _type;
        private string _remoteAddress;
        private string _localAddress;
        private bool _isActive;
        private DateTime _uptime;
        private string _comment;
        private bool _disabled;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value); 
        }
        
        public string Type 
        { 
            get => _type; 
            set => SetProperty(ref _type, value); 
        }
        
        public string RemoteAddress 
        { 
            get => _remoteAddress; 
            set => SetProperty(ref _remoteAddress, value); 
        }
        
        public string LocalAddress 
        { 
            get => _localAddress; 
            set => SetProperty(ref _localAddress, value); 
        }
        
        public bool IsActive 
        { 
            get => _isActive; 
            set => SetProperty(ref _isActive, value); 
        }
        
        public DateTime Uptime 
        { 
            get => _uptime; 
            set => SetProperty(ref _uptime, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }
        
        public bool Disabled 
        { 
            get => _disabled; 
            set => SetProperty(ref _disabled, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class QosRule : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _target;
        private string _parent;
        private long _maxLimit;
        private long _minLimit;
        private int _priority;
        private string _burst;
        private string _burstTime;
        private string _burstThreshold;
        private bool _disabled;
        private string _comment;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value); 
        }
        
        public string Target 
        { 
            get => _target; 
            set => SetProperty(ref _target, value); 
        }
        
        public string Parent 
        { 
            get => _parent; 
            set => SetProperty(ref _parent, value); 
        }
        
        public long MaxLimit 
        { 
            get => _maxLimit; 
            set => SetProperty(ref _maxLimit, value); 
        }
        
        public long MinLimit 
        { 
            get => _minLimit; 
            set => SetProperty(ref _minLimit, value); 
        }
        
        public int Priority 
        { 
            get => _priority; 
            set => SetProperty(ref _priority, value); 
        }
        
        public string Burst 
        { 
            get => _burst; 
            set => SetProperty(ref _burst, value); 
        }
        
        public string BurstTime 
        { 
            get => _burstTime; 
            set => SetProperty(ref _burstTime, value); 
        }
        
        public string BurstThreshold 
        { 
            get => _burstThreshold; 
            set => SetProperty(ref _burstThreshold, value); 
        }
        
        public bool Disabled 
        { 
            get => _disabled; 
            set => SetProperty(ref _disabled, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class TrafficFlow : INotifyPropertyChanged
    {
        private string _id;
        private string _srcAddress;
        private string _dstAddress;
        private string _protocol;
        private string _srcPort;
        private string _dstPort;
        private long _bytes;
        private long _packets;
        private DateTime _timestamp;
        private string _interface;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string SrcAddress 
        { 
            get => _srcAddress; 
            set => SetProperty(ref _srcAddress, value); 
        }
        
        public string DstAddress 
        { 
            get => _dstAddress; 
            set => SetProperty(ref _dstAddress, value); 
        }
        
        public string Protocol 
        { 
            get => _protocol; 
            set => SetProperty(ref _protocol, value); 
        }
        
        public string SrcPort 
        { 
            get => _srcPort; 
            set => SetProperty(ref _srcPort, value); 
        }
        
        public string DstPort 
        { 
            get => _dstPort; 
            set => SetProperty(ref _dstPort, value); 
        }
        
        public long Bytes 
        { 
            get => _bytes; 
            set => SetProperty(ref _bytes, value); 
        }
        
        public long Packets 
        { 
            get => _packets; 
            set => SetProperty(ref _packets, value); 
        }
        
        public DateTime Timestamp 
        { 
            get => _timestamp; 
            set => SetProperty(ref _timestamp, value); 
        }
        
        public string Interface 
        { 
            get => _interface; 
            set => SetProperty(ref _interface, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class CloudVpnUser : INotifyPropertyChanged
    {
        private string _id;
        private string _username;
        private string _email;
        private string _status;
        private DateTime _createdOn;
        private DateTime? _lastActive;
        private bool _disabled;
        private string _comment;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Username 
        { 
            get => _username; 
            set => SetProperty(ref _username, value); 
        }
        
        public string Email 
        { 
            get => _email; 
            set => SetProperty(ref _email, value); 
        }
        
        public string Status 
        { 
            get => _status; 
            set => SetProperty(ref _status, value); 
        }
        
        public DateTime CreatedOn 
        { 
            get => _createdOn; 
            set => SetProperty(ref _createdOn, value); 
        }
        
        public DateTime? LastActive 
        { 
            get => _lastActive; 
            set => SetProperty(ref _lastActive, value); 
        }
        
        public bool Disabled 
        { 
            get => _disabled; 
            set => SetProperty(ref _disabled, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}