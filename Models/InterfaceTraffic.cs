using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class InterfaceTraffic : ModelBase
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
    }
}