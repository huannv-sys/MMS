using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class NetworkInterface : ModelBase
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
        private long _txDrops;
        private long _rxDrops;
        private ObservableCollection<InterfaceTraffic> _trafficHistory = new ObservableCollection<InterfaceTraffic>();
        private DateTime _lastUpdated;
        private long _speed;
        private bool _isDynamic;
        private string _comment;
        private double _linkSpeed;
        private string _linkType;
        private bool _isWireless;
        private bool _isVirtual;
        private string _parent;

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
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
        }
        
        public double LinkSpeed 
        { 
            get => _linkSpeed; 
            set => SetProperty(ref _linkSpeed, value); 
        }
        
        public string LinkType 
        { 
            get => _linkType; 
            set => SetProperty(ref _linkType, value); 
        }
        
        public bool IsWireless 
        { 
            get => _isWireless; 
            set => SetProperty(ref _isWireless, value); 
        }
        
        public bool IsVirtual 
        { 
            get => _isVirtual; 
            set => SetProperty(ref _isVirtual, value); 
        }
        
        public string Parent 
        { 
            get => _parent; 
            set => SetProperty(ref _parent, value); 
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
    }
}