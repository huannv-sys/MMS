using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class ResourceUsage : ModelBase
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
    }
}