using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MikroTikMonitor.Models;
using Timer = System.Timers.Timer;

namespace MikroTikMonitor.Services
{
    /// <summary>
    /// Background worker service for collecting data from routers at regular intervals
    /// </summary>
    public class Worker : IDisposable
    {
        private readonly RouterApiService _routerApiService;
        private readonly SnmpService _snmpService;
        private readonly StatisticsService _statisticsService;
        private readonly CloudService _cloudService;
        private readonly List<RouterDevice> _routers;
        private readonly IConfiguration _configuration;
        private Timer _dataCollectionTimer;
        private Timer _resourceUsageTimer;
        private Timer _interfaceMonitorTimer;
        private Timer _cloudUpdateTimer;
        private Timer _connectivityCheckTimer;
        private Dictionary<string, RouterMonitoringInfo> _monitoringInfo = new Dictionary<string, RouterMonitoringInfo>();
        private bool _isDisposed;
        
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Worker));

        /// <summary>
        /// Initializes a new instance of the Worker class
        /// </summary>
        /// <param name="routerApiService">The router API service</param>
        /// <param name="snmpService">The SNMP service</param>
        /// <param name="statisticsService">The statistics service</param>
        /// <param name="cloudService">The cloud service</param>
        /// <param name="routers">The list of routers to monitor</param>
        /// <param name="configuration">The configuration</param>
        public Worker(
            RouterApiService routerApiService,
            SnmpService snmpService,
            StatisticsService statisticsService,
            CloudService cloudService,
            List<RouterDevice> routers,
            IConfiguration configuration)
        {
            _routerApiService = routerApiService ?? throw new ArgumentNullException(nameof(routerApiService));
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
            _routers = routers ?? throw new ArgumentNullException(nameof(routers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Initialize timers (but don't start them yet)
            _dataCollectionTimer = new Timer(CollectDataTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _resourceUsageTimer = new Timer(ResourceUsageTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _interfaceMonitorTimer = new Timer(InterfaceMonitorTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _cloudUpdateTimer = new Timer(CloudUpdateTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _connectivityCheckTimer = new Timer(ConnectivityCheckTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            
            // Initialize monitoring info for each router
            InitializeMonitoringInfo();
        }

        /// <summary>
        /// Initialize monitoring information for each router
        /// </summary>
        private void InitializeMonitoringInfo()
        {
            foreach (var router in _routers)
            {
                if (!_monitoringInfo.ContainsKey(router.Id))
                {
                    _monitoringInfo[router.Id] = new RouterMonitoringInfo
                    {
                        LastDataCollection = DateTime.MinValue,
                        LastResourceUpdate = DateTime.MinValue,
                        LastInterfaceUpdate = DateTime.MinValue,
                        NextDataCollection = DateTime.Now,
                        NextResourceUpdate = DateTime.Now,
                        NextInterfaceUpdate = DateTime.Now,
                        ConnectionAttempts = 0,
                        IsBeingProcessed = false
                    };
                }
            }
        }

        /// <summary>
        /// Starts the worker service
        /// </summary>
        public void Start()
        {
            _log.Info("Starting MikroTikMonitor Worker");
            
            // Get configuration values
            int dataCollectionInterval = _configuration.GetValue<int>("MonitoringSettings:DefaultDataCollectionInterval", 30);
            int resourceMonitoringInterval = _configuration.GetValue<int>("MonitoringSettings:ResourceMonitoringInterval", 5);
            int interfaceMonitoringInterval = _configuration.GetValue<int>("MonitoringSettings:InterfaceMonitoringInterval", 1);
            int cloudUpdateInterval = _configuration.GetValue<int>("MonitoringSettings:CloudUpdateInterval", 60);
            int connectivityCheckInterval = _configuration.GetValue<int>("MonitoringSettings:ConnectivityCheckInterval", 15);
            
            // Start data collection timer
            _dataCollectionTimer.Change(1000, dataCollectionInterval * 1000);
            
            // Start resource usage timer
            _resourceUsageTimer.Change(2000, resourceMonitoringInterval * 1000);
            
            // Start interface monitor timer
            _interfaceMonitorTimer.Change(3000, interfaceMonitoringInterval * 1000);
            
            // Start cloud update timer
            _cloudUpdateTimer.Change(5000, cloudUpdateInterval * 1000);
            
            // Start connectivity check timer
            _connectivityCheckTimer.Change(10000, connectivityCheckInterval * 1000);
            
            _log.Info($"Worker started with monitoring intervals - Data: {dataCollectionInterval}s, Resource: {resourceMonitoringInterval}s, Interface: {interfaceMonitoringInterval}s, Cloud: {cloudUpdateInterval}s, Connectivity: {connectivityCheckInterval}s");
        }
        
        /// <summary>
        /// Stops the worker service
        /// </summary>
        public void Stop()
        {
            _log.Info("Stopping MikroTikMonitor Worker");
            
            _dataCollectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _resourceUsageTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _interfaceMonitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _cloudUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _connectivityCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            _log.Info("Worker stopped");
        }
        
        /// <summary>
        /// Callback for the data collection timer
        /// </summary>
        private async void CollectDataTimerCallback(object state)
        {
            try
            {
                // Get routers to process
                var routersToProcess = GetRoutersForProcessing(r => 
                    r.IsMonitored && 
                    r.IsConnected && 
                    GetMonitoringInfo(r.Id).NextDataCollection <= DateTime.Now);
                
                if (routersToProcess.Count == 0)
                    return;
                
                _log.Info($"Collecting data for {routersToProcess.Count} router(s)");
                
                foreach (var router in routersToProcess)
                {
                    try
                    {
                        // Mark as being processed
                        var info = GetMonitoringInfo(router.Id);
                        info.IsBeingProcessed = true;
                        
                        // Collect system information
                        await _routerApiService.GetSystemInfoAsync(router);
                        
                        // Collect DHCP leases
                        await _routerApiService.GetDhcpLeasesAsync(router);
                        
                        // Collect wireless clients
                        await _routerApiService.GetWirelessClientsAsync(router);
                        
                        // Get log entries
                        await _routerApiService.GetLogEntriesAsync(router, 100);
                        
                        // Update monitoring info
                        info.LastDataCollection = DateTime.Now;
                        info.NextDataCollection = DateTime.Now.AddSeconds(router.MonitoringInterval > 0 ? router.MonitoringInterval : 30);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error collecting data for router {router.Name} ({router.Id}): {ex.Message}", ex);
                        router.RecordConnectivityEvent(false, $"Data collection error: {ex.Message}");
                    }
                    finally
                    {
                        // Mark as not being processed
                        GetMonitoringInfo(router.Id).IsBeingProcessed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in data collection: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Callback for the resource usage timer
        /// </summary>
        private async void ResourceUsageTimerCallback(object state)
        {
            try
            {
                // Get routers to process
                var routersToProcess = GetRoutersForProcessing(r => 
                    r.IsMonitored && 
                    r.IsConnected && 
                    GetMonitoringInfo(r.Id).NextResourceUpdate <= DateTime.Now);
                
                if (routersToProcess.Count == 0)
                    return;
                
                foreach (var router in routersToProcess)
                {
                    try
                    {
                        // Mark as being processed
                        var info = GetMonitoringInfo(router.Id);
                        info.IsBeingProcessed = true;
                        
                        // Update resource usage (CPU, memory, disk)
                        await _routerApiService.GetResourceUsageAsync(router);
                        
                        // Update via SNMP if configured
                        if (router.UseSnmp)
                        {
                            await _snmpService.GetResourceUsageAsync(router);
                        }
                        
                        // Update statistics
                        _statisticsService.UpdateResourceStatistics(router);
                        
                        // Update monitoring info
                        info.LastResourceUpdate = DateTime.Now;
                        info.NextResourceUpdate = DateTime.Now.AddSeconds(5);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error updating resource usage for router {router.Name} ({router.Id}): {ex.Message}", ex);
                        router.RecordConnectivityEvent(false, $"Resource update error: {ex.Message}");
                    }
                    finally
                    {
                        // Mark as not being processed
                        GetMonitoringInfo(router.Id).IsBeingProcessed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in resource usage monitoring: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Callback for the interface monitor timer
        /// </summary>
        private async void InterfaceMonitorTimerCallback(object state)
        {
            try
            {
                // Get routers to process
                var routersToProcess = GetRoutersForProcessing(r => 
                    r.IsMonitored && 
                    r.IsConnected && 
                    GetMonitoringInfo(r.Id).NextInterfaceUpdate <= DateTime.Now);
                
                if (routersToProcess.Count == 0)
                    return;
                
                foreach (var router in routersToProcess)
                {
                    try
                    {
                        // Mark as being processed
                        var info = GetMonitoringInfo(router.Id);
                        info.IsBeingProcessed = true;
                        
                        // Get current network interface statistics
                        await _routerApiService.GetNetworkInterfacesAsync(router);
                        
                        // Calculate bandwidth for interfaces
                        _statisticsService.UpdateInterfaceStatistics(router);
                        
                        // Update traffic charts
                        _statisticsService.UpdateTrafficCharts(router);
                        
                        // Update monitoring info
                        info.LastInterfaceUpdate = DateTime.Now;
                        info.NextInterfaceUpdate = DateTime.Now.AddSeconds(1);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error updating interfaces for router {router.Name} ({router.Id}): {ex.Message}", ex);
                        router.RecordConnectivityEvent(false, $"Interface update error: {ex.Message}");
                    }
                    finally
                    {
                        // Mark as not being processed
                        GetMonitoringInfo(router.Id).IsBeingProcessed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in interface monitoring: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Callback for the cloud update timer
        /// </summary>
        private async void CloudUpdateTimerCallback(object state)
        {
            try
            {
                // Get cloud-enabled routers
                var cloudRouters = _routers.Where(r => r.IsMonitored && r.UseCloud && !string.IsNullOrEmpty(r.CloudId)).ToList();
                
                if (cloudRouters.Count == 0)
                    return;
                
                _log.Info($"Updating cloud information for {cloudRouters.Count} router(s)");
                
                foreach (var router in cloudRouters)
                {
                    try
                    {
                        await _cloudService.UpdateRouterFromCloudAsync(router);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error updating cloud information for router {router.Name} ({router.Id}): {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in cloud update: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Callback for the connectivity check timer
        /// </summary>
        private async void ConnectivityCheckTimerCallback(object state)
        {
            try
            {
                // Find disconnected routers that should be connected
                var disconnectedRouters = _routers.Where(r => 
                    r.IsMonitored && 
                    !r.IsConnected && 
                    r.AutoReconnect && 
                    GetMonitoringInfo(r.Id).ConnectionAttempts < 3 &&
                    r.HasValidConnectionConfig &&
                    !GetMonitoringInfo(r.Id).IsBeingProcessed).ToList();
                
                if (disconnectedRouters.Count == 0)
                    return;
                
                _log.Info($"Attempting to connect to {disconnectedRouters.Count} disconnected router(s)");
                
                foreach (var router in disconnectedRouters)
                {
                    try
                    {
                        // Mark as being processed
                        var info = GetMonitoringInfo(router.Id);
                        info.IsBeingProcessed = true;
                        
                        // Check if we need to establish a VPN connection
                        if (router.UseCloud && router.IsVpnEnabled && string.IsNullOrEmpty(router.VpnConnectionId))
                        {
                            var vpnConnection = await _cloudService.EstablishVpnConnectionAsync(router.CloudId);
                            if (vpnConnection != null)
                            {
                                router.VpnConnectionId = vpnConnection.ConnectionId;
                                router.VpnIpAddress = vpnConnection.VpnIpAddress;
                                _log.Info($"Established VPN connection to router {router.Name}: {vpnConnection.VpnIpAddress}");
                            }
                        }
                        
                        // Attempt to connect
                        _log.Info($"Attempting to connect to router {router.Name} ({router.Id})");
                        bool success = await _routerApiService.ConnectAsync(router);
                        
                        if (success)
                        {
                            _log.Info($"Successfully connected to router {router.Name} ({router.Id})");
                            info.ConnectionAttempts = 0;
                            router.RecordConnectivityEvent(true, "Connected successfully");
                        }
                        else
                        {
                            _log.Warn($"Failed to connect to router {router.Name} ({router.Id}): {router.ConnectionStatus}");
                            info.ConnectionAttempts++;
                            router.RecordConnectivityEvent(false, router.ConnectionStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error checking connectivity for router {router.Name} ({router.Id}): {ex.Message}", ex);
                        GetMonitoringInfo(router.Id).ConnectionAttempts++;
                        router.RecordConnectivityEvent(false, $"Connection error: {ex.Message}");
                    }
                    finally
                    {
                        // Mark as not being processed
                        GetMonitoringInfo(router.Id).IsBeingProcessed = false;
                    }
                }
                
                // Reset connection attempts counter for routers that have been offline for a while
                foreach (var info in _monitoringInfo.Values.Where(i => i.ConnectionAttempts >= 3))
                {
                    DateTime lastReset = info.LastConnectionAttemptsReset;
                    if ((DateTime.Now - lastReset).TotalMinutes >= 5)
                    {
                        info.ConnectionAttempts = 0;
                        info.LastConnectionAttemptsReset = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in connectivity check: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Get routers for processing based on a filter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>A list of routers to process</returns>
        private List<RouterDevice> GetRoutersForProcessing(Func<RouterDevice, bool> filter)
        {
            // Get all routers that match the filter and are not already being processed
            return _routers.Where(r => filter(r) && !GetMonitoringInfo(r.Id).IsBeingProcessed).ToList();
        }
        
        /// <summary>
        /// Get monitoring information for a router
        /// </summary>
        /// <param name="routerId">The router ID</param>
        /// <returns>The monitoring information</returns>
        private RouterMonitoringInfo GetMonitoringInfo(string routerId)
        {
            if (!_monitoringInfo.TryGetValue(routerId, out var info))
            {
                info = new RouterMonitoringInfo
                {
                    LastDataCollection = DateTime.MinValue,
                    LastResourceUpdate = DateTime.MinValue,
                    LastInterfaceUpdate = DateTime.MinValue,
                    NextDataCollection = DateTime.Now,
                    NextResourceUpdate = DateTime.Now,
                    NextInterfaceUpdate = DateTime.Now,
                    ConnectionAttempts = 0,
                    IsBeingProcessed = false
                };
                
                _monitoringInfo[routerId] = info;
            }
            
            return info;
        }
        
        /// <summary>
        /// Add a router to monitor
        /// </summary>
        /// <param name="router">The router to add</param>
        public void AddRouter(RouterDevice router)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
                
            if (!_routers.Contains(router))
                _routers.Add(router);
                
            if (!_monitoringInfo.ContainsKey(router.Id))
            {
                _monitoringInfo[router.Id] = new RouterMonitoringInfo
                {
                    LastDataCollection = DateTime.MinValue,
                    LastResourceUpdate = DateTime.MinValue,
                    LastInterfaceUpdate = DateTime.MinValue,
                    NextDataCollection = DateTime.Now,
                    NextResourceUpdate = DateTime.Now,
                    NextInterfaceUpdate = DateTime.Now,
                    ConnectionAttempts = 0,
                    IsBeingProcessed = false
                };
            }
        }
        
        /// <summary>
        /// Remove a router from monitoring
        /// </summary>
        /// <param name="router">The router to remove</param>
        public void RemoveRouter(RouterDevice router)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
                
            if (_routers.Contains(router))
            {
                if (router.IsConnected)
                    _routerApiService.Disconnect(router);
                    
                _routers.Remove(router);
            }
            
            if (_monitoringInfo.ContainsKey(router.Id))
                _monitoringInfo.Remove(router.Id);
        }
        
        /// <summary>
        /// Disposes of resources used by the worker
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes of resources used by the worker
        /// </summary>
        /// <param name="disposing">Whether this is being called from Dispose()</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
                
            if (disposing)
            {
                Stop();
                
                _dataCollectionTimer?.Dispose();
                _resourceUsageTimer?.Dispose();
                _interfaceMonitorTimer?.Dispose();
                _cloudUpdateTimer?.Dispose();
                _connectivityCheckTimer?.Dispose();
            }
            
            _dataCollectionTimer = null;
            _resourceUsageTimer = null;
            _interfaceMonitorTimer = null;
            _cloudUpdateTimer = null;
            _connectivityCheckTimer = null;
            _isDisposed = true;
        }
    }
    
    /// <summary>
    /// Information about router monitoring
    /// </summary>
    public class RouterMonitoringInfo
    {
        /// <summary>
        /// Gets or sets when data was last collected
        /// </summary>
        public DateTime LastDataCollection { get; set; }
        
        /// <summary>
        /// Gets or sets when resource usage was last updated
        /// </summary>
        public DateTime LastResourceUpdate { get; set; }
        
        /// <summary>
        /// Gets or sets when interfaces were last updated
        /// </summary>
        public DateTime LastInterfaceUpdate { get; set; }
        
        /// <summary>
        /// Gets or sets when data should next be collected
        /// </summary>
        public DateTime NextDataCollection { get; set; }
        
        /// <summary>
        /// Gets or sets when resource usage should next be updated
        /// </summary>
        public DateTime NextResourceUpdate { get; set; }
        
        /// <summary>
        /// Gets or sets when interfaces should next be updated
        /// </summary>
        public DateTime NextInterfaceUpdate { get; set; }
        
        /// <summary>
        /// Gets or sets the number of connection attempts
        /// </summary>
        public int ConnectionAttempts { get; set; }
        
        /// <summary>
        /// Gets or sets when connection attempts were last reset
        /// </summary>
        public DateTime LastConnectionAttemptsReset { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets whether the router is currently being processed
        /// </summary>
        public bool IsBeingProcessed { get; set; }
    }
}