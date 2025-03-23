using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MikroTikMonitor.Models;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ip;
using tik4net.Objects.Ip.Firewall;
using tik4net.Objects.System;
using tik4net.Objects.Interface;
using tik4net.Objects.Queue;
using log4net;

namespace MikroTikMonitor.Services
{
    public class RouterApiService : IRouterApiService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RouterApiService));
        private readonly Dictionary<string, ITikConnection> _connections = new Dictionary<string, ITikConnection>();

        public async Task<bool> ConnectAsync(RouterDevice device)
        {
            try
            {
                if (_connections.ContainsKey(device.Id))
                {
                    // Already connected
                    return true;
                }

                log.Info($"Connecting to device {device.Name} ({device.IpAddress}) via RouterOS API");

                var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
                
                connection.Host = device.IpAddress;
                connection.Port = device.ApiPort;
                connection.Username = device.Username;
                connection.Password = device.Password;
                connection.UseEncryption = device.UseSsl;
                
                await Task.Run(() => connection.Open());

                _connections[device.Id] = connection;
                
                log.Info($"Successfully connected to device {device.Name} ({device.IpAddress})");
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error connecting to device {device.Name} ({device.IpAddress}): {ex.Message}", ex);
                return false;
            }
        }

        public async Task DisconnectAsync(RouterDevice device)
        {
            try
            {
                if (_connections.TryGetValue(device.Id, out var connection))
                {
                    log.Info($"Disconnecting from device {device.Name} ({device.IpAddress})");
                    
                    await Task.Run(() => 
                    {
                        connection.Close();
                        _connections.Remove(device.Id);
                    });
                    
                    log.Info($"Successfully disconnected from device {device.Name} ({device.IpAddress})");
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
                var connection = _connections[device.Id];
                
                if (!connection.IsOpened)
                {
                    // Connection is closed, try to reopen
                    log.Info($"Connection to device {device.Name} is closed, attempting to reconnect");
                    return await ConnectAsync(device);
                }
                
                // Try to execute a simple command to verify connection
                await Task.Run(() => 
                {
                    var identity = connection.Execute("/system identity print");
                    log.Debug($"Connection test successful for {device.Name}: {identity}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Connection test failed for device {device.Name}: {ex.Message}", ex);
                
                // Clean up failed connection
                _connections.Remove(device.Id);
                
                return false;
            }
        }

        public async Task<RouterDevice> GetSystemInfoAsync(RouterDevice device)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    throw new Exception("Failed to connect to device");
                }
                
                var connection = _connections[device.Id];

                await Task.Run(() => 
                {
                    // Get identity
                    var identity = connection.Execute("/system identity print").FirstOrDefault();
                    if (identity != null)
                    {
                        device.Name = identity.GetResponseField("name");
                    }

                    // Get resource usage
                    var resource = connection.Execute("/system resource print").FirstOrDefault();
                    if (resource != null)
                    {
                        device.Model = resource.GetResponseField("board-name");
                        device.SerialNumber = resource.GetResponseField("serial-number");
                        device.FirmwareVersion = resource.GetResponseField("version");
                        
                        var uptimeStr = resource.GetResponseField("uptime");
                        if (TimeSpan.TryParse(uptimeStr, out var uptime))
                        {
                            device.Uptime = uptime;
                        }
                        
                        // CPU usage
                        var cpuLoad = resource.GetResponseField("cpu-load");
                        if (double.TryParse(cpuLoad, out var cpuUsage))
                        {
                            device.CpuUsage = cpuUsage;
                        }
                        
                        // Memory usage
                        var totalMemoryStr = resource.GetResponseField("total-memory");
                        var freeMemoryStr = resource.GetResponseField("free-memory");
                        
                        if (long.TryParse(totalMemoryStr, out var totalMemory) && 
                            long.TryParse(freeMemoryStr, out var freeMemory) && 
                            totalMemory > 0)
                        {
                            device.MemoryUsage = Math.Round(((totalMemory - freeMemory) / (double)totalMemory) * 100, 2);
                        }
                        
                        // Disk usage
                        var totalHddStr = resource.GetResponseField("total-hdd-space");
                        var freeHddStr = resource.GetResponseField("free-hdd-space");
                        
                        if (long.TryParse(totalHddStr, out var totalHdd) && 
                            long.TryParse(freeHddStr, out var freeHdd) && 
                            totalHdd > 0)
                        {
                            device.DiskUsage = Math.Round(((totalHdd - freeHdd) / (double)totalHdd) * 100, 2);
                        }
                    }

                    // Get health data if available
                    try
                    {
                        var health = connection.Execute("/system health print").FirstOrDefault();
                        if (health != null)
                        {
                            var temperatureStr = health.GetResponseField("temperature");
                            if (double.TryParse(temperatureStr, out var temperature))
                            {
                                device.Temperature = temperature;
                            }
                            
                            var voltageStr = health.GetResponseField("voltage");
                            if (int.TryParse(voltageStr, out var voltage))
                            {
                                device.Voltage = voltage;
                            }
                        }
                    }
                    catch
                    {
                        // Health info may not be available on all devices, ignore errors
                        log.Debug($"Health information not available for device {device.Name}");
                    }
                });
                
                device.Status = DeviceStatus.Online;
                device.LastSeenOnline = DateTime.Now;
                device.LastUpdated = DateTime.Now;
                
                return device;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting system info for device {device.Name}: {ex.Message}", ex);
                device.Status = DeviceStatus.Error;
                return device;
            }
        }

        public async Task<IEnumerable<NetworkInterface>> GetInterfacesAsync(RouterDevice device)
        {
            var interfaces = new List<NetworkInterface>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return interfaces;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Get all interfaces
                    var tikInterfaces = connection.LoadAll<tik4net.Objects.Interface.Interface>();
                    
                    foreach (var tikInterface in tikInterfaces)
                    {
                        var netInterface = new NetworkInterface
                        {
                            Id = tikInterface.Id,
                            Name = tikInterface.Name,
                            Type = tikInterface.Type,
                            MacAddress = tikInterface.MacAddress,
                            IsEnabled = !tikInterface.Disabled,
                            IsUp = tikInterface.Running,
                            Comment = tikInterface.Comment,
                            LastUpdated = DateTime.Now
                        };
                        
                        // Get traffic statistics
                        var stats = connection.Execute($"/interface monitor-traffic {tikInterface.Name} once").FirstOrDefault();
                        if (stats != null)
                        {
                            if (long.TryParse(stats.GetResponseField("rx-bits-per-second"), out var rxBits))
                            {
                                netInterface.RxBytes = rxBits / 8; // Convert bits to bytes
                            }
                            
                            if (long.TryParse(stats.GetResponseField("tx-bits-per-second"), out var txBits))
                            {
                                netInterface.TxBytes = txBits / 8; // Convert bits to bytes
                            }
                        }
                        
                        // Try to get IP address if available
                        try
                        {
                            var ipAddresses = connection.Execute($"/ip address print where interface={tikInterface.Name}");
                            var ipAddress = ipAddresses.FirstOrDefault();
                            
                            if (ipAddress != null)
                            {
                                netInterface.IpAddress = ipAddress.GetResponseField("address");
                            }
                        }
                        catch
                        {
                            // IP address might not be available for all interfaces
                        }
                        
                        interfaces.Add(netInterface);
                    }
                });
                
                return interfaces;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting interfaces for device {device.Name}: {ex.Message}", ex);
                return interfaces;
            }
        }

        public async Task<IEnumerable<DhcpLease>> GetDhcpLeasesAsync(RouterDevice device)
        {
            var leases = new List<DhcpLease>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return leases;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    var tikLeases = connection.Execute("/ip dhcp-server lease print");
                    
                    foreach (var tikLease in tikLeases)
                    {
                        var lease = new DhcpLease
                        {
                            Id = tikLease.GetResponseField(".id"),
                            Address = tikLease.GetResponseField("address"),
                            MacAddress = tikLease.GetResponseField("mac-address"),
                            ClientId = tikLease.GetResponseField("client-id"),
                            HostName = tikLease.GetResponseField("host-name"),
                            Comment = tikLease.GetResponseField("comment"),
                            Server = tikLease.GetResponseField("server"),
                            Status = tikLease.GetResponseField("status")
                        };
                        
                        var expires = tikLease.GetResponseField("expires-after");
                        lease.IsDynamic = !string.IsNullOrEmpty(expires) && expires != "never";
                        
                        if (lease.IsDynamic && TimeSpan.TryParse(expires, out var expiryTime))
                        {
                            lease.ExpiryTime = DateTime.Now.Add(expiryTime);
                        }
                        
                        leases.Add(lease);
                    }
                });
                
                return leases;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting DHCP leases for device {device.Name}: {ex.Message}", ex);
                return leases;
            }
        }

        public async Task<IEnumerable<LogEntry>> GetLogsAsync(RouterDevice device, int limit = A100)
        {
            var logs = new List<LogEntry>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return logs;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    var tikLogs = connection.Execute($"/log print limit={limit}");
                    
                    foreach (var tikLog in tikLogs)
                    {
                        var logEntry = new LogEntry
                        {
                            Id = tikLog.GetResponseField(".id"),
                            Time = DateTime.ParseExact(
                                tikLog.GetResponseField("time"), 
                                "MMM dd HH:mm:ss", 
                                System.Globalization.CultureInfo.InvariantCulture),
                            Topics = tikLog.GetResponseField("topics"),
                            Message = tikLog.GetResponseField("message")
                        };
                        
                        logs.Add(logEntry);
                    }
                });
                
                return logs.OrderByDescending(l => l.Time);
            }
            catch (Exception ex)
            {
                log.Error($"Error getting logs for device {device.Name}: {ex.Message}", ex);
                return logs;
            }
        }

        public async Task<IEnumerable<FirewallRule>> GetFirewallRulesAsync(RouterDevice device)
        {
            var rules = new List<FirewallRule>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return rules;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    var tikRules = connection.Execute("/ip firewall filter print");
                    
                    int position = 0;
                    foreach (var tikRule in tikRules)
                    {
                        var rule = new FirewallRule
                        {
                            Id = tikRule.GetResponseField(".id"),
                            Chain = tikRule.GetResponseField("chain"),
                            Action = tikRule.GetResponseField("action"),
                            Protocol = tikRule.GetResponseField("protocol"),
                            SrcAddress = tikRule.GetResponseField("src-address"),
                            DstAddress = tikRule.GetResponseField("dst-address"),
                            SrcPort = tikRule.GetResponseField("src-port"),
                            DstPort = tikRule.GetResponseField("dst-port"),
                            Comment = tikRule.GetResponseField("comment"),
                            Position = position++,
                            Disabled = tikRule.GetResponseField("disabled") == "true"
                        };
                        
                        rules.Add(rule);
                    }
                });
                
                return rules;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting firewall rules for device {device.Name}: {ex.Message}", ex);
                return rules;
            }
        }

        public async Task<IEnumerable<VpnTunnel>> GetVpnTunnelsAsync(RouterDevice device)
        {
            var tunnels = new List<VpnTunnel>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return tunnels;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Check for different VPN types

                    // IPsec tunnels
                    try
                    {
                        var ipsecTunnels = connection.Execute("/ip ipsec policy print");
                        foreach (var tikTunnel in ipsecTunnels)
                        {
                            var tunnel = new VpnTunnel
                            {
                                Id = tikTunnel.GetResponseField(".id"),
                                Name = tikTunnel.GetResponseField("comment") ?? "IPsec Tunnel " + tikTunnel.GetResponseField(".id"),
                                Type = "IPsec",
                                RemoteAddress = tikTunnel.GetResponseField("sa-dst-address"),
                                LocalAddress = tikTunnel.GetResponseField("sa-src-address"),
                                Comment = tikTunnel.GetResponseField("comment"),
                                Disabled = tikTunnel.GetResponseField("disabled") == "true"
                            };
                            
                            tunnels.Add(tunnel);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting IPsec tunnels: {ex.Message}");
                    }

                    // PPTP active connections
                    try
                    {
                        var pptpActive = connection.Execute("/interface pptp-server print detail");
                        foreach (var tikTunnel in pptpActive)
                        {
                            var tunnel = new VpnTunnel
                            {
                                Id = tikTunnel.GetResponseField(".id"),
                                Name = tikTunnel.GetResponseField("name"),
                                Type = "PPTP",
                                RemoteAddress = tikTunnel.GetResponseField("remote-address"),
                                IsActive = tikTunnel.GetResponseField("running") == "true",
                                Comment = "PPTP Connection"
                            };
                            
                            tunnels.Add(tunnel);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting PPTP tunnels: {ex.Message}");
                    }

                    // L2TP active connections
                    try
                    {
                        var l2tpActive = connection.Execute("/interface l2tp-server print detail");
                        foreach (var tikTunnel in l2tpActive)
                        {
                            var tunnel = new VpnTunnel
                            {
                                Id = tikTunnel.GetResponseField(".id"),
                                Name = tikTunnel.GetResponseField("name"),
                                Type = "L2TP",
                                RemoteAddress = tikTunnel.GetResponseField("remote-address"),
                                IsActive = tikTunnel.GetResponseField("running") == "true",
                                Comment = "L2TP Connection"
                            };
                            
                            tunnels.Add(tunnel);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting L2TP tunnels: {ex.Message}");
                    }

                    // OpenVPN active connections
                    try
                    {
                        var ovpnActive = connection.Execute("/interface ovpn-server print detail");
                        foreach (var tikTunnel in ovpnActive)
                        {
                            var tunnel = new VpnTunnel
                            {
                                Id = tikTunnel.GetResponseField(".id"),
                                Name = tikTunnel.GetResponseField("name"),
                                Type = "OpenVPN",
                                RemoteAddress = tikTunnel.GetResponseField("remote-address"),
                                IsActive = tikTunnel.GetResponseField("running") == "true",
                                Comment = "OpenVPN Connection"
                            };
                            
                            tunnels.Add(tunnel);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting OpenVPN tunnels: {ex.Message}");
                    }
                });
                
                return tunnels;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting VPN tunnels for device {device.Name}: {ex.Message}", ex);
                return tunnels;
            }
        }

        public async Task<IEnumerable<QosRule>> GetQosRulesAsync(RouterDevice device)
        {
            var rules = new List<QosRule>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return rules;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Simple queues
                    try
                    {
                        var simpleQueues = connection.LoadAll<SimpleQueue>();
                        foreach (var queue in simpleQueues)
                        {
                            var rule = new QosRule
                            {
                                Id = queue.Id,
                                Name = queue.Name,
                                Target = queue.Target,
                                Parent = queue.Parent,
                                MaxLimit = ParseBitsPerSecond(queue.MaxLimit),
                                Priority = int.TryParse(queue.Priority, out var p) ? p : 8,
                                Burst = queue.Burst,
                                BurstTime = queue.BurstTime,
                                BurstThreshold = queue.BurstThreshold,
                                Disabled = queue.Disabled,
                                Comment = queue.Comment
                            };
                            
                            rules.Add(rule);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting simple queues: {ex.Message}");
                    }

                    // Queue trees
                    try
                    {
                        var queueTrees = connection.Execute("/queue tree print");
                        foreach (var tikQueue in queueTrees)
                        {
                            var rule = new QosRule
                            {
                                Id = tikQueue.GetResponseField(".id"),
                                Name = tikQueue.GetResponseField("name"),
                                Parent = tikQueue.GetResponseField("parent"),
                                Disabled = tikQueue.GetResponseField("disabled") == "true",
                                Comment = "Queue Tree"
                            };
                            
                            var maxLimit = tikQueue.GetResponseField("max-limit");
                            rule.MaxLimit = ParseBitsPerSecond(maxLimit);
                            
                            var priority = tikQueue.GetResponseField("priority");
                            rule.Priority = int.TryParse(priority, out var p) ? p : 8;
                            
                            rules.Add(rule);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting queue trees: {ex.Message}");
                    }
                });
                
                return rules;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting QoS rules for device {device.Name}: {ex.Message}", ex);
                return rules;
            }
        }

        private long ParseBitsPerSecond(string limit)
        {
            if (string.IsNullOrEmpty(limit) || limit == "unlimited")
                return 0;
            
            // Parse formats like "10M/10M" (max-limit/min-limit)
            if (limit.Contains('/'))
            {
                limit = limit.Split('/')[0];
            }
            
            // Parse numeric part and unit
            long multiplier = 1;
            string numericPart = string.Empty;
            
            if (limit.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1_000;
                numericPart = limit.Substring(0, limit.Length - 1);
            }
            else if (limit.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1_000_000;
                numericPart = limit.Substring(0, limit.Length - 1);
            }
            else if (limit.EndsWith("G", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1_000_000_000;
                numericPart = limit.Substring(0, limit.Length - 1);
            }
            else
            {
                numericPart = limit;
            }
            
            if (double.TryParse(numericPart, out var value))
            {
                return (long)(value * multiplier);
            }
            
            return 0;
        }

        public async Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(RouterDevice device)
        {
            var flows = new List<TrafficFlow>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return flows;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Check for torch tool data or connection tracking
                    try
                    {
                        var connections = connection.Execute("/ip firewall connection print");
                        int count = 0;
                        
                        foreach (var conn in connections)
                        {
                            if (count++ >= 100) break; // Limit to 100 connections
                            
                            var flow = new TrafficFlow
                            {
                                Id = conn.GetResponseField(".id"),
                                SrcAddress = conn.GetResponseField("src-address"),
                                DstAddress = conn.GetResponseField("dst-address"),
                                Protocol = conn.GetResponseField("protocol"),
                                SrcPort = conn.GetResponseField("src-port"),
                                DstPort = conn.GetResponseField("dst-port"),
                                Timestamp = DateTime.Now
                            };
                            
                            var bytesStr = conn.GetResponseField("bytes");
                            if (!string.IsNullOrEmpty(bytesStr) && long.TryParse(bytesStr, out var bytes))
                            {
                                flow.Bytes = bytes;
                            }
                            
                            flows.Add(flow);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug($"Error getting connection tracking data: {ex.Message}");
                    }
                });
                
                return flows;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting traffic flows for device {device.Name}: {ex.Message}", ex);
                return flows;
            }
        }

        public async Task<ResourceUsage> GetResourceUsageAsync(RouterDevice device)
        {
            var usage = new ResourceUsage
            {
                Timestamp = DateTime.Now
            };
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return usage;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Get CPU and memory usage
                    var resource = connection.Execute("/system resource print").FirstOrDefault();
                    if (resource != null)
                    {
                        var cpuLoad = resource.GetResponseField("cpu-load");
                        if (double.TryParse(cpuLoad, out var cpuUsage))
                        {
                            usage.CpuUsage = cpuUsage;
                        }
                        
                        var totalMemoryStr = resource.GetResponseField("total-memory");
                        var freeMemoryStr = resource.GetResponseField("free-memory");
                        
                        if (long.TryParse(totalMemoryStr, out var totalMemory) && 
                            long.TryParse(freeMemoryStr, out var freeMemory) && 
                            totalMemory > 0)
                        {
                            usage.MemoryUsage = Math.Round(((totalMemory - freeMemory) / (double)totalMemory) * 100, 2);
                        }
                        
                        var totalHddStr = resource.GetResponseField("total-hdd-space");
                        var freeHddStr = resource.GetResponseField("free-hdd-space");
                        
                        if (long.TryParse(totalHddStr, out var totalHdd) && 
                            long.TryParse(freeHddStr, out var freeHdd) && 
                            totalHdd > 0)
                        {
                            usage.DiskUsage = Math.Round(((totalHdd - freeHdd) / (double)totalHdd) * 100, 2);
                        }
                    }

                    // Get health data if available
                    try
                    {
                        var health = connection.Execute("/system health print").FirstOrDefault();
                        if (health != null)
                        {
                            var temperatureStr = health.GetResponseField("temperature");
                            if (double.TryParse(temperatureStr, out var temperature))
                            {
                                usage.Temperature = temperature;
                            }
                            
                            var voltageStr = health.GetResponseField("voltage");
                            if (int.TryParse(voltageStr, out var voltage))
                            {
                                usage.Voltage = voltage;
                            }
                        }
                    }
                    catch
                    {
                        // Health info may not be available on all devices
                    }
                });
                
                return usage;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting resource usage for device {device.Name}: {ex.Message}", ex);
                return usage;
            }
        }

        public async Task<InterfaceTraffic> GetInterfaceTrafficAsync(RouterDevice device, string interfaceId)
        {
            var traffic = new InterfaceTraffic
            {
                Timestamp = DateTime.Now
            };
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return traffic;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    // Find interface name by ID
                    var interfaceName = string.Empty;
                    var interfaces = connection.Execute("/interface print");
                    foreach (var iface in interfaces)
                    {
                        if (iface.GetResponseField(".id") == interfaceId)
                        {
                            interfaceName = iface.GetResponseField("name");
                            break;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(interfaceName))
                    {
                        throw new Exception($"Interface with ID {interfaceId} not found");
                    }
                    
                    // Get traffic statistics
                    var stats = connection.Execute($"/interface monitor-traffic {interfaceName} once").FirstOrDefault();
                    if (stats != null)
                    {
                        if (long.TryParse(stats.GetResponseField("rx-bits-per-second"), out var rxBits))
                        {
                            traffic.RxBytes = rxBits / 8; // Convert bits to bytes
                        }
                        
                        if (long.TryParse(stats.GetResponseField("tx-bits-per-second"), out var txBits))
                        {
                            traffic.TxBytes = txBits / 8; // Convert bits to bytes
                        }
                        
                        if (long.TryParse(stats.GetResponseField("rx-packets-per-second"), out var rxPackets))
                        {
                            traffic.RxPackets = rxPackets;
                        }
                        
                        if (long.TryParse(stats.GetResponseField("tx-packets-per-second"), out var txPackets))
                        {
                            traffic.TxPackets = txPackets;
                        }
                    }
                    
                    // Get error statistics if available
                    try
                    {
                        var errors = connection.Execute($"/interface ethernet print stats where name={interfaceName}").FirstOrDefault();
                        if (errors != null)
                        {
                            if (long.TryParse(errors.GetResponseField("rx-error"), out var rxErrors))
                            {
                                traffic.RxErrors = rxErrors;
                            }
                            
                            if (long.TryParse(errors.GetResponseField("tx-error"), out var txErrors))
                            {
                                traffic.TxErrors = txErrors;
                            }
                            
                            if (long.TryParse(errors.GetResponseField("rx-drop"), out var rxDrops))
                            {
                                traffic.RxDrops = rxDrops;
                            }
                            
                            if (long.TryParse(errors.GetResponseField("tx-drop"), out var txDrops))
                            {
                                traffic.TxDrops = txDrops;
                            }
                        }
                    }
                    catch
                    {
                        // Error statistics might not be available for all interfaces
                    }
                });
                
                return traffic;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting interface traffic for device {device.Name}: {ex.Message}", ex);
                return traffic;
            }
        }

        public async Task<bool> EnableInterfaceAsync(RouterDevice device, string interfaceId)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute($"/interface enable {interfaceId}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error enabling interface {interfaceId} on device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> DisableInterfaceAsync(RouterDevice device, string interfaceId)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute($"/interface disable {interfaceId}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error disabling interface {interfaceId} on device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> EnableFirewallRuleAsync(RouterDevice device, string ruleId)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute($"/ip firewall filter enable {ruleId}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error enabling firewall rule {ruleId} on device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> DisableFirewallRuleAsync(RouterDevice device, string ruleId)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute($"/ip firewall filter disable {ruleId}");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error disabling firewall rule {ruleId} on device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RebootDeviceAsync(RouterDevice device)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute("/system reboot");
                });
                
                // Remove connection as it will be lost after reboot
                _connections.Remove(device.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error rebooting device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> ShutdownDeviceAsync(RouterDevice device)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    connection.Execute("/system shutdown");
                });
                
                // Remove connection as it will be lost after shutdown
                _connections.Remove(device.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error shutting down device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> BackupConfigurationAsync(RouterDevice device, string backupPath)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    var tempFileName = $"backup_{device.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    
                    // Create backup on the router
                    connection.Execute($"/system backup save name={tempFileName}");
                    
                    // Download the backup file
                    var fileData = connection.LoadFile($"{tempFileName}.backup");
                    
                    // Save to local file
                    File.WriteAllBytes(backupPath, fileData);
                    
                    // Delete temporary backup file from router
                    connection.Execute($"/file remove \"{tempFileName}.backup\"");
                });
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error backing up device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RestoreConfigurationAsync(RouterDevice device, string backupPath)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return false;
                }
                
                var connection = _connections[device.Id];
                
                await Task.Run(() => 
                {
                    var tempFileName = $"restore_{device.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.backup";
                    
                    // Read backup file
                    var fileData = File.ReadAllBytes(backupPath);
                    
                    // Upload to router
                    connection.UploadFile(tempFileName, fileData);
                    
                    // Restore from backup
                    connection.Execute($"/system backup load name={tempFileName}");
                    
                    // Delete temporary file
                    connection.Execute($"/file remove \"{tempFileName}\"");
                });
                
                // Remove connection as it will be lost after restore
                _connections.Remove(device.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error restoring device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<string> ExecuteCommandAsync(RouterDevice device, string command)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return "Error: Failed to connect to device";
                }
                
                var connection = _connections[device.Id];
                
                string result = string.Empty;
                
                await Task.Run(() => 
                {
                    var response = connection.Execute(command);
                    
                    // Format the response as a string
                    foreach (var item in response)
                    {
                        foreach (var field in item.Words)
                        {
                            result += $"{field.Key}: {field.Value}\n";
                        }
                        result += "\n";
                    }
                });
                
                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error executing command on device {device.Name}: {ex.Message}", ex);
                return $"Error: {ex.Message}";
            }
        }
    }
}