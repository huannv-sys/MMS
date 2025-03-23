using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class CloudSite : ModelBase
    {
        private string _id;
        private string _name;
        private string _description;
        private string _address;
        private string _city;
        private string _state;
        private string _country;
        private string _zipCode;
        private double _latitude;
        private double _longitude;
        private string _contactName;
        private string _contactEmail;
        private string _contactPhone;
        private ObservableCollection<CloudDevice> _devices = new ObservableCollection<CloudDevice>();
        private DateTime _createdOn;
        private DateTime _lastUpdated;
        private string _notes;
        private bool _isActive;
        private string _ownerEmail;
        private List<string> _tags = new List<string>();

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
        
        public string Description 
        { 
            get => _description; 
            set => SetProperty(ref _description, value); 
        }
        
        public string Address 
        { 
            get => _address; 
            set => SetProperty(ref _address, value); 
        }
        
        public string City 
        { 
            get => _city; 
            set => SetProperty(ref _city, value); 
        }
        
        public string State 
        { 
            get => _state; 
            set => SetProperty(ref _state, value); 
        }
        
        public string Country 
        { 
            get => _country; 
            set => SetProperty(ref _country, value); 
        }
        
        public string ZipCode 
        { 
            get => _zipCode; 
            set => SetProperty(ref _zipCode, value); 
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
        
        public string ContactName 
        { 
            get => _contactName; 
            set => SetProperty(ref _contactName, value); 
        }
        
        public string ContactEmail 
        { 
            get => _contactEmail; 
            set => SetProperty(ref _contactEmail, value); 
        }
        
        public string ContactPhone 
        { 
            get => _contactPhone; 
            set => SetProperty(ref _contactPhone, value); 
        }
        
        public ObservableCollection<CloudDevice> Devices 
        { 
            get => _devices; 
            set => SetProperty(ref _devices, value); 
        }
        
        public DateTime CreatedOn 
        { 
            get => _createdOn; 
            set => SetProperty(ref _createdOn, value); 
        }
        
        public DateTime LastUpdated 
        { 
            get => _lastUpdated; 
            set => SetProperty(ref _lastUpdated, value); 
        }
        
        public string Notes 
        { 
            get => _notes; 
            set => SetProperty(ref _notes, value); 
        }
        
        public bool IsActive 
        { 
            get => _isActive; 
            set => SetProperty(ref _isActive, value); 
        }
        
        public string OwnerEmail 
        { 
            get => _ownerEmail; 
            set => SetProperty(ref _ownerEmail, value); 
        }
        
        public List<string> Tags 
        { 
            get => _tags; 
            set => SetProperty(ref _tags, value); 
        }
    }

    public class CloudDevice : ModelBase
    {
        private string _id;
        private string _name;
        private string _model;
        private string _serialNumber;
        private string _ipAddress;
        private string _macAddress;
        private string _firmwareVersion;
        private string _boardName;
        private string _identity;
        private DateTime _lastSeen;
        private bool _isOnline;
        private string _siteId;
        private string _siteName;
        private string _ownerEmail;
        private DateTime _registeredDate;
        private DeviceStatus _status;
        private bool _isMonitored;
        private string _notes;
        private List<string> _tags = new List<string>();
        private DateTime _lastBackup;
        private string _routerOsVersion;
        private ObservableCollection<CloudVpnUser> _vpnUsers = new ObservableCollection<CloudVpnUser>();
        private TimeSpan _uptime;
        private bool _hasVpnAccess;
        private bool _hasUpdates;
        private string _updateChannel;
        private string _architecture;
        private double _cpuFrequency;
        private int _cpuCount;
        private string _cpuLoad;
        private long _totalMemory;
        private long _freeMemory;
        private long _totalStorage;
        private long _freeStorage;

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
        
        public string IpAddress 
        { 
            get => _ipAddress; 
            set => SetProperty(ref _ipAddress, value); 
        }
        
        public string MacAddress 
        { 
            get => _macAddress; 
            set => SetProperty(ref _macAddress, value); 
        }
        
        public string FirmwareVersion 
        { 
            get => _firmwareVersion; 
            set => SetProperty(ref _firmwareVersion, value); 
        }
        
        public string BoardName 
        { 
            get => _boardName; 
            set => SetProperty(ref _boardName, value); 
        }
        
        public string Identity 
        { 
            get => _identity; 
            set => SetProperty(ref _identity, value); 
        }
        
        public DateTime LastSeen 
        { 
            get => _lastSeen; 
            set => SetProperty(ref _lastSeen, value); 
        }
        
        public bool IsOnline 
        { 
            get => _isOnline; 
            set => SetProperty(ref _isOnline, value); 
        }
        
        public string SiteId 
        { 
            get => _siteId; 
            set => SetProperty(ref _siteId, value); 
        }
        
        public string SiteName 
        { 
            get => _siteName; 
            set => SetProperty(ref _siteName, value); 
        }
        
        public string OwnerEmail 
        { 
            get => _ownerEmail; 
            set => SetProperty(ref _ownerEmail, value); 
        }
        
        public DateTime RegisteredDate 
        { 
            get => _registeredDate; 
            set => SetProperty(ref _registeredDate, value); 
        }
        
        public DeviceStatus Status 
        { 
            get => _status; 
            set => SetProperty(ref _status, value); 
        }
        
        public bool IsMonitored 
        { 
            get => _isMonitored; 
            set => SetProperty(ref _isMonitored, value); 
        }
        
        public string Notes 
        { 
            get => _notes; 
            set => SetProperty(ref _notes, value); 
        }
        
        public List<string> Tags 
        { 
            get => _tags; 
            set => SetProperty(ref _tags, value); 
        }
        
        public DateTime LastBackup 
        { 
            get => _lastBackup; 
            set => SetProperty(ref _lastBackup, value); 
        }
        
        public string RouterOsVersion 
        { 
            get => _routerOsVersion; 
            set => SetProperty(ref _routerOsVersion, value); 
        }
        
        public ObservableCollection<CloudVpnUser> VpnUsers 
        { 
            get => _vpnUsers; 
            set => SetProperty(ref _vpnUsers, value); 
        }
        
        public TimeSpan Uptime 
        { 
            get => _uptime; 
            set => SetProperty(ref _uptime, value); 
        }
        
        public bool HasVpnAccess 
        { 
            get => _hasVpnAccess; 
            set => SetProperty(ref _hasVpnAccess, value); 
        }
        
        public bool HasUpdates 
        { 
            get => _hasUpdates; 
            set => SetProperty(ref _hasUpdates, value); 
        }
        
        public string UpdateChannel 
        { 
            get => _updateChannel; 
            set => SetProperty(ref _updateChannel, value); 
        }
        
        public string Architecture 
        { 
            get => _architecture; 
            set => SetProperty(ref _architecture, value); 
        }
        
        public double CpuFrequency 
        { 
            get => _cpuFrequency; 
            set => SetProperty(ref _cpuFrequency, value); 
        }
        
        public int CpuCount 
        { 
            get => _cpuCount; 
            set => SetProperty(ref _cpuCount, value); 
        }
        
        public string CpuLoad 
        { 
            get => _cpuLoad; 
            set => SetProperty(ref _cpuLoad, value); 
        }
        
        public long TotalMemory 
        { 
            get => _totalMemory; 
            set => SetProperty(ref _totalMemory, value); 
        }
        
        public long FreeMemory 
        { 
            get => _freeMemory; 
            set => SetProperty(ref _freeMemory, value); 
        }
        
        public long TotalStorage 
        { 
            get => _totalStorage; 
            set => SetProperty(ref _totalStorage, value); 
        }
        
        public long FreeStorage 
        { 
            get => _freeStorage; 
            set => SetProperty(ref _freeStorage, value); 
        }
    }
}