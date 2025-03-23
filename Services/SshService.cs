using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Renci.SshNet;
using Renci.SshNet.Common;
using MikroTikMonitor.Models;
using log4net;
using Microsoft.Extensions.Configuration;

namespace MikroTikMonitor.Services
{
    public class SshService : ISshService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SshService));
        private readonly Dictionary<string, SshClient> _connections = new Dictionary<string, SshClient>();
        private readonly Dictionary<string, Terminal> _terminals = new Dictionary<string, Terminal>();
        private readonly IConfiguration _configuration;
        
        public SshService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> ConnectAsync(RouterDevice device)
        {
            try
            {
                if (_connections.ContainsKey(device.Id))
                {
                    // Already connected
                    return true;
                }
                
                log.Info($"Connecting to device {device.Name} ({device.IpAddress}) via SSH");
                
                ConnectionInfo connectionInfo;
                
                if (!string.IsNullOrEmpty(device.SshPrivateKeyPath))
                {
                    // Use private key authentication
                    log.Debug($"Using private key authentication with key at {device.SshPrivateKeyPath}");
                    var keyFile = new PrivateKeyFile(device.SshPrivateKeyPath);
                    connectionInfo = new ConnectionInfo(
                        device.IpAddress,
                        device.SshPort,
                        device.Username,
                        new PrivateKeyAuthenticationMethod(device.Username, keyFile)
                    );
                }
                else
                {
                    // Use password authentication
                    log.Debug($"Using password authentication for user {device.Username}");
                    connectionInfo = new ConnectionInfo(
                        device.IpAddress,
                        device.SshPort,
                        device.Username,
                        new PasswordAuthenticationMethod(device.Username, device.Password)
                    );
                }
                
                var client = new SshClient(connectionInfo);
                
                // Set connection timeout
                var timeout = _configuration.GetValue<int>("SshSettings:SshConnectionTimeout");
                if (timeout <= 0)
                {
                    timeout = 10000; // Default to 10 seconds if not configured
                }
                client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(timeout);
                
                await Task.Run(() => client.Connect());
                
                _connections[device.Id] = client;
                
                log.Info($"Successfully connected to device {device.Name} ({device.IpAddress}) via SSH");
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error connecting to device {device.Name} ({device.IpAddress}) via SSH: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task DisconnectAsync(RouterDevice device)
        {
            try
            {
                if (_connections.TryGetValue(device.Id, out var client))
                {
                    log.Info($"Disconnecting from device {device.Name} ({device.IpAddress}) via SSH");
                    
                    await Task.Run(() => 
                    {
                        client.Disconnect();
                        client.Dispose();
                        _connections.Remove(device.Id);
                    });
                    
                    log.Info($"Successfully disconnected from device {device.Name} ({device.IpAddress})");
                }
                
                // Clean up any terminals for this device
                var terminalsToRemove = new List<string>();
                foreach (var terminal in _terminals)
                {
                    if (terminal.Value.Device.Id == device.Id)
                    {
                        terminalsToRemove.Add(terminal.Key);
                    }
                }
                
                foreach (var terminalId in terminalsToRemove)
                {
                    await CloseTerminalAsync(_terminals[terminalId]);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error disconnecting from device {device.Name} ({device.IpAddress}): {ex.Message}", ex);
            }
        }
        
        public async Task<bool> TestConnectionAsync(RouterDevice device)
        {
            try
            {
                // Try to connect if not already connected
                if (!_connections.ContainsKey(device.Id))
                {
                    return await ConnectAsync(device);
                }
                
                // Test existing connection
                var client = _connections[device.Id];
                
                if (!client.IsConnected)
                {
                    // Connection is closed, try to reconnect
                    log.Info($"SSH connection to device {device.Name} is closed, attempting to reconnect");
                    
                    _connections.Remove(device.Id);
                    return await ConnectAsync(device);
                }
                
                // Try to execute a simple command to verify connection
                await Task.Run(() => 
                {
                    var cmd = client.CreateCommand("echo ConnectionTest");
                    var result = cmd.Execute();
                    log.Debug($"SSH connection test successful for {device.Name}: {result}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"SSH connection test failed for device {device.Name}: {ex.Message}", ex);
                
                // Clean up failed connection
                if (_connections.TryGetValue(device.Id, out var client))
                {
                    client.Dispose();
                    _connections.Remove(device.Id);
                }
                
                return false;
            }
        }
        
        public async Task<string> ExecuteCommandAsync(RouterDevice device, string command)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return "Error: Failed to connect to device via SSH";
                }
                
                var client = _connections[device.Id];
                
                string result = string.Empty;
                
                await Task.Run(() => 
                {
                    var cmd = client.CreateCommand(command);
                    result = cmd.Execute();
                });
                
                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error executing SSH command on device {device.Name}: {ex.Message}", ex);
                return $"Error: {ex.Message}";
            }
        }
        
        public async Task<string[]> ExecuteCommandsAsync(RouterDevice device, string[] commands)
        {
            var results = new List<string>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return new[] { "Error: Failed to connect to device via SSH" };
                }
                
                var client = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    foreach (var command in commands)
                    {
                        var cmd = client.CreateCommand(command);
                        var result = cmd.Execute();
                        results.Add(result);
                    }
                });
                
                return results.ToArray();
            }
            catch (Exception ex)
            {
                log.Error($"Error executing SSH commands on device {device.Name}: {ex.Message}", ex);
                results.Add($"Error: {ex.Message}");
                return results.ToArray();
            }
        }
        
        public async Task<bool> UploadFileAsync(RouterDevice device, string localPath, string remotePath)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var client = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    using (var sftp = new SftpClient(client.ConnectionInfo))
                    {
                        sftp.Connect();
                        
                        using (var fileStream = File.OpenRead(localPath))
                        {
                            sftp.UploadFile(fileStream, remotePath, true);
                        }
                        
                        sftp.Disconnect();
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error uploading file to device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<bool> DownloadFileAsync(RouterDevice device, string remotePath, string localPath)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var client = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    using (var sftp = new SftpClient(client.ConnectionInfo))
                    {
                        sftp.Connect();
                        
                        using (var fileStream = File.Create(localPath))
                        {
                            sftp.DownloadFile(remotePath, fileStream);
                        }
                        
                        sftp.Disconnect();
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error downloading file from device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }
        
        public async Task<Terminal> OpenTerminalAsync(RouterDevice device)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return null;
                }
                
                var client = _connections[device.Id];
                
                // Create and open shell stream
                var shellStream = client.CreateShellStream(
                    "xterm-256color", 
                    80, 24, 800, 600, 1024);
                
                // Create terminal object
                var terminal = new Terminal
                {
                    Id = Guid.NewGuid().ToString(),
                    Device = device,
                    SessionId = shellStream.SessionId.ToString(),
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true
                };
                
                _terminals[terminal.Id] = terminal;
                
                // Set up data received handler
                shellStream.DataReceived += (sender, e) =>
                {
                    var data = Encoding.UTF8.GetString(e.Data);
                    terminal.OnDataReceived(data);
                    terminal.LastActivity = DateTime.Now;
                };
                
                // Set up terminal closing handler
                shellStream.Closed += (sender, e) =>
                {
                    terminal.IsActive = false;
                    log.Info($"Terminal session {terminal.SessionId} closed for device {device.Name}");
                };
                
                // Set up error handler
                shellStream.ErrorOccurred += (sender, e) =>
                {
                    log.Error($"Error in terminal session {terminal.SessionId}: {e.Exception.Message}", e.Exception);
                    terminal.IsActive = false;
                };
                
                log.Info($"Terminal session {terminal.SessionId} opened for device {device.Name}");
                
                return terminal;
            }
            catch (Exception ex)
            {
                log.Error($"Error opening terminal for device {device.Name}: {ex.Message}", ex);
                return null;
            }
        }
        
        public async Task CloseTerminalAsync(Terminal terminal)
        {
            try
            {
                if (terminal == null)
                {
                    return;
                }
                
                var device = terminal.Device;
                
                if (!_connections.TryGetValue(device.Id, out var client))
                {
                    return;
                }
                
                await Task.Run(() => 
                {
                    // Close shell stream if still active
                    terminal.IsActive = false;
                    
                    // Remove from terminals dictionary
                    if (_terminals.ContainsKey(terminal.Id))
                    {
                        _terminals.Remove(terminal.Id);
                    }
                });
                
                log.Info($"Terminal session {terminal.SessionId} closed for device {device.Name}");
            }
            catch (Exception ex)
            {
                log.Error($"Error closing terminal: {ex.Message}", ex);
            }
        }
        
        public async Task<SshConnectionInfo> GetConnectionInfoAsync(RouterDevice device)
        {
            try
            {
                if (!_connections.TryGetValue(device.Id, out var client))
                {
                    return new SshConnectionInfo
                    {
                        Host = device.IpAddress,
                        Port = device.SshPort,
                        Username = device.Username,
                        IsPrivateKeyAuth = !string.IsNullOrEmpty(device.SshPrivateKeyPath),
                        PrivateKeyPath = device.SshPrivateKeyPath,
                        IsConnected = false
                    };
                }
                
                return await Task.FromResult(new SshConnectionInfo
                {
                    Host = device.IpAddress,
                    Port = device.SshPort,
                    Username = device.Username,
                    IsPrivateKeyAuth = !string.IsNullOrEmpty(device.SshPrivateKeyPath),
                    PrivateKeyPath = device.SshPrivateKeyPath,
                    IsConnected = client.IsConnected,
                    ConnectedAt = DateTime.Now,
                    SessionId = client.ConnectionInfo.SessionId.ToString()
                });
            }
            catch (Exception ex)
            {
                log.Error($"Error getting SSH connection info for device {device.Name}: {ex.Message}", ex);
                
                return new SshConnectionInfo
                {
                    Host = device.IpAddress,
                    Port = device.SshPort,
                    Username = device.Username,
                    IsPrivateKeyAuth = !string.IsNullOrEmpty(device.SshPrivateKeyPath),
                    PrivateKeyPath = device.SshPrivateKeyPath,
                    IsConnected = false
                };
            }
        }
    }
}