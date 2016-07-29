using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;



namespace EasyOPC
{
    /// <summary>
    /// OPC Manager
    /// </summary>
    [ComVisible(true)]
    [Guid("ABAC6C50-2CF7-4FA4-B6D3-7094FE5E8494")]
    public interface IMonitoringManager
    {
        /// <summary>
        /// Initializes the connection 
        /// </summary>
        /// <param name="settings">Settings object</param>
        IResult Init(ISettings settings);

        /// <summary>
        /// Initializes monitoring manager
        /// </summary>
        /// <param name="ipAddress">Ip address of the OPC server</param>
        /// <param name="port">Port of the server</param>
        /// <param name="instanceName">Instance name of the server</param>
        /// <param name="certificateName">Name of the certificate</param>
        /// <returns>The result of the init operation</returns>
        IResult Init(string ipAddress, int port, string instanceName, string certificateName);

        /// <summary>
        /// Initializes monitoring manager
        /// </summary>
        /// <param name="ipAddress">Ip address of the OPC server</param>
        /// <param name="port">Port of the server</param>
        /// <param name="instanceName">Instance name of the server</param>
        /// <param name="certificateName">Name of the certificate</param>
        /// <param name="refreshInterval">Refresh interval</param>
        /// <param name="connectionTimeout">Connection timeout</param>
        /// <returns>The result of the init operation</returns>
        IResult Init(string ipAddress, int port, string instanceName, string certificateName, int refreshInterval, int connectionTimeout);

        /// <summary>
        /// Returns the current monitoring instance settings
        /// </summary>
        ISettings MonitoringInstanceSettings { get; }

        /// <summary>
        /// Returns available tags
        /// </summary>
        /// <returns></returns>
        IList<ITag> AvailableTags { get; }

        /// <summary>
        /// Returns monitored tags
        /// </summary>
        IList<ITag> MonitoredTags { get; }

        /// <summary>
        /// Starts the monitoring process for selected tags
        /// </summary>
        /// <returns></returns>
        IResult StartMonitoring(IList<ITag> tags);

        /// <summary>
        /// Starts the monitoring process for all available tags
        /// </summary>
        /// <returns></returns>
        IResult StartMonitoring();

        /// <summary>
        /// Wethever is initialized
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Whetever monitoring is running
        /// </summary>
        bool MonitoringRunning { get; }   

        /// <summary>
        /// Stops the monitoring process for prior monitored tags
        /// </summary>
        /// <returns></returns>
        IResult StopMonitoring();

        /// <summary>
        /// Subscribes to tags change events
        /// </summary>
        /// <param name="changeAction"></param>
        /// <returns></returns>
        IResult SubscribeToChangeEvents(Action<ITag> changeAction);

        /// <summary>
        /// Writes value for a given tag
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IResult WriteTagValue(string tagID, object value);

        /// <summary>
        /// Refreshes the tag from the OPC server
        /// </summary>
        /// <param name="sTagID"></param>
        void RefreshTag(string sTagID);

    }
}
