using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using MikroTikMonitor.Models;
using MikroTikMonitor.Services;

namespace MikroTikMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for the main window
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RouterApiService _routerApiService;
        private readonly SnmpService _snmpService;
        private readonly StatisticsService _statisticsService;
        private readonly List<RouterDevice> _routers;
        private RouterDevice _selectedRouter;
        private bool _isConnecting;
        private string _statusMessage;
        private ChartViewModel _chartVM;
        private System.Timers.Timer _updateTimer;
        
        /// <summary>
        /// Gets the routers as an observable collection
        /// </summary>
        public ObservableCollection<RouterDevice> Routers { get; }
        
        /// <summary>
        /// Gets or sets the selected router
        /// </summary>
        public RouterDevice SelectedRouter
        {
            get => _selectedRouter;
            set => SetProperty(ref _selectedRouter, value);
        }
        
        /// <summary>
        /// Gets or sets whether a connection is in progress
        /// </summary>
        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }
        
        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// Gets the chart view model
        /// </summary>
        public ChartViewModel ChartVM
        {
            get => _chartVM;
            private set => SetProperty(ref _chartVM, value);
        }
        
        /// <summary>
        /// Gets the command to add a new router
        /// </summary>
        public ICommand AddRouterCommand { get; }
        
        /// <summary>
        /// Gets the command to remove a router
        /// </summary>
        public ICommand RemoveRouterCommand { get; }
        
        /// <summary>
        /// Gets the command to connect to a router
        /// </summary>
        public ICommand ConnectCommand { get; }
        
        /// <summary>
        /// Gets the command to disconnect from a router
        /// </summary>
        public ICommand DisconnectCommand { get; }
        
        /// <summary>
        /// Gets the command to refresh data
        /// </summary>
        public ICommand RefreshCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class
        /// </summary>
        /// <param name="routerApiService">The router API service</param>
        /// <param name="snmpService">The SNMP service</param>
        /// <param name="statisticsService">The statistics service</param>
        /// <param name="routers">The list of routers</param>
        public MainViewModel(
            RouterApiService routerApiService,
            SnmpService snmpService,
            StatisticsService statisticsService,
            List<RouterDevice> routers)
        {
            _routerApiService = routerApiService ?? throw new ArgumentNullException(nameof(routerApiService));
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _routers = routers ?? throw new ArgumentNullException(nameof(routers));
            
            // Create observable collection from routers list
            Routers = new ObservableCollection<RouterDevice>(_routers);
            
            // Set selected router if there are any
            if (Routers.Count > 0)
                SelectedRouter = Routers[0];
            
            // Create commands
            AddRouterCommand = new RelayCommand(ExecuteAddRouterCommand);
            RemoveRouterCommand = new RelayCommand(ExecuteRemoveRouterCommand, CanExecuteRemoveRouterCommand);
            ConnectCommand = new RelayCommand(ExecuteConnectCommand, CanExecuteConnectCommand);
            DisconnectCommand = new RelayCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand);
            RefreshCommand = new RelayCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand);
            
            // Initialize chart view model
            ChartVM = new ChartViewModel();
            
            // Set initial status
            StatusMessage = "Ready";
        }
        
        /// <summary>
        /// Executes the add router command
        /// </summary>
        private void ExecuteAddRouterCommand()
        {
            var newRouter = new RouterDevice
            {
                Id = Guid.NewGuid().ToString(),
                Name = "New Router",
                IpAddress = "192.168.1.1",
                Port = 8728,
                Username = "admin",
                Password = "",
                UseSnmp = false,
                SnmpCommunity = "public",
                SnmpPort = 161
            };
            
            _routers.Add(newRouter);
            Routers.Add(newRouter);
            SelectedRouter = newRouter;
            
            StatusMessage = "Added new router";
        }
        
        /// <summary>
        /// Determines whether the remove router command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteRemoveRouterCommand()
        {
            return SelectedRouter != null;
        }
        
        /// <summary>
        /// Executes the remove router command
        /// </summary>
        private void ExecuteRemoveRouterCommand()
        {
            if (SelectedRouter == null)
                return;
                
            // Disconnect if connected
            if (SelectedRouter.IsConnected)
                _routerApiService.Disconnect(SelectedRouter);
                
            // Remove from collections
            _routers.Remove(SelectedRouter);
            Routers.Remove(SelectedRouter);
            
            // Select another router if available
            if (Routers.Count > 0)
                SelectedRouter = Routers[0];
            else
                SelectedRouter = null;
                
            StatusMessage = "Removed router";
        }
        
        /// <summary>
        /// Determines whether the connect command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteConnectCommand()
        {
            return SelectedRouter != null && !SelectedRouter.IsConnected && !IsConnecting;
        }
        
        /// <summary>
        /// Executes the connect command
        /// </summary>
        private async void ExecuteConnectCommand()
        {
            if (SelectedRouter == null)
                return;
                
            IsConnecting = true;
            StatusMessage = "Connecting...";
            
            try
            {
                bool success = await _routerApiService.ConnectAsync(SelectedRouter);
                
                if (success)
                {
                    StatusMessage = "Connected";
                    
                    // Get initial data
                    await _routerApiService.GetSystemInfoAsync(SelectedRouter);
                    await _routerApiService.GetNetworkInterfacesAsync(SelectedRouter);
                    await _routerApiService.GetDhcpLeasesAsync(SelectedRouter);
                    await _routerApiService.GetLogEntriesAsync(SelectedRouter, 100);
                    
                    // Start update timer for charts
                    StartUpdateTimer();
                    
                    // Update command states
                    OnPropertyChanged(nameof(SelectedRouter));
                }
                else
                {
                    StatusMessage = "Failed to connect: " + SelectedRouter.ConnectionStatus;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Connection error: " + ex.Message;
            }
            finally
            {
                IsConnecting = false;
            }
        }
        
        /// <summary>
        /// Determines whether the disconnect command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteDisconnectCommand()
        {
            return SelectedRouter != null && SelectedRouter.IsConnected && !IsConnecting;
        }
        
        /// <summary>
        /// Executes the disconnect command
        /// </summary>
        private void ExecuteDisconnectCommand()
        {
            if (SelectedRouter == null)
                return;
                
            _routerApiService.Disconnect(SelectedRouter);
            StatusMessage = "Disconnected";
            
            // Stop update timer
            StopUpdateTimer();
            
            // Update command states
            OnPropertyChanged(nameof(SelectedRouter));
        }
        
        /// <summary>
        /// Determines whether the refresh command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteRefreshCommand()
        {
            return SelectedRouter != null && SelectedRouter.IsConnected && !IsConnecting;
        }
        
        /// <summary>
        /// Executes the refresh command
        /// </summary>
        private async void ExecuteRefreshCommand()
        {
            if (SelectedRouter == null)
                return;
                
            IsConnecting = true;
            StatusMessage = "Refreshing...";
            
            try
            {
                await _routerApiService.GetSystemInfoAsync(SelectedRouter);
                await _routerApiService.GetNetworkInterfacesAsync(SelectedRouter);
                await _routerApiService.GetDhcpLeasesAsync(SelectedRouter);
                await _routerApiService.GetLogEntriesAsync(SelectedRouter, 100);
                
                StatusMessage = "Refreshed";
            }
            catch (Exception ex)
            {
                StatusMessage = "Refresh error: " + ex.Message;
            }
            finally
            {
                IsConnecting = false;
            }
        }
        
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">Reference to the backing field</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the value was changed, otherwise false</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;
                
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            // Update command states
            if (propertyName == nameof(SelectedRouter) || propertyName == nameof(IsConnecting))
            {
                (RemoveRouterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        
        /// <summary>
        /// Starts the update timer for chart data
        /// </summary>
        private void StartUpdateTimer()
        {
            // Stop any existing timer
            StopUpdateTimer();
            
            // Create and configure timer
            _updateTimer = new System.Timers.Timer(1000); // Update every second
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }
        
        /// <summary>
        /// Stops the update timer
        /// </summary>
        private void StopUpdateTimer()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Elapsed -= OnUpdateTimerElapsed;
                _updateTimer.Dispose();
                _updateTimer = null;
            }
            
            // Clear chart data
            if (ChartVM != null)
            {
                // Need to run on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ChartVM.ClearChartData();
                });
            }
        }
        
        /// <summary>
        /// Handler for timer elapsed event
        /// </summary>
        private async void OnUpdateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (SelectedRouter == null || !SelectedRouter.IsConnected)
                return;
                
            try
            {
                // Get resource usage data
                await _routerApiService.GetSystemResourcesAsync(SelectedRouter);
                await _routerApiService.GetInterfaceStatisticsAsync(SelectedRouter);
                
                // Update chart data
                UpdateChartData();
            }
            catch (Exception ex)
            {
                // Log error but don't show to user (it's just a background update)
                System.Diagnostics.Debug.WriteLine($"Error updating chart data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Updates chart data from current router state
        /// </summary>
        private void UpdateChartData()
        {
            if (SelectedRouter == null || ChartVM == null)
                return;
                
            // Need to run on UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                double timestamp = DateTime.Now.Subtract(DateTime.Today).TotalSeconds;
                
                // CPU and memory chart data
                if (SelectedRouter.ResourceHistory.Count > 0)
                {
                    var latestResource = SelectedRouter.ResourceHistory[SelectedRouter.ResourceHistory.Count - 1];
                    ChartVM.UpdateCpuData(timestamp, latestResource.CpuUsage);
                    ChartVM.UpdateMemoryData(timestamp, latestResource.MemoryUsage);
                }
                
                // Traffic charts
                if (SelectedRouter.Interfaces.Count > 0)
                {
                    // Get the first interface that has stats (typically ether1)
                    var iface = SelectedRouter.Interfaces.FirstOrDefault();
                    if (iface != null)
                    {
                        // Convert to bps (bits per second)
                        double rxRate = iface.RxBytesPerSecond * 8;
                        double txRate = iface.TxBytesPerSecond * 8;
                        
                        ChartVM.UpdateRxData(timestamp, rxRate);
                        ChartVM.UpdateTxData(timestamp, txRate);
                    }
                }
            });
        }
    }
    
    /// <summary>
    /// A command that calls a delegate to execute and a delegate to determine if it can execute
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        
        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">The execute delegate</param>
        /// <param name="canExecute">The can execute delegate</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        /// <summary>
        /// Occurs when the can execute state changes
        /// </summary>
        public event EventHandler CanExecuteChanged;
        
        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        /// <returns>True if this command can be executed, otherwise false</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }
        
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter)
        {
            _execute();
        }
        
        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}