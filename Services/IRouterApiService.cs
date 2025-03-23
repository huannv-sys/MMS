using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface IRouterApiService
    {
        Task<bool> ConnectAsync(RouterDevice device);
        Task DisconnectAsync(RouterDevice device);
        Task<bool> TestConnectionAsync(RouterDevice device);
        
        Task<RouterDevice> GetSystemInfoAsync(RouterDevice device);
        Task<IEnumerable<NetworkInterface>> GetInterfacesAsync(RouterDevice device);
        Task<IEnumerable<DhcpLease>> GetDhcpLeasesAsync(RouterDevice device);
        Task<IEnumerable<LogEntry>> GetLogsAsync(RouterDevice device, int limit = 100);
        Task<IEnumerable<FirewallRule>> GetFirewallRulesAsync(RouterDevice device);
        Task<IEnumerable<VpnTunnel>> GetVpnTunnelsAsync(RouterDevice device);
        Task<IEnumerable<QosRule>> GetQosRulesAsync(RouterDevice device);
        Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(RouterDevice device);
        
        Task<ResourceUsage> GetResourceUsageAsync(RouterDevice device);
        Task<InterfaceTraffic> GetInterfaceTrafficAsync(RouterDevice device, string interfaceId);
        
        Task<bool> EnableInterfaceAsync(RouterDevice device, string interfaceId);
        Task<bool> DisableInterfaceAsync(RouterDevice device, string interfaceId);
        
        Task<bool> EnableFirewallRuleAsync(RouterDevice device, string ruleId);
        Task<bool> DisableFirewallRuleAsync(RouterDevice device, string ruleId);
        
        Task<bool> RebootDeviceAsync(RouterDevice device);
        Task<bool> ShutdownDeviceAsync(RouterDevice device);
        
        Task<bool> BackupConfigurationAsync(RouterDevice device, string backupPath);
        Task<bool> RestoreConfigurationAsync(RouterDevice device, string backupPath);
        
        Task<string> ExecuteCommandAsync(RouterDevice device, string command);
    }
}