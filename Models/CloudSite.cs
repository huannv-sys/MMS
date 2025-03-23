using System;
using System.Text.Json.Serialization;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a site in MikroTik Cloud for API serialization
    /// </summary>
    public partial class CloudSiteDto
    {
        /// <summary>
        /// Gets or sets the unique ID of the site
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the site
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the site
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the address of the site
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the site location
        /// </summary>
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the site location
        /// </summary>
        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the contact person for the site
        /// </summary>
        [JsonPropertyName("contactPerson")]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Gets or sets the contact phone for the site
        /// </summary>
        [JsonPropertyName("contactPhone")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the contact email for the site
        /// </summary>
        [JsonPropertyName("contactEmail")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets when the site was created
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the site was last updated
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the number of devices at the site
        /// </summary>
        [JsonPropertyName("deviceCount")]
        public int DeviceCount { get; set; }

        /// <summary>
        /// Gets or sets the number of online devices at the site
        /// </summary>
        [JsonPropertyName("onlineDeviceCount")]
        public int OnlineDeviceCount { get; set; }

        /// <summary>
        /// Gets the percentage of online devices at the site
        /// </summary>
        public double OnlinePercentage => DeviceCount > 0 ? (double)OnlineDeviceCount / DeviceCount * 100 : 0;

        /// <summary>
        /// Gets or sets the status of the site based on online device percentage
        /// </summary>
        public string Status
        {
            get
            {
                if (DeviceCount == 0)
                    return "Empty";
                    
                if (OnlinePercentage == 100)
                    return "All Online";
                    
                if (OnlinePercentage >= 75)
                    return "Mostly Online";
                    
                if (OnlinePercentage >= 50)
                    return "Partially Online";
                    
                if (OnlinePercentage > 0)
                    return "Mostly Offline";
                    
                return "All Offline";
            }
        }
    }
}