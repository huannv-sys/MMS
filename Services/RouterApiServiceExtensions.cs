using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikMonitor.Models;

namespace MikroTikMonitor.Services
{
    /// <summary>
    /// Extension methods for the RouterApiService
    /// </summary>
    public static class RouterApiServiceExtensions
    {
        /// <summary>
        /// Gets the log entries from a router
        /// </summary>
        /// <param name="routerApiService">The router API service</param>
        /// <param name="device">The router device</param>
        /// <param name="limit">The maximum number of log entries to retrieve</param>
        /// <returns>A collection of log entries</returns>
        public static async Task<IEnumerable<LogEntry>> GetLogEntriesAsync(this RouterApiService routerApiService, RouterDevice device, int limit = 100)
        {
            return await routerApiService.GetLogsAsync(device, limit);
        }
    }
}