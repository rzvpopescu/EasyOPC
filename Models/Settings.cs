using System.Runtime.InteropServices;

namespace EasyOPC
{
    [ComVisible(true)]
    [Guid("9108B191-1A2C-451F-B925-1287AD0D7B4A")]
    public class ESettings : ISettings
    {        
        private string _ipAddress;
        private int _port;
        private string _instanceName;
        private string _certificateName;
        private int _refreshInterval = 1000;
        private int _connectionTimeout = 60000;        

        /// <summary>
        /// Creates a settings object
        /// </summary>
        /// <param name="ipAddress">Ip address of the OPC server</param>
        /// <param name="port">Port of the server</param>
        /// <param name="instanceName">Instance name of the server</param>
        /// <param name="certificateName">Name of the certificate</param>
        /// <param name="refreshInterval">Refresh interval</param>
        /// <param name="connectionTimeout">Connection timeout</param>
        public ESettings(string ipAddress,int port,string instanceName,string certificateName,int refreshInterval,int connectionTimeout)
            :this(ipAddress,port,instanceName,certificateName)
        {                       
            // set defaults if invalid
            _refreshInterval = refreshInterval > 500 ? refreshInterval:1000;            
            _connectionTimeout = connectionTimeout > 1000 ? connectionTimeout:60000;
        }

        /// <summary>
        /// Creates a Settings object with refresh timeout 1000 ms, and connection timeoute 60 seconds
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="instanceName"></param>
        /// <param name="certificateName"></param>
        public ESettings(string ipAddress, int port, string instanceName, string certificateName) 
        {
            _ipAddress = ipAddress;
            _port = port;
            _instanceName = instanceName;
            _certificateName = certificateName;            
        }        

        public int ConnectionTimeout
        {
            get
            {
                return _connectionTimeout;
            }
        }

        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
        }

        public string IPAddress
        {
            get
            {
                return _ipAddress;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public int RefreshInterval
        {
            get
            {
                return _refreshInterval;
            }
        }

        public string CertificateName
        {
            get
            {
                return _certificateName;
            }
        }
    }
}
