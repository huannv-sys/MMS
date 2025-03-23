using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MikroTikMonitor.Models;
using MikroTikMonitor.Services;

namespace MikroTikMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for MikroTik Cloud interaction
    /// </summary>
    public class CloudViewModel : INotifyPropertyChanged
    {
        private readonly CloudService _cloudService;
        private readonly List<RouterDevice> _routers;
        private string _username;
        private string _password;
        private bool _isAuthenticated;
        private bool _isLoading;
        private string _statusMessage;
        private string _searchText;
        private CloudDevice _selectedCloudDevice;
        private List<CloudDevice> _cloudDevices;
        
        /// <summary>
        /// Gets the cloud devices as an observable collection
        /// </summary>
        public ObservableCollection<CloudDevice> CloudDevices { get; }
        
        /// <summary>
        /// Gets the filtered cloud devices as an observable collection
        /// </summary>
        public ObservableCollection<CloudDevice> FilteredCloudDevices { get; }
        
        /// <summary>
        /// Gets or sets the username for MikroTik Cloud
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        
        /// <summary>
        /// Gets or sets the password for MikroTik Cloud
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        
        /// <summary>
        /// Gets or sets whether the user is authenticated with MikroTik Cloud
        /// </summary>
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetProperty(ref _isAuthenticated, value);
        }
        
        /// <summary>
        /// Gets or sets whether data is being loaded
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
        /// Gets or sets the search text
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterDevices();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected cloud device
        /// </summary>
        public CloudDevice SelectedCloudDevice
        {
            get => _selectedCloudDevice;
            set => SetProperty(ref _selectedCloudDevice, value);
        }
        
        /// <summary>
        /// Gets the command to login to MikroTik Cloud
        /// </summary>
        public ICommand LoginCommand { get; }
        
        /// <summary>
        /// Gets the command to refresh cloud devices
        /// </summary>
        public ICommand RefreshDevicesCommand { get; }
        
        /// <summary>
        /// Gets the command to add a cloud device to monitoring
        /// </summary>
        public ICommand AddDeviceCommand { get; }
        
        /// <summary>
        /// Gets the command to logout from MikroTik Cloud
        /// </summary>
        public ICommand LogoutCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the CloudViewModel class
        /// </summary>
        /// <param name="cloudService">The cloud service</param>
        /// <param name="routers">The list of routers</param>
        public CloudViewModel(
            CloudService cloudService,
            List<RouterDevice> routers)
        {
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
            _routers = routers ?? throw new ArgumentNullException(nameof(routers));
            
            // Initialize collections
            CloudDevices = new ObservableCollection<CloudDevice>();
            FilteredCloudDevices = new ObservableCollection<CloudDevice>();
            _cloudDevices = new List<CloudDevice>();
            
            // Set initial values
            _username = string.Empty;
            _password = string.Empty;
            _isAuthenticated = false;
            _isLoading = false;
            _statusMessage = "Not logged in";
            _searchText = string.Empty;
            
            // Create commands
            LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RefreshDevicesCommand = new RelayCommand(ExecuteRefreshDevicesCommand, CanExecuteRefreshDevicesCommand);
            AddDeviceCommand = new RelayCommand(ExecuteAddDeviceCommand, CanExecuteAddDeviceCommand);
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand, CanExecuteLogoutCommand);
        }
        
        /// <summary>
        /// Filter devices based on search text
        /// </summary>
        private void FilterDevices()
        {
            FilteredCloudDevices.Clear();
            
            IEnumerable<CloudDevice> filtered = _cloudDevices;
            
            // Filter by search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                string search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(d =>
                    d.Name.ToLowerInvariant().Contains(search) ||
                    d.Model.ToLowerInvariant().Contains(search) ||
                    d.SerialNumber.ToLowerInvariant().Contains(search) ||
                    (d.PublicIpAddress ?? "").ToLowerInvariant().Contains(search) ||
                    (d.MacAddress ?? "").ToLowerInvariant().Contains(search) ||
                    (d.SiteName ?? "").ToLowerInvariant().Contains(search));
            }
            
            // Add filtered devices to collection
            foreach (var device in filtered)
            {
                FilteredCloudDevices.Add(device);
            }
            
            // Update property
            OnPropertyChanged(nameof(FilteredCloudDevices));
        }
        
        /// <summary>
        /// Determine whether a cloud device is already being monitored
        /// </summary>
        /// <param name="cloudDevice">The cloud device to check</param>
        /// <returns>True if the device is already being monitored, otherwise false</returns>
        private bool IsDeviceMonitored(CloudDevice cloudDevice)
        {
            return _routers.Any(r => r.CloudId == cloudDevice.Id);
        }
        
        /// <summary>
        /// Determine whether the login command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteLoginCommand()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !IsAuthenticated && !IsLoading;
        }
        
        /// <summary>
        /// Execute the login command
        /// </summary>
        private async void ExecuteLoginCommand()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || IsAuthenticated || IsLoading)
                return;
                
            IsLoading = true;
            StatusMessage = "Logging in...";
            
            try
            {
                bool success = await _cloudService.AuthenticateAsync(Username, Password);
                
                if (success)
                {
                    IsAuthenticated = true;
                    StatusMessage = "Logged in successfully";
                    
                    // Get devices
                    await ExecuteRefreshDevicesCommandAsync();
                }
                else
                {
                    StatusMessage = "Login failed";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Determine whether the refresh devices command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteRefreshDevicesCommand()
        {
            return IsAuthenticated && !IsLoading;
        }
        
        /// <summary>
        /// Execute the refresh devices command
        /// </summary>
        private async void ExecuteRefreshDevicesCommand()
        {
            await ExecuteRefreshDevicesCommandAsync();
        }
        
        /// <summary>
        /// Execute the refresh devices command asynchronously
        /// </summary>
        private async Task ExecuteRefreshDevicesCommandAsync()
        {
            if (!IsAuthenticated || IsLoading)
                return;
                
            IsLoading = true;
            StatusMessage = "Refreshing devices...";
            
            try
            {
                // Get devices from cloud
                var devices = await _cloudService.GetDevicesAsync();
                
                // Update devices
                _cloudDevices.Clear();
                CloudDevices.Clear();
                
                foreach (var device in devices)
                {
                    _cloudDevices.Add(device);
                    CloudDevices.Add(device);
                }
                
                // Filter devices
                FilterDevices();
                
                StatusMessage = $"Refreshed {devices.Count} devices";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing devices: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Determine whether the add device command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteAddDeviceCommand()
        {
            return IsAuthenticated && !IsLoading && SelectedCloudDevice != null && !IsDeviceMonitored(SelectedCloudDevice);
        }
        
        /// <summary>
        /// Execute the add device command
        /// </summary>
        private void ExecuteAddDeviceCommand()
        {
            if (!IsAuthenticated || IsLoading || SelectedCloudDevice == null || IsDeviceMonitored(SelectedCloudDevice))
                return;
                
            // Create router device from cloud device
            var router = _cloudService.CreateRouterDeviceFromCloudDevice(SelectedCloudDevice);
            
            // Add to routers
            _routers.Add(router);
            
            StatusMessage = $"Added {router.Name} to monitoring";
            
            // Update command state
            (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        
        /// <summary>
        /// Determine whether the logout command can be executed
        /// </summary>
        /// <returns>True if the command can be executed, otherwise false</returns>
        private bool CanExecuteLogoutCommand()
        {
            return IsAuthenticated && !IsLoading;
        }
        
        /// <summary>
        /// Execute the logout command
        /// </summary>
        private void ExecuteLogoutCommand()
        {
            if (!IsAuthenticated || IsLoading)
                return;
                
            // Clear cloud data
            _cloudDevices.Clear();
            CloudDevices.Clear();
            FilteredCloudDevices.Clear();
            
            // Reset authentication state
            IsAuthenticated = false;
            Password = string.Empty;
            
            StatusMessage = "Logged out";
            
            // Update command states
            (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RefreshDevicesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Set a property value and raise the PropertyChanged event if the value changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">The backing field for the property</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the value was changed, otherwise false</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;
                
            storage = value;
            OnPropertyChanged(propertyName);
            
            // Update command states
            if (propertyName == nameof(IsAuthenticated) || propertyName == nameof(IsLoading) || 
                propertyName == nameof(Username) || propertyName == nameof(Password) ||
                propertyName == nameof(SelectedCloudDevice))
            {
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshDevicesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            
            return true;
        }
        
        /// <summary>
        /// Raise the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}