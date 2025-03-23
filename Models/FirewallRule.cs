using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class FirewallRule : ModelBase
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
    }
}