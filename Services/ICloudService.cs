using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface ICloudService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<CloudAuthResult> AuthenticateAsync(string username, string password);
        Task<bool> RefreshTokenAsync();
        Task LogoutAsync();
        
        Task<List<CloudSite>> GetSitesAsync();
        Task<CloudSite> GetSiteAsync(string siteId);
        Task<CloudSite> CreateSiteAsync(CloudSite site);
        Task<CloudSite> UpdateSiteAsync(CloudSite site);
        Task<bool> DeleteSiteAsync(string siteId);
        
        Task<List<CloudDevice>> GetDevicesAsync(string siteId = null);
        Task<CloudDevice> GetDeviceAsync(string deviceId);
        Task<bool> UpdateDeviceAsync(CloudDevice device);
        
        Task<bool> SyncDevicesAsync(CloudSite site);
        Task<bool> SyncAllSitesAsync();
        
        Task<bool> ImportDeviceAsync(RouterDevice device, CloudSite site);
        Task<RouterDevice> ExportDeviceAsync(CloudDevice cloudDevice);
        
        Task<List<CloudVpnUser>> GetVpnUsersAsync(string deviceId);
        Task<CloudVpnUser> CreateVpnUserAsync(string deviceId, CloudVpnUser user);
        Task<bool> UpdateVpnUserAsync(string deviceId, CloudVpnUser user);
        Task<bool> DeleteVpnUserAsync(string deviceId, string userId);
        
        Task<bool> EstablishVpnConnectionAsync(string deviceId);
        Task<bool> DisconnectVpnAsync(string deviceId);
        Task<bool> IsVpnConnectedAsync(string deviceId);
        
        Task<string> GetCloudApiUrlAsync();
        Task<CloudServiceStatus> GetCloudServiceStatusAsync();
    }
    
    public class CloudAuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
    
    public class CloudServiceStatus
    {
        public bool IsAvailable { get; set; }
        public string ApiVersion { get; set; }
        public DateTime LastChecked { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}