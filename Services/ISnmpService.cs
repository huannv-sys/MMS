using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface ISnmpService
    {
        Task<bool> TestConnectionAsync(RouterDevice device);
        
        Task<RouterDevice> GetSystemInfoAsync(RouterDevice device);
        Task<IEnumerable<NetworkInterface>> GetInterfacesAsync(RouterDevice device);
        Task<ResourceUsage> GetResourceUsageAsync(RouterDevice device);
        
        Task<InterfaceTraffic> GetInterfaceTrafficAsync(RouterDevice device, string interfaceName);
        
        Task<Dictionary<string, string>> GetDeviceOidsAsync(RouterDevice device);
    }
}