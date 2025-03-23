using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public enum ConnectionType
    {
        Api,
        Snmp,
        Ssh,
        Winbox,
        Cloud
    }

    public enum DeviceStatus
    {
        Online,
        Offline,
        Warning,
        Error,
        Unknown
    }
    
    public class RouterDevice : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _ipAddress;
        private int _apiPort = 8728;
        private int _snmpPort = 161;
        private int _sshPort = 22;
        private int _winboxPort = 8291;
        private string _username;
        private string _password;
        private string _snmpCommunity = "public";
        private string _sshPrivateKeyPath;
        private bool _useSsl;
        private ConnectionType _connectionType = ConnectionType.Api;
        private DeviceStatus _status = DeviceStatus.Unknown;
        private DateTime _lastSeenOnline;
        private string _model;
        private string _serialNumber;
        private string _firmwareVersion;
        private TimeSpan _uptime;
        private string _siteId;
        private string _groupName;
        private List<string> _tags = new List<string>();
        private string _notes;
        private bool _isMonitored = true;
        private ObservableCollection<ResourceUsage> _resourceHistory = new ObservableCollection<ResourceUsage>();
        private ObservableCollection<NetworkInterface> _interfaces = new ObservableCollection<NetworkInterface>();
        private ObservableCollection<DhcpLease> _dhcpLeases = new ObservableCollection<DhcpLease>();
        private ObservableCollection<LogEntry> _logs = new ObservableCollection<LogEntry>();
        private ObservableCollection<FirewallRule> _firewallRules = new ObservableCollection<FirewallRule>();
        private ObservableCollection<VpnTunnel> _vpnTunnels = new ObservableCollection<VpnTunnel>();
        private ObservableCollection<QosRule> _qosRules = new ObservableCollection<QosRule>();
        private ObservableCollection<TrafficFlow> _trafficFlows = new ObservableCollection<TrafficFlow>();
        private DateTime _lastUpdated;
        private double _cpuUsage;
        private double _memoryUsage;
        private double _diskUsage;
        private double _temperature;
        private int _voltage;
        private string _cloudId;
        private bool _isCloudManaged;
        private ObservableCollection<CloudVpnUser> _cloudVpnUsers = new ObservableCollection<CloudVpnUser>();
        private double _latitude;
        private double _longitude;
        private string _locationName;
        private bool _isLocationManual;

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
        
        public string IpAddress 
        { 
            get => _ipAddress; 
            set => SetProperty(ref _ipAddress, value); 
        }
        
        public int ApiPort 
        { 
            get => _apiPort; 
            set => SetProperty(ref _apiPort, value); 
        }
        
        public int SnmpPort 
        { 
            get => _snmpPort; 
            set => SetProperty(ref _snmpPort, value); 
        }
        
        public int SshPort 
        { 
            get => _sshPort; 
            set => SetProperty(ref _sshPort, value); 
        }
        
        public int WinboxPort 
        { 
            get => _winboxPort; 
            set => SetProperty(ref _winboxPort, value); 
        }
        
        public string Username 
        { 
            get => _username; 
            set => SetProperty(ref _username, value); 
        }
        
        public string Password 
        { 
            get => _password; 
            set => SetProperty(ref _password, value); 
        }
        
        public string SnmpCommunity 
        { 
            get => _snmpCommunity; 
            set => SetProperty(ref _snmpCommunity, value); 
        }
        
        public string SshPrivateKeyPath 
        { 
            get => _sshPrivateKeyPath; 
            set => SetProperty(ref _sshPrivateKeyPath, value); 
        }
        
        public bool UseSsl 
        { 
            get => _useSsl; 
            set => SetProperty(ref _useSsl, value); 
        }
        
        public ConnectionType ConnectionType 
        { 
            get => _connectionType; 
            set => SetProperty(ref _connectionType, value); 
        }
        
        public DeviceStatus Status 
        { 
            get => _status; 
            set => SetProperty(ref _status, value); 
        }
        
        public DateTime LastSeenOnline 
        { 
            get => _lastSeenOnline; 
            set => SetProperty(ref _lastSeenOnline, value); 
        }
        
        public string Model 
        { 
            get => _model; 
            set => SetProperty(ref _model, value); 
        }
        
        public string SerialNumber 
        { 
            get => _serialNumber; 
            set => SetProperty(ref _serialNumber, value); 
        }
        
        public string FirmwareVersion 
        { 
            get => _firmwareVersion; 
            set => SetProperty(ref _firmwareVersion, value); 
        }
        
        public TimeSpan Uptime 
        { 
            get => _uptime; 
            set => SetProperty(ref _uptime, value); 
        }
        
        public string SiteId 
        { 
            get => _siteId; 
            set => SetProperty(ref _siteId, value); 
        }
        
        public string GroupName 
        { 
            get => _groupName; 
            set => SetProperty(ref _groupName, value); 
        }
        
        public List<string> Tags 
        { 
            get => _tags; 
            set => SetProperty(ref _tags, value); 
        }
        
        public string Notes 
        { 
            get => _notes; 
            set => SetProperty(ref _notes, value); 
        }
        
        public bool IsMonitored 
        { 
            get => _isMonitored; 
            set => SetProperty(ref _isMonitored, value); 
        }
        
        public ObservableCollection<ResourceUsage> ResourceHistory 
        { 
            get => _resourceHistory; 
            set => SetProperty(ref _resourceHistory, value); 
        }
        
        public ObservableCollection<NetworkInterface> Interfaces 
        { 
            get => _interfaces; 
            set => SetProperty(ref _interfaces, value); 
        }
        
        public ObservableCollection<DhcpLease> DhcpLeases 
        { 
            get => _dhcpLeases; 
            set => SetProperty(ref _dhcpLeases, value); 
        }
        
        public ObservableCollection<LogEntry> Logs 
        { 
            get => _logs; 
            set => SetProperty(ref _logs, value); 
        }
        
        public ObservableCollection<FirewallRule> FirewallRules 
        { 
            get => _firewallRules; 
            set => SetProperty(ref _firewallRules, value); 
        }
        
        public ObservableCollection<VpnTunnel> VpnTunnels 
        { 
            get => _vpnTunnels; 
            set => SetProperty(ref _vpnTunnels, value); 
        }
        
        public ObservableCollection<QosRule> QosRules 
        { 
            get => _qosRules; 
            set => SetProperty(ref _qosRules, value); 
        }
        
        public ObservableCollection<TrafficFlow> TrafficFlows 
        { 
            get => _trafficFlows; 
            set => SetProperty(ref _trafficFlows, value); 
        }
        
        public DateTime LastUpdated 
        { 
            get => _lastUpdated; 
            set => SetProperty(ref _lastUpdated, value); 
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
        
        public string CloudId 
        { 
            get => _cloudId; 
            set => SetProperty(ref _cloudId, value); 
        }
        
        public bool IsCloudManaged 
        { 
            get => _isCloudManaged; 
            set => SetProperty(ref _isCloudManaged, value); 
        }
        
        public ObservableCollection<CloudVpnUser> CloudVpnUsers 
        { 
            get => _cloudVpnUsers; 
            set => SetProperty(ref _cloudVpnUsers, value); 
        }
        
        public double Latitude 
        { 
            get => _latitude; 
            set => SetProperty(ref _latitude, value); 
        }
        
        public double Longitude 
        { 
            get => _longitude; 
            set => SetProperty(ref _longitude, value); 
        }
        
        public string LocationName 
        { 
            get => _locationName; 
            set => SetProperty(ref _locationName, value); 
        }
        
        public bool IsLocationManual 
        { 
            get => _isLocationManual; 
            set => SetProperty(ref _isLocationManual, value); 
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