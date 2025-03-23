using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class VpnTunnel : ModelBase
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
    }
}