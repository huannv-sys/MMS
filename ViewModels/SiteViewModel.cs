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
    /// ViewModel for site management
    /// </summary>
    public class SiteViewModel : INotifyPropertyChanged
    {
        private readonly CloudService _cloudService;
        private readonly List<CloudSite> _sites;
        private readonly List<RouterDevice> _routers;
        private CloudSite _selectedSite;
        private RouterDevice _selectedRouter;
        private bool _isLoading;
        private string _statusMessage;
        private string _searchText;
        private bool _showOnlySitesWithDevices;
        
        /// <summary>
        /// Gets the sites as an observable collection
        /// </summary>
        public ObservableCollection<CloudSite> Sites { get; }
        
        /// <summary>
        /// Gets the filtered sites as an observable collection
        /// </summary>
        public ObservableCollection<CloudSite> FilteredSites { get; }
        
        /// <summary>
        /// Gets the routers at the selected site as an observable collection
        /// </summary>
        public ObservableCollection<RouterDevice> RoutersAtSite { get; }
        
        /// <summary>
        /// Gets or sets the selected site
        /// </summary>
        public CloudSite SelectedSite
        {
            get => _selectedSite;
            set
            {
                if (SetProperty(ref _selectedSite, value))
                {
                    UpdateRoutersAtSite();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected router
        /// </summary>
        public RouterDevice SelectedRouter
        {
            get => _selectedRouter;
            set => SetProperty(ref _selectedRouter, value);
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
                    FilterSites();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether to show only sites with devices
        /// </summary>
        public bool ShowOnlySitesWithDevices
        {
            get => _showOnlySitesWithDevices;
            set
            {
                if (SetProperty(ref _showOnlySitesWithDevices, value))
                {
                    FilterSites();
                }
            }
        }
        
        /// <summary>
        /// Gets the command to refresh sites
        /// </summary>
        public ICommand RefreshSitesCommand { get; }
        
        /// <summary>
        /// Gets the command to add a new site
        /// </summary>
        public ICommand AddSiteCommand { get; }
        
        /// <summary>
        /// Gets the command to edit a site
        /// </summary>
        public ICommand EditSiteCommand { get; }
        
        /// <summary>
        /// Gets the command to delete a site
        /// </summary>
        public ICommand DeleteSiteCommand { get; }
        
        /// <summary>
        /// Gets the command to add a router to the selected site
        /// </summary>
        public ICommand AddRouterToSiteCommand { get; }
        
        /// <summary>
        /// Gets the command to remove a router from the selected site
        /// </summary>
        public ICommand RemoveRouterFromSiteCommand { get; }
        
        /// <summary>
        /// Gets the command to add all site routers to monitoring
        /// </summary>
        public ICommand MonitorAllRoutersCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the SiteViewModel class
        /// </summary>
        /// <param name="cloudService">The cloud service</param>
        /// <param name="sites">The list of sites</param>
        /// <param name="routers">The list of routers</param>
        public SiteViewModel(
            CloudService cloudService,
            List<CloudSite> sites,
            List<RouterDevice> routers)
        {
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
            _sites = sites ?? throw new ArgumentNullException(nameof(sites));
            _routers = routers ?? throw new ArgumentNullException(nameof(routers));
            
            // Create observable collections
            Sites = new ObservableCollection<CloudSite>(_sites);
            FilteredSites = new ObservableCollection<CloudSite>();
            RoutersAtSite = new ObservableCollection<RouterDevice>();
            
            // Set initial values
            _searchText = string.Empty;
            _showOnlySitesWithDevices = false;
            _isLoading = false;
            _statusMessage = "Ready";
            
            // Create commands
            RefreshSitesCommand = new RelayCommand(ExecuteRefreshSitesCommand);
            AddSiteCommand = new RelayCommand(ExecuteAddSiteCommand);
            EditSiteCommand = new RelayCommand(ExecuteEditSiteCommand, CanExecuteEditSiteCommand);
            DeleteSiteCommand = new RelayCommand(ExecuteDeleteSiteCommand, CanExecuteDeleteSiteCommand);
            AddRouterToSiteCommand = new RelayCommand(ExecuteAddRouterToSiteCommand, CanExecuteAddRouterToSiteCommand);
            RemoveRouterFromSiteCommand = new RelayCommand(ExecuteRemoveRouterFromSiteCommand, CanExecuteRemoveRouterFromSiteCommand);
            MonitorAllRoutersCommand = new RelayCommand(ExecuteMonitorAllRoutersCommand, CanExecuteMonitorAllRoutersCommand);
            
            // Filter sites
            FilterSites();
        }
        
        /// <summary>
        /// Filter sites based on search text and other criteria
        /// </summary>
        private void FilterSites()
        {
            FilteredSites.Clear();
            
            IEnumerable<CloudSite> filtered = _sites;
            
            // Filter by search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                string search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(s =>
                    s.Name.ToLowerInvariant().Contains(search) ||
                    (s.Description ?? "").ToLowerInvariant().Contains(search) ||
                    (s.Address ?? "").ToLowerInvariant().Contains(search));
            }
            
            // Filter by devices
            if (ShowOnlySitesWithDevices)
            {
                filtered = filtered.Where(s => s.DeviceCount > 0);
            }
            
            // Add filtered sites to collection
            foreach (var site in filtered)
            {
                FilteredSites.Add(site);
            }
            
            // Update property
            OnPropertyChanged(nameof(FilteredSites));
            
            // Select first site if none selected
            if (SelectedSite == null && FilteredSites.Count > 0)
            {
                SelectedSite = FilteredSites[0];
            }
        }
        
        /// <summary>
        /// Update the routers at the selected site
        /// </summary>
        private void UpdateRoutersAtSite()
        {
            RoutersAtSite.Clear();
            
            if (SelectedSite != null)
            {
                var routersAtSite = _routers.Where(r => r.SiteId == SelectedSite.Id).ToList();
                foreach (var router in routersAtSite)
                {
                    RoutersAtSite.Add(router);
                }
            }
            
            OnPropertyChanged(nameof(RoutersAtSite));
        }
        
        /// <summary>
        /// Execute the refresh sites command
        /// </summary>
        private async void ExecuteRefreshSitesCommand()
        {
            if (IsLoading)
                return;
                
            IsLoading = true;
            StatusMessage = "Refreshing sites...";
            
            try
            {
                // Get sites from cloud
                var cloudSites = await _cloudService.GetSitesAsync();
                
                // Update sites
                _sites.Clear();
                Sites.Clear();
                
                foreach (var site in cloudSites)
                {
                    _sites.Add(site);
                    Sites.Add(site);
                }
                
                // Filter sites
                FilterSites();
                
                StatusMessage = $"Refreshed {cloudSites.Count} sites";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing sites: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Execute the add site command
        /// </summary>
        private void ExecuteAddSiteCommand()
        {
            // TODO: Show dialog to add a new site
            StatusMessage = "Add site not implemented";
        }
        
        /// <summary>
        /// Determine whether the edit site command can be executed
        /// </summary>
        private bool CanExecuteEditSiteCommand()
        {
            return SelectedSite != null;
        }
        
        /// <summary>
        /// Execute the edit site command
        /// </summary>
        private void ExecuteEditSiteCommand()
        {
            // TODO: Show dialog to edit a site
            StatusMessage = "Edit site not implemented";
        }
        
        /// <summary>
        /// Determine whether the delete site command can be executed
        /// </summary>
        private bool CanExecuteDeleteSiteCommand()
        {
            return SelectedSite != null;
        }
        
        /// <summary>
        /// Execute the delete site command
        /// </summary>
        private void ExecuteDeleteSiteCommand()
        {
            // TODO: Show dialog to confirm deletion of a site
            StatusMessage = "Delete site not implemented";
        }
        
        /// <summary>
        /// Determine whether the add router to site command can be executed
        /// </summary>
        private bool CanExecuteAddRouterToSiteCommand()
        {
            return SelectedSite != null;
        }
        
        /// <summary>
        /// Execute the add router to site command
        /// </summary>
        private async void ExecuteAddRouterToSiteCommand()
        {
            if (SelectedSite == null || IsLoading)
                return;
                
            IsLoading = true;
            StatusMessage = "Adding router to site...";
            
            try
            {
                // Get devices for site
                var devices = await _cloudService.GetDevicesBySiteAsync(SelectedSite.Id);
                
                // TODO: Show dialog to select devices to add
                
                StatusMessage = $"Found {devices.Count} devices at site {SelectedSite.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding router to site: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Determine whether the remove router from site command can be executed
        /// </summary>
        private bool CanExecuteRemoveRouterFromSiteCommand()
        {
            return SelectedSite != null && SelectedRouter != null;
        }
        
        /// <summary>
        /// Execute the remove router from site command
        /// </summary>
        private void ExecuteRemoveRouterFromSiteCommand()
        {
            if (SelectedSite == null || SelectedRouter == null)
                return;
                
            // TODO: Show dialog to confirm removal of router from site
            StatusMessage = "Remove router from site not implemented";
        }
        
        /// <summary>
        /// Determine whether the monitor all routers command can be executed
        /// </summary>
        private bool CanExecuteMonitorAllRoutersCommand()
        {
            return SelectedSite != null && RoutersAtSite.Count > 0;
        }
        
        /// <summary>
        /// Execute the monitor all routers command
        /// </summary>
        private void ExecuteMonitorAllRoutersCommand()
        {
            if (SelectedSite == null || RoutersAtSite.Count == 0)
                return;
                
            // TODO: Mark all routers at site for monitoring
            foreach (var router in RoutersAtSite)
            {
                router.IsMonitored = true;
            }
            
            StatusMessage = $"Monitoring {RoutersAtSite.Count} routers at site {SelectedSite.Name}";
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
            if (propertyName == nameof(SelectedSite) || propertyName == nameof(SelectedRouter))
            {
                (EditSiteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteSiteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AddRouterToSiteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RemoveRouterFromSiteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MonitorAllRoutersCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
    
    // Removed duplicate RelayCommand class definition
    // The RelayCommand class is now defined in ViewModels/RelayCommand.cs
}