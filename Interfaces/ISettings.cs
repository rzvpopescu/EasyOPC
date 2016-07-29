using System.Runtime.InteropServices;

namespace EasyOPC
{
    /// <summary>
    /// Settings interface
    /// </summary>
    [ComVisible(true)]
    [Guid("471CC19F-47DC-4CEB-B49A-2D5C84E448AC")]
    public interface ISettings
    {
        /// <summary>
        /// IP address or FQDN
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// Port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Refresh inreval
        /// </summary>
        int RefreshInterval { get; }

        /// <summary>
        /// Connection timeout
        /// </summary>
        int ConnectionTimeout { get;}

        /// <summary>
        /// Instance name of the server
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// Name of the certificate
        /// </summary>
        string CertificateName { get; }
        
    }
}
