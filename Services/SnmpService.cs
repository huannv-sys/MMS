using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MikroTikMonitor.Models;
using log4net;

namespace MikroTikMonitor.Services
{
    public class SnmpService : ISnmpService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SnmpService));
        
        // MikroTik standard OIDs
        private static readonly Dictionary<string, string> MikroTikOids = new Dictionary<string, string>
        {
            { "sysDescr", "1.3.6.1.2.1.1.1.0" },
            { "sysName", "1.3.6.1.2.1.1.5.0" },
            { "sysUpTime", "1.3.6.1.2.1.1.3.0" },
            { "cpuLoad", "1.3.6.1.2.1.25.3.3.1.2.1" },
            { "memoryTotal", "1.3.6.1.4.1.14988.1.1.1.8.1.0" },
            { "memoryUsed", "1.3.6.1.4.1.14988.1.1.1.8.2.0" },
            { "temperature", "1.3.6.1.4.1.14988.1.1.3.10.0" },
            { "voltage", "1.3.6.1.4.1.14988.1.1.3.8.0" },
            { "activeFirmware", "1.3.6.1.4.1.14988.1.1.7.7.0" },
            { "firmwareVersion", "1.3.6.1.4.1.14988.1.1.7.4.0" },
            { "serialNumber", "1.3.6.1.4.1.14988.1.1.7.3.0" },
            { "boardName", "1.3.6.1.4.1.14988.1.1.7.8.0" }
        };

        public async Task<bool> TestConnectionAsync(RouterDevice device)
        {
            try
            {
                log.Info($"Testing SNMP connection to device {device.Name} ({device.IpAddress})");
                
                var result = await Task.Run(() => 
                {
                    try
                    {
                        var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                        var community = new OctetString(device.SnmpCommunity);
                        
                        // Try to get the system description
                        var oid = new ObjectIdentifier(MikroTikOids["sysName"]);
                        var message = new GetRequestMessage(
                            0,
                            VersionCode.V2,
                            community,
                            new List<Variable> { new Variable(oid) }
                        );
                        
                        var response = message.GetResponse(2000, endpoint);
                        if (response != null)
                        {
                            log.Info($"SNMP connection successful: {response}");
                            return true;
                        }
                        
                        return false;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"SNMP connection failed: {ex.Message}");
                        return false;
                    }
                });
                
                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error testing SNMP connection to device {device.Name}: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<RouterDevice> GetSystemInfoAsync(RouterDevice device)
        {
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    device.Status = DeviceStatus.Error;
                    return device;
                }
                
                log.Info($"Getting system info via SNMP for device {device.Name} ({device.IpAddress})");
                
                await Task.Run(() => 
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                    var community = new OctetString(device.SnmpCommunity);
                    
                    // Create a list of OIDs to query
                    var oids = new List<Variable>
                    {
                        new Variable(new ObjectIdentifier(MikroTikOids["sysName"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["sysDescr"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["sysUpTime"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["cpuLoad"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["memoryTotal"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["memoryUsed"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["temperature"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["voltage"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["firmwareVersion"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["serialNumber"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["boardName"]))
                    };
                    
                    // Perform SNMP GET requests for each OID
                    foreach (var variable in oids)
                    {
                        try
                        {
                            var message = new GetRequestMessage(
                                0,
                                VersionCode.V2,
                                community,
                                new List<Variable> { variable }
                            );
                            
                            var response = message.GetResponse(2000, endpoint);
                            var result = response.Variables().FirstOrDefault();
                            
                            if (result != null)
                            {
                                string oid = result.Id.ToString();
                                
                                if (oid == MikroTikOids["sysName"])
                                {
                                    device.Name = result.Data.ToString();
                                }
                                else if (oid == MikroTikOids["boardName"])
                                {
                                    device.Model = result.Data.ToString();
                                }
                                else if (oid == MikroTikOids["serialNumber"])
                                {
                                    device.SerialNumber = result.Data.ToString();
                                }
                                else if (oid == MikroTikOids["firmwareVersion"])
                                {
                                    device.FirmwareVersion = result.Data.ToString();
                                }
                                else if (oid == MikroTikOids["sysUpTime"])
                                {
                                    var ticks = result.Data.ToUInt32();
                                    device.Uptime = TimeSpan.FromSeconds(ticks / 100);
                                }
                                else if (oid == MikroTikOids["cpuLoad"])
                                {
                                    var cpuLoad = result.Data.ToInt32();
                                    device.CpuUsage = cpuLoad;
                                }
                                else if (oid == MikroTikOids["memoryTotal"] && result.Data.ToString() != "")
                                {
                                    var totalMemory = result.Data.ToInt64();
                                    
                                    // Get memory used and calculate percentage
                                    var memUsedMsg = new GetRequestMessage(
                                        0,
                                        VersionCode.V2,
                                        community,
                                        new List<Variable> { new Variable(new ObjectIdentifier(MikroTikOids["memoryUsed"])) }
                                    );
                                    
                                    var memUsedResponse = memUsedMsg.GetResponse(2000, endpoint);
                                    var memUsedResult = memUsedResponse.Variables().FirstOrDefault();
                                    
                                    if (memUsedResult != null && memUsedResult.Data.ToString() != "" && totalMemory > 0)
                                    {
                                        var usedMemory = memUsedResult.Data.ToInt64();
                                        device.MemoryUsage = Math.Round((usedMemory / (double)totalMemory) * 100, 2);
                                    }
                                }
                                else if (oid == MikroTikOids["temperature"] && result.Data.ToString() != "")
                                {
                                    var temp = result.Data.ToInt32();
                                    device.Temperature = temp / 10.0; // Temperature is stored in tenths of degrees
                                }
                                else if (oid == MikroTikOids["voltage"] && result.Data.ToString() != "")
                                {
                                    var voltage = result.Data.ToInt32();
                                    device.Voltage = voltage; // Voltage in mV
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn($"Error getting SNMP data for {variable.Id}: {ex.Message}");
                        }
                    }
                });
                
                device.Status = DeviceStatus.Online;
                device.LastSeenOnline = DateTime.Now;
                device.LastUpdated = DateTime.Now;
                
                return device;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting system info via SNMP for device {device.Name}: {ex.Message}", ex);
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
                
                log.Info($"Getting interfaces via SNMP for device {device.Name} ({device.IpAddress})");
                
                await Task.Run(() => 
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                    var community = new OctetString(device.SnmpCommunity);
                    
                    // Get interface list using SNMP walk
                    var interfaceNamesOid = new ObjectIdentifier("1.3.6.1.2.1.2.2.1.2"); // ifDescr
                    var interfaceNames = Messenger.Walk(
                        VersionCode.V2,
                        endpoint,
                        community,
                        interfaceNamesOid,
                        null,
                        60000,
                        WalkMode.WithinSubtree
                    ).ToList();
                    
                    foreach (var variable in interfaceNames)
                    {
                        try
                        {
                            // Extract the interface index from the OID
                            string oid = variable.Id.ToString();
                            string indexStr = oid.Substring(oid.LastIndexOf('.') + 1);
                            
                            if (int.TryParse(indexStr, out int interfaceIndex))
                            {
                                var netInterface = new NetworkInterface
                                {
                                    Id = indexStr,
                                    Name = variable.Data.ToString(),
                                    LastUpdated = DateTime.Now
                                };
                                
                                // Get interface MAC address
                                var macAddrOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.6.{interfaceIndex}"); // ifPhysAddress
                                var macMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(macAddrOid) }
                                );
                                
                                var macResponse = macMessage.GetResponse(2000, endpoint);
                                var macResult = macResponse.Variables().FirstOrDefault();
                                
                                if (macResult != null && macResult.Data.TypeCode == SnmpType.OctetString)
                                {
                                    byte[] macBytes = ((OctetString)macResult.Data).GetRaw();
                                    if (macBytes.Length == 6)
                                    {
                                        netInterface.MacAddress = string.Join(":", macBytes.Select(b => b.ToString("X2")));
                                    }
                                }
                                
                                // Get interface operational status
                                var statusOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.8.{interfaceIndex}"); // ifOperStatus
                                var statusMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(statusOid) }
                                );
                                
                                var statusResponse = statusMessage.GetResponse(2000, endpoint);
                                var statusResult = statusResponse.Variables().FirstOrDefault();
                                
                                if (statusResult != null)
                                {
                                    netInterface.IsUp = statusResult.Data.ToInt32() == 1; // 1 = up, 2 = down
                                }
                                
                                // Get admin status
                                var adminStatusOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.7.{interfaceIndex}"); // ifAdminStatus
                                var adminStatusMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(adminStatusOid) }
                                );
                                
                                var adminStatusResponse = adminStatusMessage.GetResponse(2000, endpoint);
                                var adminStatusResult = adminStatusResponse.Variables().FirstOrDefault();
                                
                                if (adminStatusResult != null)
                                {
                                    netInterface.IsEnabled = adminStatusResult.Data.ToInt32() == 1; // 1 = up, 2 = down
                                }
                                
                                // Get interface type
                                var typeOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.3.{interfaceIndex}"); // ifType
                                var typeMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(typeOid) }
                                );
                                
                                var typeResponse = typeMessage.GetResponse(2000, endpoint);
                                var typeResult = typeResponse.Variables().FirstOrDefault();
                                
                                if (typeResult != null)
                                {
                                    int ifType = typeResult.Data.ToInt32();
                                    switch (ifType)
                                    {
                                        case 6:
                                            netInterface.Type = "ethernet";
                                            break;
                                        case 71:
                                            netInterface.Type = "wifi";
                                            break;
                                        case 131:
                                            netInterface.Type = "tunnel";
                                            break;
                                        case 24:
                                            netInterface.Type = "loopback";
                                            break;
                                        case 23:
                                            netInterface.Type = "ppp";
                                            break;
                                        case 1:
                                            netInterface.Type = "other";
                                            break;
                                        default:
                                            netInterface.Type = ifType.ToString();
                                            break;
                                    }
                                }
                                
                                // Get interface traffic counters
                                var rxBytesOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.10.{interfaceIndex}"); // ifInOctets
                                var rxBytesMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(rxBytesOid) }
                                );
                                
                                var rxBytesResponse = rxBytesMessage.GetResponse(2000, endpoint);
                                var rxBytesResult = rxBytesResponse.Variables().FirstOrDefault();
                                
                                if (rxBytesResult != null)
                                {
                                    netInterface.RxBytes = rxBytesResult.Data.ToInt64();
                                }
                                
                                var txBytesOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.16.{interfaceIndex}"); // ifOutOctets
                                var txBytesMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(txBytesOid) }
                                );
                                
                                var txBytesResponse = txBytesMessage.GetResponse(2000, endpoint);
                                var txBytesResult = txBytesResponse.Variables().FirstOrDefault();
                                
                                if (txBytesResult != null)
                                {
                                    netInterface.TxBytes = txBytesResult.Data.ToInt64();
                                }
                                
                                var rxErrorsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.14.{interfaceIndex}"); // ifInErrors
                                var rxErrorsMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(rxErrorsOid) }
                                );
                                
                                var rxErrorsResponse = rxErrorsMessage.GetResponse(2000, endpoint);
                                var rxErrorsResult = rxErrorsResponse.Variables().FirstOrDefault();
                                
                                if (rxErrorsResult != null)
                                {
                                    netInterface.RxErrors = rxErrorsResult.Data.ToInt64();
                                }
                                
                                var txErrorsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.20.{interfaceIndex}"); // ifOutErrors
                                var txErrorsMessage = new GetRequestMessage(
                                    0,
                                    VersionCode.V2,
                                    community,
                                    new List<Variable> { new Variable(txErrorsOid) }
                                );
                                
                                var txErrorsResponse = txErrorsMessage.GetResponse(2000, endpoint);
                                var txErrorsResult = txErrorsResponse.Variables().FirstOrDefault();
                                
                                if (txErrorsResult != null)
                                {
                                    netInterface.TxErrors = txErrorsResult.Data.ToInt64();
                                }
                                
                                interfaces.Add(netInterface);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn($"Error getting SNMP data for interface {variable.Data}: {ex.Message}");
                        }
                    }
                });
                
                return interfaces;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting interfaces via SNMP for device {device.Name}: {ex.Message}", ex);
                return interfaces;
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
                
                log.Info($"Getting resource usage via SNMP for device {device.Name} ({device.IpAddress})");
                
                await Task.Run(() => 
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                    var community = new OctetString(device.SnmpCommunity);
                    
                    // Create a list of OIDs to query
                    var oids = new List<Variable>
                    {
                        new Variable(new ObjectIdentifier(MikroTikOids["cpuLoad"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["memoryTotal"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["memoryUsed"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["temperature"])),
                        new Variable(new ObjectIdentifier(MikroTikOids["voltage"]))
                    };
                    
                    // Perform SNMP GET requests for each OID
                    foreach (var variable in oids)
                    {
                        try
                        {
                            var message = new GetRequestMessage(
                                0,
                                VersionCode.V2,
                                community,
                                new List<Variable> { variable }
                            );
                            
                            var response = message.GetResponse(2000, endpoint);
                            var result = response.Variables().FirstOrDefault();
                            
                            if (result != null)
                            {
                                string oid = result.Id.ToString();
                                
                                if (oid == MikroTikOids["cpuLoad"])
                                {
                                    usage.CpuUsage = result.Data.ToInt32();
                                }
                                else if (oid == MikroTikOids["memoryTotal"] && result.Data.ToString() != "")
                                {
                                    var totalMemory = result.Data.ToInt64();
                                    
                                    // Get memory used and calculate percentage
                                    var memUsedMsg = new GetRequestMessage(
                                        0,
                                        VersionCode.V2,
                                        community,
                                        new List<Variable> { new Variable(new ObjectIdentifier(MikroTikOids["memoryUsed"])) }
                                    );
                                    
                                    var memUsedResponse = memUsedMsg.GetResponse(2000, endpoint);
                                    var memUsedResult = memUsedResponse.Variables().FirstOrDefault();
                                    
                                    if (memUsedResult != null && memUsedResult.Data.ToString() != "" && totalMemory > 0)
                                    {
                                        var usedMemory = memUsedResult.Data.ToInt64();
                                        usage.MemoryUsage = Math.Round((usedMemory / (double)totalMemory) * 100, 2);
                                    }
                                }
                                else if (oid == MikroTikOids["temperature"] && result.Data.ToString() != "")
                                {
                                    var temp = result.Data.ToInt32();
                                    usage.Temperature = temp / 10.0; // Temperature is stored in tenths of degrees
                                }
                                else if (oid == MikroTikOids["voltage"] && result.Data.ToString() != "")
                                {
                                    var voltage = result.Data.ToInt32();
                                    usage.Voltage = voltage; // Voltage in mV
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn($"Error getting SNMP data for {variable.Id}: {ex.Message}");
                        }
                    }
                });
                
                return usage;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting resource usage via SNMP for device {device.Name}: {ex.Message}", ex);
                return usage;
            }
        }

        public async Task<InterfaceTraffic> GetInterfaceTrafficAsync(RouterDevice device, string interfaceName)
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
                
                log.Info($"Getting interface traffic via SNMP for device {device.Name}, interface {interfaceName}");
                
                await Task.Run(() => 
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                    var community = new OctetString(device.SnmpCommunity);
                    
                    // Find interface index
                    int interfaceIndex = -1;
                    
                    var interfaceNamesOid = new ObjectIdentifier("1.3.6.1.2.1.2.2.1.2"); // ifDescr
                    var interfaceNames = Messenger.Walk(
                        VersionCode.V2,
                        endpoint,
                        community,
                        interfaceNamesOid,
                        null,
                        60000,
                        WalkMode.WithinSubtree
                    ).ToList();
                    
                    foreach (var variable in interfaceNames)
                    {
                        if (variable.Data.ToString() == interfaceName)
                        {
                            string oid = variable.Id.ToString();
                            string indexStr = oid.Substring(oid.LastIndexOf('.') + 1);
                            if (int.TryParse(indexStr, out interfaceIndex))
                            {
                                break;
                            }
                        }
                    }
                    
                    if (interfaceIndex == -1)
                    {
                        log.Warn($"Interface {interfaceName} not found");
                        return;
                    }
                    
                    // Get interface traffic data
                    var rxBytesOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.10.{interfaceIndex}"); // ifInOctets
                    var rxBytesMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(rxBytesOid) }
                    );
                    
                    var rxBytesResponse = rxBytesMessage.GetResponse(2000, endpoint);
                    var rxBytesResult = rxBytesResponse.Variables().FirstOrDefault();
                    
                    if (rxBytesResult != null)
                    {
                        traffic.RxBytes = rxBytesResult.Data.ToInt64();
                    }
                    
                    var txBytesOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.16.{interfaceIndex}"); // ifOutOctets
                    var txBytesMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(txBytesOid) }
                    );
                    
                    var txBytesResponse = txBytesMessage.GetResponse(2000, endpoint);
                    var txBytesResult = txBytesResponse.Variables().FirstOrDefault();
                    
                    if (txBytesResult != null)
                    {
                        traffic.TxBytes = txBytesResult.Data.ToInt64();
                    }
                    
                    var rxPacketsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.11.{interfaceIndex}"); // ifInUcastPkts
                    var rxPacketsMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(rxPacketsOid) }
                    );
                    
                    var rxPacketsResponse = rxPacketsMessage.GetResponse(2000, endpoint);
                    var rxPacketsResult = rxPacketsResponse.Variables().FirstOrDefault();
                    
                    if (rxPacketsResult != null)
                    {
                        traffic.RxPackets = rxPacketsResult.Data.ToInt64();
                    }
                    
                    var txPacketsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.17.{interfaceIndex}"); // ifOutUcastPkts
                    var txPacketsMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(txPacketsOid) }
                    );
                    
                    var txPacketsResponse = txPacketsMessage.GetResponse(2000, endpoint);
                    var txPacketsResult = txPacketsResponse.Variables().FirstOrDefault();
                    
                    if (txPacketsResult != null)
                    {
                        traffic.TxPackets = txPacketsResult.Data.ToInt64();
                    }
                    
                    var rxErrorsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.14.{interfaceIndex}"); // ifInErrors
                    var rxErrorsMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(rxErrorsOid) }
                    );
                    
                    var rxErrorsResponse = rxErrorsMessage.GetResponse(2000, endpoint);
                    var rxErrorsResult = rxErrorsResponse.Variables().FirstOrDefault();
                    
                    if (rxErrorsResult != null)
                    {
                        traffic.RxErrors = rxErrorsResult.Data.ToInt64();
                    }
                    
                    var txErrorsOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.20.{interfaceIndex}"); // ifOutErrors
                    var txErrorsMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(txErrorsOid) }
                    );
                    
                    var txErrorsResponse = txErrorsMessage.GetResponse(2000, endpoint);
                    var txErrorsResult = txErrorsResponse.Variables().FirstOrDefault();
                    
                    if (txErrorsResult != null)
                    {
                        traffic.TxErrors = txErrorsResult.Data.ToInt64();
                    }
                    
                    var rxDiscardOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.13.{interfaceIndex}"); // ifInDiscards
                    var rxDiscardMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(rxDiscardOid) }
                    );
                    
                    var rxDiscardResponse = rxDiscardMessage.GetResponse(2000, endpoint);
                    var rxDiscardResult = rxDiscardResponse.Variables().FirstOrDefault();
                    
                    if (rxDiscardResult != null)
                    {
                        traffic.RxDrops = rxDiscardResult.Data.ToInt64();
                    }
                    
                    var txDiscardOid = new ObjectIdentifier($"1.3.6.1.2.1.2.2.1.19.{interfaceIndex}"); // ifOutDiscards
                    var txDiscardMessage = new GetRequestMessage(
                        0,
                        VersionCode.V2,
                        community,
                        new List<Variable> { new Variable(txDiscardOid) }
                    );
                    
                    var txDiscardResponse = txDiscardMessage.GetResponse(2000, endpoint);
                    var txDiscardResult = txDiscardResponse.Variables().FirstOrDefault();
                    
                    if (txDiscardResult != null)
                    {
                        traffic.TxDrops = txDiscardResult.Data.ToInt64();
                    }
                });
                
                return traffic;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting interface traffic via SNMP for device {device.Name}: {ex.Message}", ex);
                return traffic;
            }
        }

        public async Task<Dictionary<string, string>> GetDeviceOidsAsync(RouterDevice device)
        {
            var oidValues = new Dictionary<string, string>();
            
            try
            {
                if (!await TestConnectionAsync(device))
                {
                    return oidValues;
                }
                
                log.Info($"Getting device OIDs via SNMP for device {device.Name} ({device.IpAddress})");
                
                await Task.Run(() => 
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(device.IpAddress), device.SnmpPort);
                    var community = new OctetString(device.SnmpCommunity);
                    
                    // Get all OIDs from MikroTikOids
                    foreach (var oidPair in MikroTikOids)
                    {
                        try
                        {
                            var oid = new ObjectIdentifier(oidPair.Value);
                            var message = new GetRequestMessage(
                                0,
                                VersionCode.V2,
                                community,
                                new List<Variable> { new Variable(oid) }
                            );
                            
                            var response = message.GetResponse(2000, endpoint);
                            var result = response.Variables().FirstOrDefault();
                            
                            if (result != null)
                            {
                                oidValues[oidPair.Key] = result.Data.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn($"Error getting SNMP data for {oidPair.Key}: {ex.Message}");
                        }
                    }
                });
                
                return oidValues;
            }
            catch (Exception ex)
            {
                log.Error($"Error getting device OIDs via SNMP for device {device.Name}: {ex.Message}", ex);
                return oidValues;
            }
        }
    }
}