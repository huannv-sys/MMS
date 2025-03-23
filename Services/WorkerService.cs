using System;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using MikroTikMonitor.Models;
using log4net;
using Timer = System.Timers.Timer;

namespace MikroTikMonitor.Services
{
    public class WorkerService : IWorkerService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WorkerService));
        
        private readonly IRouterApiService _routerApiService;
        private readonly ISnmpService _snmpService;
        private readonly ICloudService _cloudService;
        private readonly IConfiguration _configuration;
        
        private readonly List<Timer> _timers = new List<Timer>();
        private readonly ConcurrentDictionary<string, RouterDevice> _devices = new ConcurrentDictionary<string, RouterDevice>();
        private readonly ConcurrentDictionary<string, MonitoringTask> _tasks = new ConcurrentDictionary<string, MonitoringTask>();
        
        private readonly Dictionary<MonitoringType, int> _intervalSeconds = new Dictionary<MonitoringType, int>();
        private readonly Dictionary<MonitoringType, bool> _isPaused = new Dictionary<MonitoringType, bool>();
        private readonly Dictionary<MonitoringType, int> _completedTasks = new Dictionary<MonitoringType, int>();
        private readonly Dictionary<MonitoringType, int> _failedTasks = new Dictionary<MonitoringType, int>();
        private readonly Dictionary<string, int> _errorCounts = new Dictionary<string, int>();
        
        private bool _isRunning;
        private DateTime _startTime;
        
        public bool IsRunning => _isRunning;
        
        public event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
        public event EventHandler<DeviceMonitoringCompletedEventArgs> DeviceMonitoringCompleted;
        public event EventHandler<MonitoringErrorEventArgs> MonitoringError;
        
        public WorkerService(
            IRouterApiService routerApiService,
            ISnmpService snmpService,
            ICloudService cloudService,
            IConfiguration configuration)
        {
            _routerApiService = routerApiService;
            _snmpService = snmpService;
            _cloudService = cloudService;
            _configuration = configuration;
            
            // Initialize interval settings from configuration
            _intervalSeconds[MonitoringType.Resource] = _configuration.GetValue<int>("MonitoringSettings:ResourceMonitoringInterval", 5);
            _intervalSeconds[MonitoringType.Interface] = _configuration.GetValue<int>("MonitoringSettings:InterfaceMonitoringInterval", 1);
            _intervalSeconds[MonitoringType.Dhcp] = _configuration.GetValue<int>("MonitoringSettings:DhcpMonitoringInterval", 30);
            _intervalSeconds[MonitoringType.Log] = _configuration.GetValue<int>("MonitoringSettings:LogMonitoringInterval", 30);
            _intervalSeconds[MonitoringType.Connectivity] = _configuration.GetValue<int>("MonitoringSettings:ConnectivityCheckInterval", 15);
            _intervalSeconds[MonitoringType.Cloud] = _configuration.GetValue<int>("MonitoringSettings:CloudUpdateInterval", 60);
            
            // Initialize pause settings
            foreach (MonitoringType type in Enum.GetValues(typeof(MonitoringType)))
            {
                _isPaused[type] = false;
                _completedTasks[type] = 0;
                _failedTasks[type] = 0;
            }
        }
        
        public void Start()
        {
            if (_isRunning)
            {
                log.Warn("Worker service is already running");
                return;
            }
            
            log.Info("Starting worker service");
            
            _isRunning = true;
            _startTime = DateTime.Now;
            
            // Create timers for each monitoring type
            CreateTimer(MonitoringType.Resource);
            CreateTimer(MonitoringType.Interface);
            CreateTimer(MonitoringType.Dhcp);
            CreateTimer(MonitoringType.Log);
            CreateTimer(MonitoringType.Connectivity);
            CreateTimer(MonitoringType.Cloud);
            
            log.Info("Worker service started");
        }
        
        public void Stop()
        {
            if (!_isRunning)
            {
                log.Warn("Worker service is not running");
                return;
            }
            
            log.Info("Stopping worker service");
            
            // Stop all timers
            foreach (var timer in _timers)
            {
                timer.Stop();
                timer.Dispose();
            }
            
            _timers.Clear();
            _isRunning = false;
            
            log.Info("Worker service stopped");
        }
        
        public void SetMonitoringInterval(MonitoringType type, int intervalSeconds)
        {
            if (intervalSeconds <= 0)
            {
                log.Error($"Invalid interval ({intervalSeconds}) for {type} monitoring");
                return;
            }
            
            log.Info($"Setting {type} monitoring interval to {intervalSeconds} seconds");
            
            _intervalSeconds[type] = intervalSeconds;
            
            // Update timer if running
            if (_isRunning)
            {
                var timer = _timers.FirstOrDefault(t => (MonitoringType)t.Tag == type);
                if (timer != null)
                {
                    timer.Interval = intervalSeconds * 1000;
                }
            }
        }
        
        public int GetMonitoringInterval(MonitoringType type)
        {
            return _intervalSeconds.TryGetValue(type, out var interval) ? interval : 0;
        }
        
        public void PauseMonitoring(MonitoringType type)
        {
            log.Info($"Pausing {type} monitoring");
            _isPaused[type] = true;
        }
        
        public void ResumeMonitoring(MonitoringType type)
        {
            log.Info($"Resuming {type} monitoring");
            _isPaused[type] = false;
        }
        
        public bool IsMonitoringPaused(MonitoringType type)
        {
            return _isPaused.TryGetValue(type, out var isPaused) && isPaused;
        }
        
        public async Task<bool> RunImmediateMonitoringAsync(RouterDevice device, MonitoringType type)
        {
            if (device == null)
            {
                log.Error("Cannot run monitoring for null device");
                return false;
            }
            
            log.Info($"Running immediate {type} monitoring for device {device.Name}");
            
            try
            {
                var success = await RunMonitoringTaskAsync(device, type);
                
                if (success)
                {
                    log.Info($"Immediate {type} monitoring completed successfully for device {device.Name}");
                }
                else
                {
                    log.Error($"Immediate {type} monitoring failed for device {device.Name}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                log.Error($"Error running immediate {type} monitoring for device {device.Name}: {ex.Message}", ex);
                
                // Raise error event
                RaiseMonitoringError(device, type, ex.Message, ex);
                
                return false;
            }
        }
        
        public async Task<List<MonitoringTask>> GetScheduledTasksAsync()
        {
            return await Task.FromResult(_tasks.Values.ToList());
        }
        
        public async Task<MonitoringStats> GetMonitoringStatsAsync()
        {
            var stats = new MonitoringStats
            {
                TotalDevices = _devices.Count,
                OnlineDevices = _devices.Values.Count(d => d.Status == DeviceStatus.Online),
                OfflineDevices = _devices.Values.Count(d => d.Status == DeviceStatus.Offline),
                WarningDevices = _devices.Values.Count(d => d.Status == DeviceStatus.Warning),
                ErrorDevices = _devices.Values.Count(d => d.Status == DeviceStatus.Error),
                CompletedTasks = new Dictionary<MonitoringType, int>(_completedTasks),
                FailedTasks = new Dictionary<MonitoringType, int>(_failedTasks),
                StartTime = _startTime,
                Uptime = DateTime.Now - _startTime,
                ErrorCounts = new Dictionary<string, int>(_errorCounts)
            };
            
            return await Task.FromResult(stats);
        }
        
        public void AddDevice(RouterDevice device)
        {
            if (device == null)
            {
                log.Error("Cannot add null device");
                return;
            }
            
            if (string.IsNullOrEmpty(device.Id))
            {
                device.Id = Guid.NewGuid().ToString();
            }
            
            if (_devices.TryAdd(device.Id, device))
            {
                log.Info($"Added device {device.Name} ({device.Id}) to monitoring");
                
                // Create monitoring tasks for the device
                CreateDeviceMonitoringTasks(device);
            }
            else
            {
                log.Warn($"Device {device.Name} ({device.Id}) is already being monitored");
            }
        }
        
        public void RemoveDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                log.Error("Cannot remove device with null or empty ID");
                return;
            }
            
            if (_devices.TryRemove(deviceId, out var device))
            {
                log.Info($"Removed device {device.Name} ({deviceId}) from monitoring");
                
                // Remove monitoring tasks for the device
                var tasksToRemove = _tasks.Where(t => t.Value.Device.Id == deviceId).Select(t => t.Key).ToList();
                foreach (var taskId in tasksToRemove)
                {
                    _tasks.TryRemove(taskId, out _);
                }
            }
            else
            {
                log.Warn($"Device with ID {deviceId} is not being monitored");
            }
        }
        
        public void UpdateDevice(RouterDevice device)
        {
            if (device == null || string.IsNullOrEmpty(device.Id))
            {
                log.Error("Cannot update null device or device with empty ID");
                return;
            }
            
            if (_devices.TryGetValue(device.Id, out var existingDevice))
            {
                // Update device
                _devices[device.Id] = device;
                
                log.Info($"Updated device {device.Name} ({device.Id}) in monitoring");
                
                // Update device in monitoring tasks
                foreach (var task in _tasks.Values.Where(t => t.Device.Id == device.Id))
                {
                    task.Device = device;
                }
            }
            else
            {
                log.Warn($"Device {device.Name} ({device.Id}) is not being monitored");
            }
        }
        
        public List<RouterDevice> GetMonitoredDevices()
        {
            return _devices.Values.ToList();
        }
        
        private void CreateTimer(MonitoringType type)
        {
            var intervalMs = _intervalSeconds[type] * 1000;
            
            var timer = new Timer(intervalMs);
            timer.Tag = type;
            timer.Elapsed += async (sender, e) => await OnTimerElapsed(type);
            timer.AutoReset = true;
            timer.Enabled = true;
            
            _timers.Add(timer);
            
            log.Debug($"Created timer for {type} monitoring with interval {_intervalSeconds[type]} seconds");
        }
        
        private async Task OnTimerElapsed(MonitoringType type)
        {
            if (_isPaused[type])
            {
                log.Debug($"{type} monitoring is paused, skipping");
                return;
            }
            
            log.Debug($"Running {type} monitoring for all devices");
            
            // Get devices to monitor
            var devices = _devices.Values.Where(d => d.IsMonitored).ToList();
            
            // Run monitoring for each device
            foreach (var device in devices)
            {
                try
                {
                    await RunMonitoringTaskAsync(device, type);
                }
                catch (Exception ex)
                {
                    log.Error($"Error running {type} monitoring for device {device.Name}: {ex.Message}", ex);
                    
                    // Increment error count
                    IncrementErrorCount(ex.GetType().Name);
                    
                    // Raise error event
                    RaiseMonitoringError(device, type, ex.Message, ex);
                }
            }
        }
        
        private async Task<bool> RunMonitoringTaskAsync(RouterDevice device, MonitoringType type)
        {
            if (device == null)
            {
                return false;
            }
            
            // Get task for this device and type
            var taskId = GetTaskId(device.Id, type);
            var task = _tasks.GetOrAdd(taskId, id => new MonitoringTask
            {
                Id = id,
                Device = device,
                Type = type,
                IntervalSeconds = _intervalSeconds[type],
                IsPaused = false,
                IsRunning = false,
                SuccessCount = 0,
                ErrorCount = 0
            });
            
            if (task.IsPaused)
            {
                log.Debug($"{type} monitoring for device {device.Name} is paused, skipping");
                return false;
            }
            
            if (task.IsRunning)
            {
                log.Warn($"{type} monitoring for device {device.Name} is already running, skipping");
                return false;
            }
            
            task.IsRunning = true;
            task.LastRun = DateTime.Now;
            
            var startTime = DateTime.Now;
            bool success = false;
            
            try
            {
                log.Debug($"Running {type} monitoring for device {device.Name}");
                
                // Run monitoring based on type
                switch (type)
                {
                    case MonitoringType.Resource:
                        success = await MonitorResourcesAsync(device);
                        break;
                    case MonitoringType.Interface:
                        success = await MonitorInterfacesAsync(device);
                        break;
                    case MonitoringType.Dhcp:
                        success = await MonitorDhcpAsync(device);
                        break;
                    case MonitoringType.Log:
                        success = await MonitorLogsAsync(device);
                        break;
                    case MonitoringType.Connectivity:
                        success = await CheckConnectivityAsync(device);
                        break;
                    case MonitoringType.Cloud:
                        success = await UpdateCloudAsync(device);
                        break;
                    default:
                        log.Error($"Unknown monitoring type: {type}");
                        break;
                }
                
                // Update task
                task.IsRunning = false;
                task.NextRun = DateTime.Now.AddSeconds(task.IntervalSeconds);
                
                if (success)
                {
                    task.SuccessCount++;
                    IncrementCompletedTasks(type);
                }
                else
                {
                    task.ErrorCount++;
                    IncrementFailedTasks(type);
                }
                
                // Raise event
                var duration = DateTime.Now - startTime;
                RaiseDeviceMonitoringCompleted(device, type, success, duration);
                
                return success;
            }
            catch (Exception ex)
            {
                task.IsRunning = false;
                task.ErrorCount++;
                IncrementFailedTasks(type);
                
                // Raise error event
                RaiseMonitoringError(device, type, ex.Message, ex);
                
                throw;
            }
        }
        
        private void CreateDeviceMonitoringTasks(RouterDevice device)
        {
            foreach (MonitoringType type in Enum.GetValues(typeof(MonitoringType)))
            {
                var taskId = GetTaskId(device.Id, type);
                _tasks.TryAdd(taskId, new MonitoringTask
                {
                    Id = taskId,
                    Device = device,
                    Type = type,
                    LastRun = DateTime.MinValue,
                    NextRun = DateTime.Now.AddSeconds(_intervalSeconds[type]),
                    IsPaused = false,
                    IntervalSeconds = _intervalSeconds[type],
                    IsRunning = false,
                    SuccessCount = 0,
                    ErrorCount = 0
                });
            }
        }
        
        private string GetTaskId(string deviceId, MonitoringType type)
        {
            return $"{deviceId}_{type}";
        }
        
        private void IncrementCompletedTasks(MonitoringType type)
        {
            lock (_completedTasks)
            {
                if (_completedTasks.ContainsKey(type))
                {
                    _completedTasks[type]++;
                }
                else
                {
                    _completedTasks[type] = 1;
                }
            }
        }
        
        private void IncrementFailedTasks(MonitoringType type)
        {
            lock (_failedTasks)
            {
                if (_failedTasks.ContainsKey(type))
                {
                    _failedTasks[type]++;
                }
                else
                {
                    _failedTasks[type] = 1;
                }
            }
        }
        
        private void IncrementErrorCount(string errorType)
        {
            lock (_errorCounts)
            {
                if (_errorCounts.ContainsKey(errorType))
                {
                    _errorCounts[errorType]++;
                }
                else
                {
                    _errorCounts[errorType] = 1;
                }
            }
        }
        
        private void RaiseDeviceStatusChanged(RouterDevice device, DeviceStatus oldStatus, DeviceStatus newStatus)
        {
            try
            {
                DeviceStatusChanged?.Invoke(this, new DeviceStatusChangedEventArgs
                {
                    Device = device,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                log.Error("Error in device status changed event handler", ex);
            }
        }
        
        private void RaiseDeviceMonitoringCompleted(RouterDevice device, MonitoringType type, bool success, TimeSpan duration)
        {
            try
            {
                DeviceMonitoringCompleted?.Invoke(this, new DeviceMonitoringCompletedEventArgs
                {
                    Device = device,
                    Type = type,
                    Timestamp = DateTime.Now,
                    Success = success,
                    Duration = duration
                });
            }
            catch (Exception ex)
            {
                log.Error("Error in device monitoring completed event handler", ex);
            }
        }
        
        private void RaiseMonitoringError(RouterDevice device, MonitoringType type, string errorMessage, Exception exception)
        {
            try
            {
                MonitoringError?.Invoke(this, new MonitoringErrorEventArgs
                {
                    Device = device,
                    Type = type,
                    ErrorMessage = errorMessage,
                    Exception = exception,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                log.Error("Error in monitoring error event handler", ex);
            }
        }
        
        #region Monitoring Methods
        
        private async Task<bool> MonitorResourcesAsync(RouterDevice device)
        {
            try
            {
                if (device.ConnectionType == ConnectionType.Api)
                {
                    // Get resource usage via RouterOS API
                    var usage = await _routerApiService.GetResourceUsageAsync(device);
                    
                    if (usage != null)
                    {
                        // Add to history
                        device.ResourceHistory.Add(usage);
                        
                        // Trim history if needed
                        var maxHistoryPoints = _configuration.GetValue<int>("MonitoringSettings:MaxHistoryPoints", 300);
                        while (device.ResourceHistory.Count > maxHistoryPoints)
                        {
                            device.ResourceHistory.RemoveAt(0);
                        }
                        
                        // Update device properties
                        device.CpuUsage = usage.CpuUsage;
                        device.MemoryUsage = usage.MemoryUsage;
                        device.DiskUsage = usage.DiskUsage;
                        device.Temperature = usage.Temperature;
                        device.Voltage = usage.Voltage;
                        device.LastUpdated = usage.Timestamp;
                        
                        // Check thresholds and update status if needed
                        var oldStatus = device.Status;
                        var newStatus = oldStatus;
                        
                        var cpuThreshold = _configuration.GetValue<double>("MonitoringSettings:CpuAlertThreshold", 90);
                        var memoryThreshold = _configuration.GetValue<double>("MonitoringSettings:MemoryAlertThreshold", 90);
                        var diskThreshold = _configuration.GetValue<double>("MonitoringSettings:DiskAlertThreshold", 90);
                        
                        if (device.CpuUsage > cpuThreshold || 
                            device.MemoryUsage > memoryThreshold || 
                            device.DiskUsage > diskThreshold)
                        {
                            newStatus = DeviceStatus.Warning;
                        }
                        else if (oldStatus == DeviceStatus.Warning)
                        {
                            newStatus = DeviceStatus.Online;
                        }
                        
                        if (oldStatus != newStatus)
                        {
                            device.Status = newStatus;
                            RaiseDeviceStatusChanged(device, oldStatus, newStatus);
                        }
                        
                        return true;
                    }
                }
                else if (device.ConnectionType == ConnectionType.Snmp)
                {
                    // Get resource usage via SNMP
                    var usage = await _snmpService.GetResourceUsageAsync(device);
                    
                    if (usage != null)
                    {
                        // Add to history
                        device.ResourceHistory.Add(usage);
                        
                        // Trim history if needed
                        var maxHistoryPoints = _configuration.GetValue<int>("MonitoringSettings:MaxHistoryPoints", 300);
                        while (device.ResourceHistory.Count > maxHistoryPoints)
                        {
                            device.ResourceHistory.RemoveAt(0);
                        }
                        
                        // Update device properties
                        device.CpuUsage = usage.CpuUsage;
                        device.MemoryUsage = usage.MemoryUsage;
                        device.DiskUsage = usage.DiskUsage;
                        device.Temperature = usage.Temperature;
                        device.Voltage = usage.Voltage;
                        device.LastUpdated = usage.Timestamp;
                        
                        // Check thresholds and update status if needed
                        var oldStatus = device.Status;
                        var newStatus = oldStatus;
                        
                        var cpuThreshold = _configuration.GetValue<double>("MonitoringSettings:CpuAlertThreshold", 90);
                        var memoryThreshold = _configuration.GetValue<double>("MonitoringSettings:MemoryAlertThreshold", 90);
                        var diskThreshold = _configuration.GetValue<double>("MonitoringSettings:DiskAlertThreshold", 90);
                        
                        if (device.CpuUsage > cpuThreshold || 
                            device.MemoryUsage > memoryThreshold || 
                            device.DiskUsage > diskThreshold)
                        {
                            newStatus = DeviceStatus.Warning;
                        }
                        else if (oldStatus == DeviceStatus.Warning)
                        {
                            newStatus = DeviceStatus.Online;
                        }
                        
                        if (oldStatus != newStatus)
                        {
                            device.Status = newStatus;
                            RaiseDeviceStatusChanged(device, oldStatus, newStatus);
                        }
                        
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error monitoring resources for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        private async Task<bool> MonitorInterfacesAsync(RouterDevice device)
        {
            try
            {
                if (device.ConnectionType == ConnectionType.Api)
                {
                    // Get interfaces via RouterOS API
                    var interfaces = await _routerApiService.GetInterfacesAsync(device);
                    
                    if (interfaces != null)
                    {
                        // Update interface collection
                        device.Interfaces.Clear();
                        foreach (var iface in interfaces)
                        {
                            device.Interfaces.Add(iface);
                        }
                        
                        device.LastUpdated = DateTime.Now;
                        return true;
                    }
                }
                else if (device.ConnectionType == ConnectionType.Snmp)
                {
                    // Get interfaces via SNMP
                    var interfaces = await _snmpService.GetInterfacesAsync(device);
                    
                    if (interfaces != null)
                    {
                        // Update interface collection
                        device.Interfaces.Clear();
                        foreach (var iface in interfaces)
                        {
                            device.Interfaces.Add(iface);
                        }
                        
                        device.LastUpdated = DateTime.Now;
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error monitoring interfaces for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        private async Task<bool> MonitorDhcpAsync(RouterDevice device)
        {
            try
            {
                if (device.ConnectionType == ConnectionType.Api)
                {
                    // Get DHCP leases via RouterOS API
                    var leases = await _routerApiService.GetDhcpLeasesAsync(device);
                    
                    if (leases != null)
                    {
                        // Update DHCP leases collection
                        device.DhcpLeases.Clear();
                        foreach (var lease in leases)
                        {
                            device.DhcpLeases.Add(lease);
                        }
                        
                        device.LastUpdated = DateTime.Now;
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error monitoring DHCP leases for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        private async Task<bool> MonitorLogsAsync(RouterDevice device)
        {
            try
            {
                if (device.ConnectionType == ConnectionType.Api)
                {
                    // Get logs via RouterOS API
                    var limit = _configuration.GetValue<int>("AppSettings:DefaultLogLimit", 100);
                    var logs = await _routerApiService.GetLogsAsync(device, limit);
                    
                    if (logs != null)
                    {
                        // Update logs collection
                        device.Logs.Clear();
                        foreach (var logEntry in logs)
                        {
                            device.Logs.Add(logEntry);
                        }
                        
                        device.LastUpdated = DateTime.Now;
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error monitoring logs for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        private async Task<bool> CheckConnectivityAsync(RouterDevice device)
        {
            try
            {
                var oldStatus = device.Status;
                var connected = false;
                
                switch (device.ConnectionType)
                {
                    case ConnectionType.Api:
                        connected = await _routerApiService.TestConnectionAsync(device);
                        break;
                    case ConnectionType.Snmp:
                        connected = await _snmpService.TestConnectionAsync(device);
                        break;
                    // Other connection types would go here
                }
                
                if (connected)
                {
                    if (oldStatus == DeviceStatus.Offline || oldStatus == DeviceStatus.Unknown)
                    {
                        var newStatus = DeviceStatus.Online;
                        device.Status = newStatus;
                        device.LastSeenOnline = DateTime.Now;
                        
                        RaiseDeviceStatusChanged(device, oldStatus, newStatus);
                    }
                }
                else
                {
                    if (oldStatus == DeviceStatus.Online || oldStatus == DeviceStatus.Warning)
                    {
                        var newStatus = DeviceStatus.Offline;
                        device.Status = newStatus;
                        
                        RaiseDeviceStatusChanged(device, oldStatus, newStatus);
                    }
                }
                
                return connected;
            }
            catch (Exception ex)
            {
                log.Error($"Error checking connectivity for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        private async Task<bool> UpdateCloudAsync(RouterDevice device)
        {
            try
            {
                if (device.IsCloudManaged && !string.IsNullOrEmpty(device.CloudId))
                {
                    // Check if authenticated
                    if (!await _cloudService.IsAuthenticatedAsync())
                    {
                        log.Warn("Not authenticated to MikroTik Cloud, skipping cloud update");
                        return false;
                    }
                    
                    // Get cloud device
                    var cloudDevice = await _cloudService.GetDeviceAsync(device.CloudId);
                    
                    if (cloudDevice != null)
                    {
                        // Update cloud device if needed
                        // For now, we're just checking if it's up-to-date
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating cloud for device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        #endregion
    }
}