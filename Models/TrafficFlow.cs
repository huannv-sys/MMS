using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class TrafficFlow : ModelBase
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
    }
}