using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class QosRule : ModelBase
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
    }
}