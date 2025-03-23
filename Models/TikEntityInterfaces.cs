using System;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Interface for tik4net entity objects
    /// </summary>
    public interface ITikEntity
    {
        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        string Id { get; set; }
    }
}