using System;
using System.Threading.Tasks;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    public interface ISshService
    {
        Task<bool> ConnectAsync(RouterDevice device);
        Task DisconnectAsync(RouterDevice device);
        Task<bool> TestConnectionAsync(RouterDevice device);
        
        Task<string> ExecuteCommandAsync(RouterDevice device, string command);
        Task<string[]> ExecuteCommandsAsync(RouterDevice device, string[] commands);
        
        Task<bool> UploadFileAsync(RouterDevice device, string localPath, string remotePath);
        Task<bool> DownloadFileAsync(RouterDevice device, string remotePath, string localPath);
        
        Task<Terminal> OpenTerminalAsync(RouterDevice device);
        Task CloseTerminalAsync(Terminal terminal);
        
        Task<SshConnectionInfo> GetConnectionInfoAsync(RouterDevice device);
    }
    
    public class Terminal
    {
        public string Id { get; set; }
        public RouterDevice Device { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public event EventHandler<TerminalDataEventArgs> DataReceived;
        
        public void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, new TerminalDataEventArgs { Data = data });
        }
    }
    
    public class TerminalDataEventArgs : EventArgs
    {
        public string Data { get; set; }
    }
    
    public class SshConnectionInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public bool IsPrivateKeyAuth { get; set; }
        public string PrivateKeyPath { get; set; }
        public bool IsConnected { get; set; }
        public DateTime ConnectedAt { get; set; }
        public string SessionId { get; set; }
    }
}