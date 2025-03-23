using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class DhcpLease : ModelBase
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
    }
}