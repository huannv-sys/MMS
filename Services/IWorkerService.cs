using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface IWorkerService
    {
        bool IsRunning { get; }
        
        void Start();
        void Stop();
        
        void SetMonitoringInterval(MonitoringType type, int intervalSeconds);
        int GetMonitoringInterval(MonitoringType type);
        
        void PauseMonitoring(MonitoringType type);
        void ResumeMonitoring(MonitoringType type);
        bool IsMonitoringPaused(MonitoringType type);
        
        Task<bool> RunImmediateMonitoringAsync(RouterDevice device, MonitoringType type);
        
        Task<List<MonitoringTask>> GetScheduledTasksAsync();
        Task<MonitoringStats> GetMonitoringStatsAsync();
        
        void AddDevice(RouterDevice device);
        void RemoveDevice(string deviceId);
        void UpdateDevice(RouterDevice device);
        List<RouterDevice> GetMonitoredDevices();
        
        event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
        event EventHandler<DeviceMonitoringCompletedEventArgs> DeviceMonitoringCompleted;
        event EventHandler<MonitoringErrorEventArgs> MonitoringError;
    }
    
    public enum MonitoringType
    {
        Resource,
        Interface,
        Dhcp,
        Log,
        Connectivity,
        Cloud
    }
    
    public class MonitoringTask
    {
        public string Id { get; set; }
        public RouterDevice Device { get; set; }
        public MonitoringType Type { get; set; }
        public DateTime LastRun { get; set; }
        public DateTime NextRun { get; set; }
        public bool IsPaused { get; set; }
        public int IntervalSeconds { get; set; }
        public bool IsRunning { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }
    
    public class MonitoringStats
    {
        public int TotalDevices { get; set; }
        public int OnlineDevices { get; set; }
        public int OfflineDevices { get; set; }
        public int WarningDevices { get; set; }
        public int ErrorDevices { get; set; }
        public Dictionary<MonitoringType, int> CompletedTasks { get; set; }
        public Dictionary<MonitoringType, int> FailedTasks { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public Dictionary<string, int> ErrorCounts { get; set; }
    }
    
    public class DeviceStatusChangedEventArgs : EventArgs
    {
        public RouterDevice Device { get; set; }
        public DeviceStatus OldStatus { get; set; }
        public DeviceStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class DeviceMonitoringCompletedEventArgs : EventArgs
    {
        public RouterDevice Device { get; set; }
        public MonitoringType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    public class MonitoringErrorEventArgs : EventArgs
    {
        public RouterDevice Device { get; set; }
        public MonitoringType Type { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public DateTime Timestamp { get; set; }
    }
}