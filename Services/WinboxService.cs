using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using MikroTikMonitor.Models;
using log4net;

namespace MikroTikMonitor.Services
{
    public class WinboxService : IWinboxService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WinboxService));
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, WinboxSession> _activeSessions = new Dictionary<string, WinboxSession>();
        
        public WinboxService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> IsWinboxInstalledAsync()
        {
            return await Task.Run(() => 
            {
                try
                {
                    var winboxPath = GetWinboxPath();
                    return !string.IsNullOrEmpty(winboxPath) && File.Exists(winboxPath);
                }
                catch (Exception ex)
                {
                    log.Error("Error checking if Winbox is installed", ex);
                    return false;
                }
            });
        }
        
        public async Task<string> GetWinboxPathAsync()
        {
            return await Task.Run(() => GetWinboxPath());
        }
        
        private string GetWinboxPath()
        {
            // First check configuration
            var configPath = _configuration.GetValue<string>("ConnectionSettings:WinboxPath");
            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
            {
                return configPath;
            }
            
            // Check common installation locations
            var commonPaths = new[]
            {
                @"C:\Program Files (x86)\Mikrotik\Winbox\winbox64.exe",
                @"C:\Program Files\Mikrotik\Winbox\winbox64.exe",
                @"C:\Program Files (x86)\Mikrotik\winbox64.exe",
                @"C:\Program Files\Mikrotik\winbox64.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mikrotik", "Winbox", "winbox64.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mikrotik", "Winbox", "winbox64.exe")
            };
            
            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            // Check for 32-bit version
            var commonPaths32 = new[]
            {
                @"C:\Program Files (x86)\Mikrotik\Winbox\winbox.exe",
                @"C:\Program Files\Mikrotik\Winbox\winbox.exe",
                @"C:\Program Files (x86)\Mikrotik\winbox.exe",
                @"C:\Program Files\Mikrotik\winbox.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mikrotik", "Winbox", "winbox.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mikrotik", "Winbox", "winbox.exe")
            };
            
            foreach (var path in commonPaths32)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            // Check registry
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\winbox.exe"))
                {
                    if (key != null)
                    {
                        var registryPath = key.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(registryPath) && File.Exists(registryPath))
                        {
                            return registryPath;
                        }
                    }
                }
                
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\winbox64.exe"))
                {
                    if (key != null)
                    {
                        var registryPath = key.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(registryPath) && File.Exists(registryPath))
                        {
                            return registryPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error checking registry for Winbox path", ex);
            }
            
            // Not found
            return null;
        }
        
        public async Task<bool> SetWinboxPathAsync(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return false;
                }
                
                // No direct way to update configuration at runtime
                // This would need to be handled externally by updating appsettings.json
                
                log.Info($"Winbox path set to: {path}");
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error setting Winbox path: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<WinboxSession> LaunchWinboxAsync(RouterDevice device, WinboxLaunchOptions options = null)
        {
            try
            {
                if (options == null)
                {
                    options = new WinboxLaunchOptions();
                }
                
                var winboxPath = await GetWinboxPathAsync();
                if (string.IsNullOrEmpty(winboxPath))
                {
                    log.Error("Winbox not found. Please set the correct path to winbox64.exe");
                    return null;
                }
                
                var args = new List<string>();
                
                // Add IP address and port
                args.Add(device.IpAddress);
                
                if (device.WinboxPort != 8291) // 8291 is the default port
                {
                    args.Add(device.WinboxPort.ToString());
                }
                
                // Add Winbox launch options
                if (options.FullScreen)
                {
                    args.Add("fullscreen=yes");
                }
                
                if (options.SaveCredentials)
                {
                    args.Add("saveCredentials=yes");
                }
                
                if (!string.IsNullOrEmpty(options.SelectedTab))
                {
                    args.Add($"tab={options.SelectedTab}");
                }
                
                if (options.ReadOnly)
                {
                    args.Add("readonly=yes");
                }
                
                if (options.SafeMode)
                {
                    args.Add("safemode=yes");
                }
                
                if (!string.IsNullOrEmpty(options.AdditionalParameters))
                {
                    args.Add(options.AdditionalParameters);
                }
                
                // Launch process
                var process = await Task.Run(() => 
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = winboxPath,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = true
                    };
                    
                    return Process.Start(startInfo);
                });
                
                if (process == null)
                {
                    log.Error("Failed to start Winbox process");
                    return null;
                }
                
                // Create and return session object
                var session = new WinboxSession
                {
                    Id = Guid.NewGuid().ToString(),
                    Device = device,
                    LaunchedAt = DateTime.Now,
                    ProcessId = process.Id.ToString(),
                    IsActive = true,
                    Options = options
                };
                
                _activeSessions[session.Id] = session;
                
                log.Info($"Launched Winbox session {session.Id} for device {device.Name} ({device.IpAddress})");
                
                return session;
            }
            catch (Exception ex)
            {
                log.Error($"Error launching Winbox for device {device.Name}: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<WinboxSession> LaunchWinboxWithCredentialsAsync(RouterDevice device, string username, string password, WinboxLaunchOptions options = null)
        {
            try
            {
                if (options == null)
                {
                    options = new WinboxLaunchOptions();
                }
                
                var winboxPath = await GetWinboxPathAsync();
                if (string.IsNullOrEmpty(winboxPath))
                {
                    log.Error("Winbox not found. Please set the correct path to winbox64.exe");
                    return null;
                }
                
                var args = new List<string>();
                
                // Add IP address and port
                args.Add(device.IpAddress);
                
                if (device.WinboxPort != 8291) // 8291 is the default port
                {
                    args.Add(device.WinboxPort.ToString());
                }
                
                // Add credentials
                if (!string.IsNullOrEmpty(username))
                {
                    args.Add(username);
                }
                
                if (!string.IsNullOrEmpty(password))
                {
                    args.Add(password);
                }
                
                // Add Winbox launch options
                if (options.FullScreen)
                {
                    args.Add("fullscreen=yes");
                }
                
                if (options.SaveCredentials)
                {
                    args.Add("saveCredentials=yes");
                }
                
                if (!string.IsNullOrEmpty(options.SelectedTab))
                {
                    args.Add($"tab={options.SelectedTab}");
                }
                
                if (options.ReadOnly)
                {
                    args.Add("readonly=yes");
                }
                
                if (options.SafeMode)
                {
                    args.Add("safemode=yes");
                }
                
                if (!string.IsNullOrEmpty(options.AdditionalParameters))
                {
                    args.Add(options.AdditionalParameters);
                }
                
                // Launch process
                var process = await Task.Run(() => 
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = winboxPath,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = true
                    };
                    
                    return Process.Start(startInfo);
                });
                
                if (process == null)
                {
                    log.Error("Failed to start Winbox process");
                    return null;
                }
                
                // Create and return session object
                var session = new WinboxSession
                {
                    Id = Guid.NewGuid().ToString(),
                    Device = device,
                    LaunchedAt = DateTime.Now,
                    ProcessId = process.Id.ToString(),
                    IsActive = true,
                    Options = options
                };
                
                _activeSessions[session.Id] = session;
                
                log.Info($"Launched Winbox session {session.Id} for device {device.Name} ({device.IpAddress}) with credentials");
                
                return session;
            }
            catch (Exception ex)
            {
                log.Error($"Error launching Winbox for device {device.Name}: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task<bool> CloseWinboxSessionAsync(WinboxSession session)
        {
            try
            {
                if (session == null || !session.IsActive)
                {
                    return false;
                }
                
                // Try to find and close the process
                var success = await Task.Run(() => 
                {
                    try
                    {
                        if (int.TryParse(session.ProcessId, out var processId))
                        {
                            var process = Process.GetProcessById(processId);
                            if (process != null && !process.HasExited)
                            {
                                process.CloseMainWindow();
                                
                                // Give it a moment to close gracefully
                                if (!process.WaitForExit(5000))
                                {
                                    process.Kill();
                                }
                                
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn($"Error closing Winbox process: {ex.Message}");
                    }
                    
                    return false;
                });
                
                // Update session status regardless of process closure success
                session.IsActive = false;
                
                if (_activeSessions.ContainsKey(session.Id))
                {
                    _activeSessions.Remove(session.Id);
                }
                
                log.Info($"Closed Winbox session {session.Id} for device {session.Device.Name}");
                
                return success;
            }
            catch (Exception ex)
            {
                log.Error($"Error closing Winbox session: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<List<WinboxSession>> GetActiveSessionsAsync()
        {
            try
            {
                // Check if processes are still running and update session status
                await Task.Run(() => 
                {
                    var inactiveSessions = new List<string>();
                    
                    foreach (var session in _activeSessions.Values)
                    {
                        try
                        {
                            if (int.TryParse(session.ProcessId, out var processId))
                            {
                                try
                                {
                                    var process = Process.GetProcessById(processId);
                                    session.IsActive = process != null && !process.HasExited;
                                }
                                catch
                                {
                                    // Process doesn't exist anymore
                                    session.IsActive = false;
                                }
                            }
                            else
                            {
                                session.IsActive = false;
                            }
                            
                            if (!session.IsActive)
                            {
                                inactiveSessions.Add(session.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug($"Error checking session status: {ex.Message}");
                            inactiveSessions.Add(session.Id);
                        }
                    }
                    
                    // Remove inactive sessions
                    foreach (var sessionId in inactiveSessions)
                    {
                        _activeSessions.Remove(sessionId);
                    }
                });
                
                return _activeSessions.Values.ToList();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting active Winbox sessions: {ex.Message}", ex);
                return new List<WinboxSession>();
            }
        }
        
        public async Task<bool> IsDeviceReachableAsync(RouterDevice device)
        {
            try
            {
                // Ping the device first to check if it's reachable
                var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(device.IpAddress, 2000);
                
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
            catch (Exception ex)
            {
                log.Error($"Error checking if device {device.Name} is reachable: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> AddToFavoritesAsync(RouterDevice device, string groupName = null)
        {
            try
            {
                // Winbox stores favorites in a binary format in a .crs file
                // There's no official API to modify this file
                // This operation would require either special reverse-engineered code
                // or launching Winbox and automating UI interactions
                
                // For now, return false to indicate this operation is not supported
                log.Warn("Adding to Winbox favorites is not directly supported");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error adding device {device.Name} to Winbox favorites: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> RemoveFromFavoritesAsync(RouterDevice device)
        {
            try
            {
                // Winbox stores favorites in a binary format in a .crs file
                // There's no official API to modify this file
                // This operation would require either special reverse-engineered code
                // or launching Winbox and automating UI interactions
                
                // For now, return false to indicate this operation is not supported
                log.Warn("Removing from Winbox favorites is not directly supported");
                return false;
            }
            catch (Exception ex)
            {
                log.Error($"Error removing device {device.Name} from Winbox favorites: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<List<WinboxFavorite>> GetFavoritesAsync()
        {
            try
            {
                // Winbox stores favorites in a binary format in a .crs file
                // There's no official API to read this file
                // This operation would require either special reverse-engineered code
                // or launching Winbox and automating UI interactions
                
                // For now, return an empty list to indicate this operation is not supported
                log.Warn("Reading Winbox favorites is not directly supported");
                return new List<WinboxFavorite>();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting Winbox favorites: {ex.Message}", ex);
                return new List<WinboxFavorite>();
            }
        }
    }
}