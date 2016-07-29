using EasyOPC.Misc;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EasyOPC
{
    [ComVisible(true)]
    [Guid("D3ED0590-BEF6-428B-88C4-FA6DD8EA0DF8")]
    public class MonitoringManager : IMonitoringManager
    {
        #region Private members
        #region MonitoringManager specific members
        /// <summary>
        /// Endpoint url format
        /// </summary>
        private const string ENDPOINT_URL_FORMAT = "opc.tcp://{0}:{1}";

        /// <summary>
        /// List of available tags of the current session
        /// </summary>
        private List<ITag> _availableTags;

        /// <summary>
        /// The list of monitored tags
        /// </summary>
        private List<ITag> _monitoredTags;

        /// <summary>
        /// Monitoring session settings
        /// </summary>
        private ISettings _settings;

        /// <summary>
        /// Monitoring is initialized
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Monitoring is running
        /// </summary>
        private bool _monitoringRunning;

        /// <summary>
        /// Application name
        /// </summary>
        private string _applicationName;

        /// <summary>
        /// Name of the certificate
        /// </summary>
        private string _certificateName;

        /// <summary>
        /// Name of the session
        /// </summary>
        private string _sessionName;

        /// <summary>
        /// Action triggered when a tag had changed
        /// </summary>
        private Action<ITag> _onTagChanged;

        #endregion MonitoringManager pecific members

        #region OPC-UA related members

        /// <summary>
        /// OPC session config
        /// </summary>
        ApplicationConfiguration _config;

        /// <summary>
        /// OPC session
        /// </summary>
        Session _session;

        /// <summary>
        /// OPC monitoring subscription
        /// </summary>
        Subscription _monitoringSubscription;

        #endregion OPC-UA related members
        #endregion Private members

        #region Public Members
        /// <summary>
        /// Creates a monitoring instance
        /// </summary>
        public MonitoringManager()
        {
            Logger.LogInfo("Instantiating Monitoring Manager ..");

            _applicationName = "OPCDriver";
            _certificateName = "OPCDefaultCertificate";
            _sessionName = "OPCDefaultDriverSessionName";                                   

            //iniliaze lists
            _availableTags = new List<ITag>();
            _monitoredTags = new List<ITag>();

            Logger.LogInfo("Monitoring Manager instantiated !");
        }

        public IList<ITag> AvailableTags
        {
            get
            {
                return _availableTags;
            }
        }

        public IList<ITag> MonitoredTags
        {
            get
            {
                return _monitoredTags;
            }
        }
       
        public ISettings MonitoringInstanceSettings
        {
            get
            {
                return _settings;
            }
        }

        public bool Initialized
        {
            get
            {
                return _initialized;
            }
        }

        public bool MonitoringRunning
        {
            get
            {
                return _monitoringRunning;
            }
        }

        public IResult Init(ISettings settings)
        {

            Logger.LogInfo("Initializinc Monitoring session ...");
                    
            _settings = settings;

            try {

                // Init the OPC session configuration
                InitializeConfig();
                //Create session
                bool sessionCreated = CreateSession();                
                //Get available tags
                bool populateAvailableTags = PopulateAvailableTags();
                // is successfully initialized if session is created and tags retrieved
                _initialized = sessionCreated && populateAvailableTags;

                Logger.LogInfo(string.Format("Monitoring session initialized : {0} !", _initialized));

                return new Result(_initialized);
            }
            catch(Exception ex)
            {
                Logger.LogError(string.Format("Error while initializing Monitoring session . Error : {0} !", ex.Message));
                return new Result(false, ex);
            }            
        }

        public IResult Init(string ipAddress, int port, string instanceName, string certificateName)
        {
            ESettings settings = new ESettings(ipAddress, port, instanceName, certificateName);
            return Init(settings);
        }

        public IResult Init(string ipAddress, int port, string instanceName, string certificateName, int refreshInterval, int connectionTimeout)
        {            
            ESettings settings = new ESettings(ipAddress, port, instanceName, certificateName, refreshInterval, connectionTimeout);
            return Init(settings);
        }
      
        public IResult StartMonitoring(IList<ITag> tags)
        {
            Exception exception;                     
            bool monitoringStarted = StartMonitoringSubscription(tags,out exception);

            IResult result = new Result(monitoringStarted,exception);
            return result;
        }

        public IResult StartMonitoring()
        {
            return StartMonitoring(this.AvailableTags);
        }
       
        public IResult StopMonitoring()
        {
            bool stopped = StopMonitoringSubscription();
            IResult result = new Result(stopped);
            return result;
        }

        public IResult SubscribeToChangeEvents(Action<ITag> changeAction)
        {
            _onTagChanged = changeAction;

            IResult result = new Result(true);
            return result;
        }

        public IResult WriteTagValue(string tagID, object value)
        {
            return WriteValue(tagID, value);   
        }

        public void RefreshTag(string sTagID)
        {
            ReadAndUpdateTag(sTagID);
        }

        #endregion Public Members

        #region Private Methods

        #region Initialize && Create session
        /// <summary>
        /// Initializes ApplicationConfiguration object
        /// </summary>
        private void InitializeConfig()
        {
            Logger.LogInfo("Initializing OPC Application Configuration ...");
            _config = new ApplicationConfiguration()
            {
                ApplicationName = _applicationName,
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration { ApplicationCertificate = new CertificateIdentifier { StoreType = @"Windows", StorePath = @"CurrentUser\My", SubjectName = Utils.Format(@"CN={0}", _settings.CertificateName) }, TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Windows", StorePath = @"CurrentUser\TrustedPeople", }, NonceLength = 32, AutoAcceptUntrustedCertificates = true },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = _settings.ConnectionTimeout },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _settings.ConnectionTimeout }
            };
            _config.Validate(ApplicationType.Client);

            if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                _config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }
            Logger.LogInfo("OPC Application Configuration initialized !");
        }

        /// <summary>
        /// Initializes the session
        /// </summary>
        /// <returns></returns>
        private bool CreateSession()
        {
            Logger.LogInfo("Creating session ..");
            // no seting or no config -> no job
            if (_settings == null || _config == null) {
                Logger.LogWarning("Could not create session. App config or settings are null or invalid !");
                return false;
            }

            //no ip or no port -> do nothing
            if (string.IsNullOrEmpty(_settings.IPAddress) || _settings.Port == 0)
            {
                Logger.LogWarning("Could not create session. Settings are null or invalid !");
                return false;
            }

            //build Endpoint Url
            string sEndpointURL = string.Format(ENDPOINT_URL_FORMAT, _settings.IPAddress, _settings.Port);

            // Create an EndointDescription based on the EnpointUrl
            EndpointDescription endpointDescription = new EndpointDescription(sEndpointURL);

            //no endpoint, do nothing
            if (endpointDescription == null)
            {
                Logger.LogWarning("Could not create session. Could not create and EnpointDescription for given settings !");
                return false;
            }
            //Create a ConfgurationEndpoint object
            ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription);

            if (configuredEndpoint == null)
            {
                Logger.LogWarning("Could not create session. Could not create and ConfiguredEndpoint for given settings !");
                return false;
            }

            //Create the session 
            _session = Session.Create(_config, configuredEndpoint, true, _sessionName, (uint)_settings.ConnectionTimeout, null, null);

            if(_session == null)
            {
                Logger.LogWarning("Could not create session. Something went wrong !");
                return false;
            }

            Logger.LogInfo("Session created successfully !");
            return true;
        }
        #endregion
        #region Browse Tags, Create tags list , Recursive

        /// <summary>
        /// Populates the Available tags of the current monitoring session
        /// </summary>
        /// <returns></returns>
        private bool PopulateAvailableTags()
        {
            try
            {
                Logger.LogInfo("Populating available tags ...");

                //clear available tags
                _availableTags.Clear();
                //populate available tags
                _availableTags = BrowseAvailableTags();

                Logger.LogInfo("Available tags populated !");
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError(string.Format("Could not populate available tags . {0} !", ex.Message));

                return false;
            }
        }

        /// <summary>
        /// Gets available tags from system
        /// </summary>
        /// <returns></returns>
        private List<ITag> BrowseAvailableTags()
        {
            return BrowseNodesOfNode(null,string.Empty);            
        }
        
        /// <summary>
        /// Browse a node and its subnodes, recursively
        /// </summary>
        /// <param name="node">The current node</param>
        /// <param name="parentDisplayName">parent display</param>
        /// <returns></returns>
        private List<ITag> BrowseNodesOfNode(ReferenceDescription node,string parentDisplayName)
        {
            List<ITag> tags = new List<ITag>();
            ReferenceDescriptionCollection refsCollection;
            byte[] continuePoint;

            // if node is null, the node will be the root node 
            NodeId nodeID = node == null ? ObjectIds.ObjectsFolder : ExpandedNodeId.ToNodeId(node.NodeId, _session.NamespaceUris);

            //browse. store children in refColection variab;
            _session.Browse(null, null, nodeID, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out continuePoint, out refsCollection);
            
            // go trough the refCollection items and create ITag items
            foreach(ReferenceDescription refDesc in refsCollection)
            {
                // build display name of the tag
                string displayName = !string.IsNullOrEmpty(parentDisplayName) ? parentDisplayName + '.' + refDesc.DisplayName.Text: refDesc.DisplayName.Text;

                // if variable add it to list
                if (refDesc.NodeClass == NodeClass.Variable)
                {                    
                    ETag newTag = new ETag(refDesc.NodeId.ToString(), refDesc.NodeId.Identifier.ToString(), displayName);
                    tags.Add(newTag);                    
                }
                // get child tags if exists
                var childTags = BrowseNodesOfNode(refDesc, displayName);
                tags.AddRange(childTags);
            }

            return tags.ToList<ITag>();
        }

        /// <summary>
        /// Starts the monitoring subscription
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private bool StartMonitoringSubscription(IList<ITag> tags,out Exception exception)
        {
            Logger.LogInfo("Starting monitoring subscription ...");
            //allready runing 
            if (_monitoringRunning)
            {
                Logger.LogWarning("An existing monitoring subscription is already running !");
                exception =  new Exception("An existing monitoring subscription is already running !");
                return false;
            }

            try
            {
                //clear the monitored tags
                _monitoredTags.Clear();
                
                //Create a subscription object
                _monitoringSubscription = new Subscription(_session.DefaultSubscription);
                _monitoringSubscription.PublishingInterval = _settings.RefreshInterval;
                
                _monitoringSubscription.PublishingEnabled = true;

                //for every tag, create a MonitoredItem object and subscribe to Notification event
                foreach(ITag tag in tags)
                {
                    MonitoredItem item = new MonitoredItem(_monitoringSubscription.DefaultItem);
                    item.StartNodeId = tag.TagID;
                    item.Notification += Item_Notification;
                    item.DisplayName = tag.DisplayName;

                    _monitoringSubscription.AddItem(item);
                    //add tag to monitored tags collection
                    _monitoredTags.Add(tag);
                }

                //assign session with subscription
                _session.AddSubscription(_monitoringSubscription);

                //Create and start the subscription
                _monitoringSubscription.Create();

                bool created  = _monitoringSubscription.Created;

                // mark that monitoring is running                
                _monitoringRunning = created;

                Logger.LogInfo("Monitoring subscription created and started !");
                exception = created ? null : new Exception("Could not create a monitoring subscription !");
                return created;
            }
            catch(Exception ex)
            {
                exception = ex;
                Logger.LogError(string.Format("Could not create and start a monitoring subscription. Error : {0} !", ex.Message));
                _monitoringRunning = false;
                return false;
            }            
        }      

        /// <summary>
        /// Event fired when an iem is modified
        /// </summary>
        /// <param name="monitoredItem"></param>
        /// <param name="e"></param>
        private void Item_Notification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach(var value  in item.DequeueValues())
            {
                
                SetValeForTag(item.StartNodeId.ToString(), value.Value, (int)value.StatusCode.CodeBits, value.ServerTimestamp,true);
            }
        }

        /// <summary>
        /// Sets a value for a tag
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="value"></param>
        private void SetValeForTag(string tagID, object value, int quality,DateTime timeStamp, bool notify)
        {
            if (_monitoredTags == null) return;
            //find the tag by id
            ITag tag = _monitoredTags.Find(tg => tg.TagID.Equals(tagID));

            if (tag == null) return;
            // set the value for tag
            tag.SetValue(value,quality, timeStamp);

            // Notify that the value had changed
            if (notify) NotifyTagValueChanged(tag);
            
        }

        /// <summary>
        /// Notifyes the that that changed
        /// </summary>
        /// <param name="tag">Tag that changed</param>   
        private void NotifyTagValueChanged(ITag tag)
        {
            if (_onTagChanged != null)
                _onTagChanged(tag);
        }

        /// <summary>
        /// Stops monitoring process
        /// </summary>
        /// <returns></returns>
        private bool StopMonitoringSubscription()
        {
            try
            {
                // monitoring is stopped
                if (!_monitoringRunning) return true;

                Logger.LogInfo("Stopping monitoring subscription ...");
                               
                //stop publishing
                _monitoringSubscription.SetPublishingMode(false);

                // close session
                _session.CloseSession(null, true);

                //dispose monitoring subscription
                _monitoringSubscription.Dispose();

                _initialized = false;
                _monitoringRunning = false;

                Logger.LogInfo("Monitoring subscription stopped !");
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError(string.Format("Monitoring seubscription/session could not be stopped successfully . Error : {0}", ex.Message));
                return false;
            }
        }
        
        private ITag GetTagByID(string sTagID)
        {
            return _monitoredTags.FirstOrDefault(tg => tg.TagID.Equals(sTagID, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Writes a value to a tag
        /// </summary>
        /// <param name="sTagID">Tag id</param>
        /// <param name="value">value of the tag</param>
        /// <returns></returns>
        private IResult WriteValue(string sTagID,object value)
        {
            try
            {
                ITag tag = GetTagByID(sTagID);

                if (tag == null)
                    return new Result(false, new Exception("Could not find monitored tag !"));
                
                //Create collection
                WriteValueCollection writeValues = new WriteValueCollection();

                //Create a write value and set it's properties
                WriteValue writeValue = new Opc.Ua.WriteValue();
                writeValue.NodeId = new NodeId(sTagID);
                writeValue.AttributeId = Attributes.Value;

                // build and retrieve a Variant object for given tag
                Variant variant = GetVariantForTag(value, tag);
                //Set the writevalue datavalue
                writeValue.Value = new DataValue(variant);
                //add to the collection
                writeValues.Add(writeValue);
                                
                StatusCodeCollection statusCodes;
                DiagnosticInfoCollection diagnosticInfo;
                // Do the Write Opreation
                ResponseHeader response = _session.Write(null, writeValues, out statusCodes, out diagnosticInfo);

                return GetResultFromStatus(statusCodes);
            }
            catch(Exception ex)
            {
                Logger.LogError(string.Format("Could not write value for tag {1} . Error : {0}", ex.Message,sTagID));
                return new Result(false, ex);
            }
        }

        /// <summary>
        /// Builds a Variant object for a given value and a given type
        /// </summary>
        /// <param name="value">Value object</param>
        /// <param name="tag">Tag</param>
        /// <returns></returns>
        private Variant GetVariantForTag(object value,ITag tag)
        {
            if (tag.DataType != null)
                return new Variant(Convert.ChangeType(value, tag.DataType));
            else
                return new Variant(value);
        }

        /// <summary>
        /// Reads and returns a tag value
        /// </summary>
        /// <param name="tag">the tag for read</param>
        /// <returns></returns>
        private DataValue ReadTagValue(ITag tag)
        {
            try {
                DataValue value = _session.ReadValue(new NodeId(tag.TagID));
                return value;
            }
            catch(Exception ex)
            {
                Logger.LogError(string.Format("Could not read value for tag {1} . Error : {0}", ex.Message, tag.TagID));
                return null;
            }
        }

        /// <summary>
        /// Reads and updates a tag from the monitored tags collection
        /// </summary>
        /// <param name="sTagID"></param>
        private void ReadAndUpdateTag(string sTagID)
        {
            ITag tag = GetTagByID(sTagID);
            if(tag != null)
            {
                ReadAndUpdateTag(tag);
            }
        }

        /// <summary>
        /// Reads and updates a tag in monitored tags collection
        /// </summary>
        /// <param name="tag">Target tag</param>
        private void ReadAndUpdateTag(ITag tag)
        {
            DataValue tagValue = ReadTagValue(tag);
            if(tagValue !=null)
                SetValeForTag(tag.TagID, tagValue.Value, (int)tagValue.StatusCode.CodeBits, tagValue.ServerTimestamp, true);

        }

        /// <summary>
        /// Decodes a StatusCollection object and based on the status returns a Result object
        /// </summary>
        /// <param name="statusCodes"></param>
        /// <returns></returns>
        private Result GetResultFromStatus(StatusCodeCollection statusCodes)
        {
            if (statusCodes == null || statusCodes.Count == 0) return new Result(false, new Exception("Could not write tag value !"));
            bool isBad = false;
            isBad = statusCodes.Any(s => StatusCode.IsBad(s) );

            return new Result(!isBad);
        }
        #endregion

        #endregion Private Methods
    }
}
