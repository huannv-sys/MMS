using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents system information for a MikroTik router
    /// </summary>
    public class SystemInfo : ModelBase
    {
        private string _model;
        private string _architecture;
        private string _routerOsVersion;
        private string _cpuModel;
        private int _cpuCount;
        private int _cpuFrequencyMHz;
        private double _cpuLoad;
        private string _totalMemory;
        private string _freeMemory;
        private string _totalStorage;
        private string _freeStorage;
        private long _uptimeSeconds;
        private DateTime _systemDateTime;
        
        /// <summary>
        /// Gets or sets the router model
        /// </summary>
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
        
        /// <summary>
        /// Gets or sets the router architecture
        /// </summary>
        public string Architecture
        {
            get => _architecture;
            set => SetProperty(ref _architecture, value);
        }
        
        /// <summary>
        /// Gets or sets the RouterOS version
        /// </summary>
        public string RouterOsVersion
        {
            get => _routerOsVersion;
            set => SetProperty(ref _routerOsVersion, value);
        }
        
        /// <summary>
        /// Gets or sets the CPU model
        /// </summary>
        public string CpuModel
        {
            get => _cpuModel;
            set => SetProperty(ref _cpuModel, value);
        }
        
        /// <summary>
        /// Gets or sets the CPU count
        /// </summary>
        public int CpuCount
        {
            get => _cpuCount;
            set => SetProperty(ref _cpuCount, value);
        }
        
        /// <summary>
        /// Gets or sets the CPU frequency in MHz
        /// </summary>
        public int CpuFrequencyMHz
        {
            get => _cpuFrequencyMHz;
            set => SetProperty(ref _cpuFrequencyMHz, value);
        }
        
        /// <summary>
        /// Gets or sets the CPU load percentage
        /// </summary>
        public double CpuLoad
        {
            get => _cpuLoad;
            set => SetProperty(ref _cpuLoad, value);
        }
        
        /// <summary>
        /// Gets or sets the total memory as a string
        /// </summary>
        public string TotalMemory
        {
            get => _totalMemory;
            set
            {
                if (SetProperty(ref _totalMemory, value))
                {
                    OnPropertyChanged(nameof(MemoryUsagePercentage));
                    OnPropertyChanged(nameof(MemoryUsageFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the free memory as a string
        /// </summary>
        public string FreeMemory
        {
            get => _freeMemory;
            set
            {
                if (SetProperty(ref _freeMemory, value))
                {
                    OnPropertyChanged(nameof(MemoryUsagePercentage));
                    OnPropertyChanged(nameof(MemoryUsageFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the total storage as a string
        /// </summary>
        public string TotalStorage
        {
            get => _totalStorage;
            set
            {
                if (SetProperty(ref _totalStorage, value))
                {
                    OnPropertyChanged(nameof(StorageUsagePercentage));
                    OnPropertyChanged(nameof(StorageUsageFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the free storage as a string
        /// </summary>
        public string FreeStorage
        {
            get => _freeStorage;
            set
            {
                if (SetProperty(ref _freeStorage, value))
                {
                    OnPropertyChanged(nameof(StorageUsagePercentage));
                    OnPropertyChanged(nameof(StorageUsageFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the uptime in seconds
        /// </summary>
        public long UptimeSeconds
        {
            get => _uptimeSeconds;
            set
            {
                if (SetProperty(ref _uptimeSeconds, value))
                {
                    OnPropertyChanged(nameof(UptimeFormatted));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the system date and time
        /// </summary>
        public DateTime SystemDateTime
        {
            get => _systemDateTime;
            set => SetProperty(ref _systemDateTime, value);
        }
        
        /// <summary>
        /// Gets the memory usage percentage
        /// </summary>
        public double MemoryUsagePercentage
        {
            get
            {
                if (string.IsNullOrEmpty(TotalMemory) || string.IsNullOrEmpty(FreeMemory))
                    return 0;
                    
                // Parse memory values
                if (TryParseBytes(TotalMemory, out long total) && TryParseBytes(FreeMemory, out long free))
                {
                    if (total > 0)
                        return 100 * (1 - (double)free / total);
                }
                
                return 0;
            }
        }
        
        /// <summary>
        /// Gets the memory usage as a formatted string
        /// </summary>
        public string MemoryUsageFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(TotalMemory) || string.IsNullOrEmpty(FreeMemory))
                    return "N/A";
                    
                // Parse memory values
                if (TryParseBytes(TotalMemory, out long total) && TryParseBytes(FreeMemory, out long free))
                {
                    string totalFormatted = FormatBytes(total);
                    string freeFormatted = FormatBytes(free);
                    string usedFormatted = FormatBytes(total - free);
                    
                    return $"{usedFormatted} of {totalFormatted} ({MemoryUsagePercentage:0.0}%)";
                }
                
                return "N/A";
            }
        }
        
        /// <summary>
        /// Gets the storage usage percentage
        /// </summary>
        public double StorageUsagePercentage
        {
            get
            {
                if (string.IsNullOrEmpty(TotalStorage) || string.IsNullOrEmpty(FreeStorage))
                    return 0;
                    
                // Parse storage values
                if (TryParseBytes(TotalStorage, out long total) && TryParseBytes(FreeStorage, out long free))
                {
                    if (total > 0)
                        return 100 * (1 - (double)free / total);
                }
                
                return 0;
            }
        }
        
        /// <summary>
        /// Gets the storage usage as a formatted string
        /// </summary>
        public string StorageUsageFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(TotalStorage) || string.IsNullOrEmpty(FreeStorage))
                    return "N/A";
                    
                // Parse storage values
                if (TryParseBytes(TotalStorage, out long total) && TryParseBytes(FreeStorage, out long free))
                {
                    string totalFormatted = FormatBytes(total);
                    string freeFormatted = FormatBytes(free);
                    string usedFormatted = FormatBytes(total - free);
                    
                    return $"{usedFormatted} of {totalFormatted} ({StorageUsagePercentage:0.0}%)";
                }
                
                return "N/A";
            }
        }
        
        /// <summary>
        /// Gets the uptime as a formatted string
        /// </summary>
        public string UptimeFormatted
        {
            get
            {
                TimeSpan uptime = TimeSpan.FromSeconds(UptimeSeconds);
                
                if (uptime.TotalDays >= 1)
                    return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
                    
                if (uptime.TotalHours >= 1)
                    return $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";
                    
                if (uptime.TotalMinutes >= 1)
                    return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
                    
                return $"{uptime.Seconds}s";
            }
        }
        
        /// <summary>
        /// Gets the CPU information as a formatted string
        /// </summary>
        public string CpuInfoFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(CpuModel))
                    return "N/A";
                    
                if (CpuCount > 1)
                    return $"{CpuModel} ({CpuCount} cores @ {CpuFrequencyMHz} MHz)";
                    
                return $"{CpuModel} ({CpuFrequencyMHz} MHz)";
            }
        }
        
        /// <summary>
        /// Try to parse a bytes string (e.g. "1.5GiB")
        /// </summary>
        /// <param name="bytesString">The bytes string to parse</param>
        /// <param name="bytes">The parsed bytes</param>
        /// <returns>True if parsing was successful, otherwise false</returns>
        private bool TryParseBytes(string bytesString, out long bytes)
        {
            bytes = 0;
            
            if (string.IsNullOrEmpty(bytesString))
                return false;
                
            try
            {
                // Handle case where the string is just a number
                if (long.TryParse(bytesString, out bytes))
                    return true;
                    
                // Extract number part and unit part
                string numberPart = string.Empty;
                string unitPart = string.Empty;
                
                int i = 0;
                while (i < bytesString.Length && (char.IsDigit(bytesString[i]) || bytesString[i] == '.'))
                {
                    numberPart += bytesString[i];
                    i++;
                }
                
                while (i < bytesString.Length && char.IsLetter(bytesString[i]))
                {
                    unitPart += bytesString[i];
                    i++;
                }
                
                if (string.IsNullOrEmpty(numberPart) || !double.TryParse(numberPart, out double number))
                    return false;
                    
                // Convert to bytes based on unit
                unitPart = unitPart.ToLowerInvariant();
                
                if (unitPart.Contains("k"))
                    number *= 1024;
                else if (unitPart.Contains("m"))
                    number *= 1024 * 1024;
                else if (unitPart.Contains("g"))
                    number *= 1024 * 1024 * 1024;
                else if (unitPart.Contains("t"))
                    number *= 1024L * 1024L * 1024L * 1024L;
                    
                bytes = (long)number;
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Format a bytes value to a readable string
        /// </summary>
        /// <param name="bytes">The bytes value</param>
        /// <returns>A formatted string</returns>
        private string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;
            const long TB = GB * 1024;
            
            if (bytes < KB)
                return $"{bytes} B";
                
            if (bytes < MB)
                return $"{bytes / (double)KB:0.0} KiB";
                
            if (bytes < GB)
                return $"{bytes / (double)MB:0.0} MiB";
                
            if (bytes < TB)
                return $"{bytes / (double)GB:0.0} GiB";
                
            return $"{bytes / (double)TB:0.0} TiB";
        }
        

    }
}