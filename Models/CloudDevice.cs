using System;
using System.Text.Json.Serialization;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a MikroTik device registered in MikroTik Cloud
    /// </summary>
    public class CloudDevice
    {
        /// <summary>
        /// Gets or sets the unique ID of the device
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the device
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the model of the device
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the serial number of the device
        /// </summary>
        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the MAC address of the device
        /// </summary>
        [JsonPropertyName("macAddress")]
        public string MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the public IP address of the device
        /// </summary>
        [JsonPropertyName("publicIpAddress")]
        public string PublicIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the RouterOS version of the device
        /// </summary>
        [JsonPropertyName("routerOsVersion")]
        public string RouterOsVersion { get; set; }

        /// <summary>
        /// Gets or sets whether VPN is enabled for the device
        /// </summary>
        [JsonPropertyName("isVpnEnabled")]
        public bool IsVpnEnabled { get; set; }

        /// <summary>
        /// Gets or sets the status of the device
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets when the device was last seen
        /// </summary>
        [JsonPropertyName("lastSeen")]
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Gets or sets when the device was last updated
        /// </summary>
        [JsonPropertyName("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// Gets or sets the site ID of the device
        /// </summary>
        [JsonPropertyName("siteId")]
        public string SiteId { get; set; }

        /// <summary>
        /// Gets or sets the site name of the device
        /// </summary>
        [JsonPropertyName("siteName")]
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the device
        /// </summary>
        [JsonPropertyName("metadata")]
        public CloudDeviceMetadata Metadata { get; set; }
    }

    /// <summary>
    /// Represents metadata for a MikroTik cloud device
    /// </summary>
    public class CloudDeviceMetadata
    {
        /// <summary>
        /// Gets or sets the latitude of the device location
        /// </summary>
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the device location
        /// </summary>
        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the address of the device location
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the contact person for the device
        /// </summary>
        [JsonPropertyName("contactPerson")]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Gets or sets the contact phone for the device
        /// </summary>
        [JsonPropertyName("contactPhone")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the contact email for the device
        /// </summary>
        [JsonPropertyName("contactEmail")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the notes for the device
        /// </summary>
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }
}