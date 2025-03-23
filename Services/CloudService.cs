using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using MikroTikMonitor.Models;
using log4net;

namespace MikroTikMonitor.Services
{
    public class CloudService : ICloudService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CloudService));
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        private CloudAuthResult _authInfo;
        
        public CloudService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            
            var cloudApiUrl = _configuration.GetValue<string>("CloudSettings:CloudApiUrl");
            if (!string.IsNullOrEmpty(cloudApiUrl))
            {
                _httpClient.BaseAddress = new Uri(cloudApiUrl);
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://api.mikrotik.cloud/v1");
            }
            
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<bool> IsAuthenticatedAsync()
        {
            return _authInfo != null && _authInfo.Success && _authInfo.TokenExpiry > DateTime.Now;
        }
        
        public async Task<CloudAuthResult> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (await IsAuthenticatedAsync())
                {
                    log.Info("Already authenticated to MikroTik Cloud");
                    return _authInfo;
                }
                
                log.Info($"Authenticating to MikroTik Cloud as {username}");
                
                var authData = new 
                {
                    username,
                    password
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(authData),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync("/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var authResult = JsonConvert.DeserializeObject<CloudAuthResult>(responseData);
                    
                    if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                    {
                        authResult.Success = true;
                        _authInfo = authResult;
                        
                        // Add authorization header for subsequent requests
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", authResult.Token);
                        
                        log.Info("Successfully authenticated to MikroTik Cloud");
                        
                        return authResult;
                    }
                }
                
                log.Error($"Failed to authenticate to MikroTik Cloud: {response.StatusCode}");
                
                return new CloudAuthResult
                {
                    Success = false,
                    ErrorMessage = $"Authentication failed with status code: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                log.Error($"Error authenticating to MikroTik Cloud: {ex.Message}", ex);
                
                return new CloudAuthResult
                {
                    Success = false,
                    ErrorMessage = $"Authentication error: {ex.Message}"
                };
            }
        }
        
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (_authInfo == null || string.IsNullOrEmpty(_authInfo.RefreshToken))
                {
                    log.Error("No refresh token available. Please authenticate first.");
                    return false;
                }
                
                log.Info("Refreshing MikroTik Cloud authentication token");
                
                var refreshData = new 
                {
                    refreshToken = _authInfo.RefreshToken
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(refreshData),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync("/auth/refresh", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var authResult = JsonConvert.DeserializeObject<CloudAuthResult>(responseData);
                    
                    if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                    {
                        authResult.Success = true;
                        _authInfo = authResult;
                        
                        // Update authorization header
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", authResult.Token);
                        
                        log.Info("Successfully refreshed MikroTik Cloud authentication token");
                        
                        return true;
                    }
                }
                
                log.Error($"Failed to refresh MikroTik Cloud token: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error refreshing MikroTik Cloud token: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task LogoutAsync()
        {
            try
            {
                if (_authInfo == null)
                {
                    return;
                }
                
                log.Info("Logging out from MikroTik Cloud");
                
                var response = await _httpClient.PostAsync("/auth/logout", null);
                
                // Clear auth info regardless of response
                _authInfo = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                
                log.Info("Successfully logged out from MikroTik Cloud");
            }
            catch (Exception ex)
            {
                log.Error($"Error logging out from MikroTik Cloud: {ex.Message}", ex);
                
                // Clear auth info anyway
                _authInfo = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
        
        public async Task<List<CloudSite>> GetSitesAsync()
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return new List<CloudSite>();
                }
                
                log.Info("Getting sites from MikroTik Cloud");
                
                var response = await _httpClient.GetAsync("/sites");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var sitesDtos = JsonConvert.DeserializeObject<List<CloudSiteDto>>(responseData);
                    
                    if (sitesDtos == null)
                    {
                        return new List<CloudSite>();
                    }
                    
                    var sites = sitesDtos.Select(dto => CloudSite.FromDto(dto)).ToList();
                    
                    log.Info($"Successfully retrieved {sites.Count} sites from MikroTik Cloud");
                    
                    return sites;
                }
                
                log.Error($"Failed to get sites from MikroTik Cloud: {response.StatusCode}");
                return new List<CloudSite>();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting sites from MikroTik Cloud: {ex.Message}", ex);
                return new List<CloudSite>();
            }
        }
        
        public async Task<CloudSite> GetSiteAsync(string siteId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return null;
                }
                
                if (string.IsNullOrEmpty(siteId))
                {
                    log.Error("Site ID cannot be null or empty");
                    return null;
                }
                
                log.Info($"Getting site {siteId} from MikroTik Cloud");
                
                var response = await _httpClient.GetAsync($"/sites/{siteId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var siteDto = JsonConvert.DeserializeObject<CloudSiteDto>(responseData);
                    
                    if (siteDto == null)
                    {
                        return null;
                    }
                    
                    var site = CloudSite.FromDto(siteDto);
                    
                    log.Info($"Successfully retrieved site {siteId} from MikroTik Cloud");
                    
                    return site;
                }
                
                log.Error($"Failed to get site {siteId} from MikroTik Cloud: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting site {siteId} from MikroTik Cloud: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<CloudSite> CreateSiteAsync(CloudSite site)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return null;
                }
                
                if (site == null)
                {
                    log.Error("Site cannot be null");
                    return null;
                }
                
                log.Info($"Creating site {site.Name} in MikroTik Cloud");
                
                // Convert to DTO for serialization
                var siteDto = new CloudSiteDto
                {
                    Id = site.Id,
                    Name = site.Name,
                    Description = site.Description,
                    Location = site.Location,
                    ContactName = site.ContactName,
                    ContactEmail = site.ContactEmail,
                    ContactPhone = site.ContactPhone,
                    Address = site.Address,
                    City = site.City,
                    State = site.State,
                    PostalCode = site.PostalCode,
                    Country = site.Country,
                    CreatedAt = site.CreatedAt,
                    UpdatedAt = site.UpdatedAt,
                    Status = site.Status,
                    IsActive = site.IsActive,
                    Tags = site.Tags,
                    Notes = site.Notes
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(siteDto),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync("/sites", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var createdSiteDto = JsonConvert.DeserializeObject<CloudSiteDto>(responseData);
                    
                    if (createdSiteDto == null)
                    {
                        return null;
                    }
                    
                    var createdSite = CloudSite.FromDto(createdSiteDto);
                    
                    log.Info($"Successfully created site {createdSite.Id} in MikroTik Cloud");
                    
                    return createdSite;
                }
                
                log.Error($"Failed to create site in MikroTik Cloud: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error($"Error creating site in MikroTik Cloud: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<CloudSite> UpdateSiteAsync(CloudSite site)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return null;
                }
                
                if (site == null || string.IsNullOrEmpty(site.Id))
                {
                    log.Error("Site cannot be null and must have an ID");
                    return null;
                }
                
                log.Info($"Updating site {site.Id} in MikroTik Cloud");
                
                // Convert to DTO for serialization
                var siteDto = new CloudSiteDto
                {
                    Id = site.Id,
                    Name = site.Name,
                    Description = site.Description,
                    Location = site.Location,
                    ContactName = site.ContactName,
                    ContactEmail = site.ContactEmail,
                    ContactPhone = site.ContactPhone,
                    Address = site.Address,
                    City = site.City,
                    State = site.State,
                    PostalCode = site.PostalCode,
                    Country = site.Country,
                    CreatedAt = site.CreatedAt,
                    UpdatedAt = site.UpdatedAt,
                    Status = site.Status,
                    IsActive = site.IsActive,
                    Tags = site.Tags,
                    Notes = site.Notes
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(siteDto),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"/sites/{site.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var updatedSiteDto = JsonConvert.DeserializeObject<CloudSiteDto>(responseData);
                    
                    if (updatedSiteDto == null)
                    {
                        return null;
                    }
                    
                    var updatedSite = CloudSite.FromDto(updatedSiteDto);
                    
                    log.Info($"Successfully updated site {site.Id} in MikroTik Cloud");
                    
                    return updatedSite;
                }
                
                log.Error($"Failed to update site {site.Id} in MikroTik Cloud: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating site {site.Id} in MikroTik Cloud: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<bool> DeleteSiteAsync(string siteId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(siteId))
                {
                    log.Error("Site ID cannot be null or empty");
                    return false;
                }
                
                log.Info($"Deleting site {siteId} from MikroTik Cloud");
                
                var response = await _httpClient.DeleteAsync($"/sites/{siteId}");
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully deleted site {siteId} from MikroTik Cloud");
                    return true;
                }
                
                log.Error($"Failed to delete site {siteId} from MikroTik Cloud: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error deleting site {siteId} from MikroTik Cloud: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<List<CloudDevice>> GetDevicesAsync(string siteId = null)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return new List<CloudDevice>();
                }
                
                string endpoint = string.IsNullOrEmpty(siteId) ? "/devices" : $"/sites/{siteId}/devices";
                
                log.Info($"Getting devices from MikroTik Cloud{(string.IsNullOrEmpty(siteId) ? "" : $" for site {siteId}")}");
                
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var deviceDtos = JsonConvert.DeserializeObject<List<CloudDeviceDto>>(responseData);
                    
                    if (deviceDtos == null)
                    {
                        return new List<CloudDevice>();
                    }
                    
                    var devices = deviceDtos.Select(dto => CloudDevice.FromDto(dto)).ToList();
                    
                    log.Info($"Successfully retrieved {devices.Count} devices from MikroTik Cloud");
                    
                    return devices;
                }
                
                log.Error($"Failed to get devices from MikroTik Cloud: {response.StatusCode}");
                return new List<CloudDevice>();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting devices from MikroTik Cloud: {ex.Message}", ex);
                return new List<CloudDevice>();
            }
        }
        
        public async Task<CloudDevice> GetDeviceAsync(string deviceId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return null;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return null;
                }
                
                log.Info($"Getting device {deviceId} from MikroTik Cloud");
                
                var response = await _httpClient.GetAsync($"/devices/{deviceId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var deviceDto = JsonConvert.DeserializeObject<CloudDeviceDto>(responseData);
                    
                    if (deviceDto == null)
                    {
                        return null;
                    }
                    
                    var device = CloudDevice.FromDto(deviceDto);
                    
                    log.Info($"Successfully retrieved device {deviceId} from MikroTik Cloud");
                    
                    return device;
                }
                
                log.Error($"Failed to get device {deviceId} from MikroTik Cloud: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting device {deviceId} from MikroTik Cloud: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<bool> UpdateDeviceAsync(CloudDevice device)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (device == null || string.IsNullOrEmpty(device.Id))
                {
                    log.Error("Device cannot be null and must have an ID");
                    return false;
                }
                
                log.Info($"Updating device {device.Id} in MikroTik Cloud");
                
                // Convert to DTO for serialization
                var deviceDto = new CloudDeviceDto
                {
                    Id = device.Id,
                    Name = device.Name,
                    SerialNumber = device.SerialNumber,
                    MacAddress = device.MacAddress,
                    IpAddress = device.IpAddress,
                    Model = device.Model,
                    BoardName = device.BoardName,
                    FirmwareVersion = device.FirmwareVersion,
                    IsOnline = device.IsOnline,
                    LastSeen = device.LastSeen,
                    SiteId = device.SiteId,
                    SiteName = device.SiteName,
                    OwnerEmail = device.OwnerEmail,
                    CreatedAt = device.CreatedAt,
                    UpdatedAt = device.UpdatedAt,
                    LastConfigBackup = device.LastConfigBackup,
                    LastFirmwareCheck = device.LastFirmwareCheck,
                    IsMonitored = device.IsMonitored,
                    Tags = device.Tags,
                    Notes = device.Notes
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(deviceDto),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"/devices/{device.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully updated device {device.Id} in MikroTik Cloud");
                    return true;
                }
                
                log.Error($"Failed to update device {device.Id} in MikroTik Cloud: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating device {device.Id} in MikroTik Cloud: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> SyncDevicesAsync(CloudSite site)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (site == null || string.IsNullOrEmpty(site.Id))
                {
                    log.Error("Site cannot be null and must have an ID");
                    return false;
                }
                
                log.Info($"Syncing devices for site {site.Id} in MikroTik Cloud");
                
                var response = await _httpClient.PostAsync($"/sites/{site.Id}/sync", null);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully initiated device sync for site {site.Id}");
                    return true;
                }
                
                log.Error($"Failed to sync devices for site {site.Id}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error syncing devices for site {site.Id}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> SyncAllSitesAsync()
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                log.Info("Syncing all sites in MikroTik Cloud");
                
                var response = await _httpClient.PostAsync("/sites/sync-all", null);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info("Successfully initiated sync for all sites");
                    return true;
                }
                
                log.Error($"Failed to sync all sites: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error syncing all sites: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> ImportDeviceAsync(RouterDevice device, CloudSite site)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (device == null)
                {
                    log.Error("Device cannot be null");
                    return false;
                }
                
                if (site == null || string.IsNullOrEmpty(site.Id))
                {
                    log.Error("Site cannot be null and must have an ID");
                    return false;
                }
                
                log.Info($"Importing device {device.Name} to site {site.Id} in MikroTik Cloud");
                
                var importData = new
                {
                    name = device.Name,
                    address = device.IpAddress,
                    username = device.Username,
                    password = device.Password,
                    siteId = site.Id
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(importData),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync("/devices/import", content);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully imported device {device.Name} to MikroTik Cloud");
                    return true;
                }
                
                log.Error($"Failed to import device {device.Name}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error importing device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<RouterDevice> ExportDeviceAsync(CloudDevice cloudDevice)
        {
            try
            {
                if (cloudDevice == null || string.IsNullOrEmpty(cloudDevice.Id))
                {
                    log.Error("Cloud device cannot be null and must have an ID");
                    return null;
                }
                
                // Create router device from cloud device
                var routerDevice = new RouterDevice
                {
                    Name = cloudDevice.Name,
                    IpAddress = cloudDevice.IpAddress,
                    Username = cloudDevice.OwnerEmail,
                    Model = cloudDevice.Model,
                    SerialNumber = cloudDevice.SerialNumber,
                    FirmwareVersion = cloudDevice.FirmwareVersion,
                    CloudId = cloudDevice.Id,
                    IsCloudManaged = true,
                    Status = cloudDevice.IsOnline ? DeviceStatus.Online : DeviceStatus.Offline,
                    LastSeenOnline = cloudDevice.LastSeen,
                    SiteId = cloudDevice.SiteId,
                    Tags = cloudDevice.Tags,
                    Notes = cloudDevice.Notes,
                    IsMonitored = cloudDevice.IsMonitored
                };
                
                log.Info($"Exported cloud device {cloudDevice.Id} to local router device");
                
                return routerDevice;
            }
            catch (Exception ex)
            {
                log.Error($"Error exporting cloud device {cloudDevice.Id}: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<List<CloudVpnUser>> GetVpnUsersAsync(string deviceId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return new List<CloudVpnUser>();
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return new List<CloudVpnUser>();
                }
                
                log.Info($"Getting VPN users for device {deviceId} from MikroTik Cloud");
                
                var response = await _httpClient.GetAsync($"/devices/{deviceId}/vpn-users");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var vpnUserDtos = JsonConvert.DeserializeObject<List<CloudVpnUserDto>>(responseData);
                    
                    if (vpnUserDtos == null)
                    {
                        return new List<CloudVpnUser>();
                    }
                    
                    var vpnUsers = vpnUserDtos.Select(dto => CloudVpnUser.FromDto(dto)).ToList();
                    
                    log.Info($"Successfully retrieved {vpnUsers.Count} VPN users for device {deviceId}");
                    
                    return vpnUsers;
                }
                
                log.Error($"Failed to get VPN users for device {deviceId}: {response.StatusCode}");
                return new List<CloudVpnUser>();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting VPN users for device {deviceId}: {ex.Message}", ex);
                return new List<CloudVpnUser>();
            }
        }
        
        public async Task<CloudVpnUser> CreateVpnUserAsync(string deviceId, CloudVpnUser user)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return null;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return null;
                }
                
                if (user == null)
                {
                    log.Error("VPN user cannot be null");
                    return null;
                }
                
                log.Info($"Creating VPN user {user.Username} for device {deviceId} in MikroTik Cloud");
                
                // Convert to DTO for serialization
                var vpnUserDto = new CloudVpnUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    ExpirationDate = user.ExpirationDate,
                    DeviceId = user.DeviceId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsConnected = user.IsConnected,
                    ConnectionInfo = user.ConnectionInfo,
                    Notes = user.Notes,
                    Tags = user.Tags
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(vpnUserDto),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync($"/devices/{deviceId}/vpn-users", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var createdUserDto = JsonConvert.DeserializeObject<CloudVpnUserDto>(responseData);
                    
                    if (createdUserDto == null)
                    {
                        return null;
                    }
                    
                    var createdUser = CloudVpnUser.FromDto(createdUserDto);
                    
                    log.Info($"Successfully created VPN user {user.Username} for device {deviceId}");
                    
                    return createdUser;
                }
                
                log.Error($"Failed to create VPN user {user.Username} for device {deviceId}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error($"Error creating VPN user for device {deviceId}: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<bool> UpdateVpnUserAsync(string deviceId, CloudVpnUser user)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return false;
                }
                
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    log.Error("VPN user cannot be null and must have an ID");
                    return false;
                }
                
                log.Info($"Updating VPN user {user.Id} for device {deviceId} in MikroTik Cloud");
                
                // Convert to DTO for serialization
                var vpnUserDto = new CloudVpnUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    ExpirationDate = user.ExpirationDate,
                    DeviceId = user.DeviceId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsConnected = user.IsConnected,
                    ConnectionInfo = user.ConnectionInfo,
                    Notes = user.Notes,
                    Tags = user.Tags
                };
                
                var content = new StringContent(
                    JsonConvert.SerializeObject(vpnUserDto),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"/devices/{deviceId}/vpn-users/{user.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully updated VPN user {user.Id} for device {deviceId}");
                    return true;
                }
                
                log.Error($"Failed to update VPN user {user.Id} for device {deviceId}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating VPN user {user.Id} for device {deviceId}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> DeleteVpnUserAsync(string deviceId, string userId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return false;
                }
                
                if (string.IsNullOrEmpty(userId))
                {
                    log.Error("User ID cannot be null or empty");
                    return false;
                }
                
                log.Info($"Deleting VPN user {userId} for device {deviceId} from MikroTik Cloud");
                
                var response = await _httpClient.DeleteAsync($"/devices/{deviceId}/vpn-users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully deleted VPN user {userId} for device {deviceId}");
                    return true;
                }
                
                log.Error($"Failed to delete VPN user {userId} for device {deviceId}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error deleting VPN user {userId} for device {deviceId}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> EstablishVpnConnectionAsync(string deviceId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return false;
                }
                
                log.Info($"Establishing VPN connection to device {deviceId} via MikroTik Cloud");
                
                var response = await _httpClient.PostAsync($"/devices/{deviceId}/vpn/connect", null);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully established VPN connection to device {deviceId}");
                    return true;
                }
                
                log.Error($"Failed to establish VPN connection to device {deviceId}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error establishing VPN connection to device {deviceId}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> DisconnectVpnAsync(string deviceId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return false;
                }
                
                log.Info($"Disconnecting VPN from device {deviceId}");
                
                var response = await _httpClient.PostAsync($"/devices/{deviceId}/vpn/disconnect", null);
                
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Successfully disconnected VPN from device {deviceId}");
                    return true;
                }
                
                log.Error($"Failed to disconnect VPN from device {deviceId}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error disconnecting VPN from device {deviceId}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> IsVpnConnectedAsync(string deviceId)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    log.Error("Device ID cannot be null or empty");
                    return false;
                }
                
                log.Debug($"Checking VPN connection status for device {deviceId}");
                
                var response = await _httpClient.GetAsync($"/devices/{deviceId}/vpn/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var status = JsonConvert.DeserializeObject<dynamic>(responseData);
                    
                    if (status != null && status.connected != null)
                    {
                        return (bool)status.connected;
                    }
                }
                
                log.Warn($"Failed to check VPN connection status for device {deviceId}: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error checking VPN connection status for device {deviceId}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<string> GetCloudApiUrlAsync()
        {
            string cloudApiUrl = _configuration.GetValue<string>("CloudSettings:CloudApiUrl");
            if (string.IsNullOrEmpty(cloudApiUrl))
            {
                cloudApiUrl = "https://api.mikrotik.cloud/v1";
            }
            
            return await Task.FromResult(cloudApiUrl);
        }
        
        public async Task<CloudServiceStatus> GetCloudServiceStatusAsync()
        {
            try
            {
                var status = new CloudServiceStatus
                {
                    LastChecked = DateTime.Now
                };
                
                log.Info("Checking MikroTik Cloud service status");
                
                var response = await _httpClient.GetAsync("/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var statusData = JsonConvert.DeserializeObject<dynamic>(responseData);
                    
                    status.IsAvailable = true;
                    status.Status = "Available";
                    
                    if (statusData != null)
                    {
                        if (statusData.version != null)
                        {
                            status.ApiVersion = statusData.version.ToString();
                        }
                        
                        if (statusData.status != null)
                        {
                            status.Status = statusData.status.ToString();
                        }
                        
                        if (statusData.message != null)
                        {
                            status.Message = statusData.message.ToString();
                        }
                    }
                    
                    log.Info($"MikroTik Cloud service is available (API version: {status.ApiVersion})");
                }
                else
                {
                    status.IsAvailable = false;
                    status.Status = "Unavailable";
                    status.Message = $"HTTP Error: {response.StatusCode}";
                    
                    log.Error($"MikroTik Cloud service is unavailable: {response.StatusCode}");
                }
                
                return status;
            }
            catch (Exception ex)
            {
                log.Error($"Error checking MikroTik Cloud service status: {ex.Message}", ex);
                
                return new CloudServiceStatus
                {
                    IsAvailable = false,
                    Status = "Error",
                    Message = ex.Message,
                    LastChecked = DateTime.Now
                };
            }
        }
        
        // Helper method to ensure authenticated state
        private async Task<bool> EnsureAuthenticatedAsync()
        {
            if (!await IsAuthenticatedAsync())
            {
                // Try to refresh token if available
                if (_authInfo != null && !string.IsNullOrEmpty(_authInfo.RefreshToken))
                {
                    return await RefreshTokenAsync();
                }
                
                log.Error("Not authenticated to MikroTik Cloud. Please authenticate first.");
                return false;
            }
            
            return true;
        }
    }
}