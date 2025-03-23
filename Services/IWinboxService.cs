using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface IWinboxService
    {
        Task<bool> IsWinboxInstalledAsync();
        Task<string> GetWinboxPathAsync();
        Task<bool> SetWinboxPathAsync(string path);
        
        Task<WinboxSession> LaunchWinboxAsync(RouterDevice device, WinboxLaunchOptions options = null);
        Task<WinboxSession> LaunchWinboxWithCredentialsAsync(RouterDevice device, string username, string password, WinboxLaunchOptions options = null);
        
        Task<bool> CloseWinboxSessionAsync(WinboxSession session);
        Task<List<WinboxSession>> GetActiveSessionsAsync();
        
        Task<bool> IsDeviceReachableAsync(RouterDevice device);
        
        Task<bool> AddToFavoritesAsync(RouterDevice device, string groupName = null);
        Task<bool> RemoveFromFavoritesAsync(RouterDevice device);
        
        Task<List<WinboxFavorite>> GetFavoritesAsync();
    }
    
    public class WinboxSession
    {
        public string Id { get; set; }
        public RouterDevice Device { get; set; }
        public DateTime LaunchedAt { get; set; }
        public string ProcessId { get; set; }
        public bool IsActive { get; set; }
        public WinboxLaunchOptions Options { get; set; }
    }
    
    public class WinboxLaunchOptions
    {
        public bool FullScreen { get; set; }
        public bool SaveCredentials { get; set; }
        public string SelectedTab { get; set; }
        public bool ReadOnly { get; set; }
        public bool SafeMode { get; set; }
        public string AdditionalParameters { get; set; }
    }
    
    public class WinboxFavorite
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string GroupName { get; set; }
        public DateTime LastAccessed { get; set; }
        public string Notes { get; set; }
    }
}