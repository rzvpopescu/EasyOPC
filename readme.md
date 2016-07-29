
## Easy OPC
Eeasy OPC is a .NET wrapper over the [OPC Foundation UA .NET](https://github.com/OPCFoundation/UA-.NET) library and can be used for OPC Server tags/singals retrieval,monitoring and updating in real time.


### Prerequiesites
- A 2048 bits key size valid certificate, for the "CurrentUser" store.


### How to use it

- Create an instance of MonitoringManager class:

```C#
IMonitoringManager manager =  new MonitoringManager();
```

- Initialize the instance with data about he monitored OPC server:

```C#
IResult result = manager.Init(ipAddress,iPort,OPCServerInstanceName,CertificateName);
```

 or you can use the Settings class :

```C#
ISettings settings =  new Settings(ipAddress,iPort,OPCServerInstanceName,CertificateName);

manager.Init(settings);
```
- Retrieve the tags from the OPC Server:

```C#
List<Tag> tags = manager.AvailableTags;	
```

- Subscribe to tags changes :

```C#
manager.SuSubscribeToChangeEvents((changedTag)=>{
	Console.WriteLine(String.Format("Tag {0} has a new value of {1} ",changedTag.DisplayName,changedTag.Value.ToString()));
});
```

- Start the OPC server monitoring process. 

```C#
IResult result = manager.StartMonitoring();

if(!result.Success)
{ 
 	throw result.Exception;
}
```
The above code monitors all the available tags for changes.

To monitor just for a certain tag or tags changes you can use the ``StartMonitoring()`` method overload:

```C#
List<Tag> selectedTags = manager.AvailableTags.Where(tag=> ... some condition here ..);
manager.StartMonitoring(selectedTags);
```

- To retrieve the monitored tags (the tags passed to the ``StartMonitoring()`` method) can be used the *MonitoredTags* property :
```C#
var monitoredTags = manager.MonitoredTags;
```

- Force a tag refresh. The value and the quelity of the tag will be reloaded from the OPC Server and will update the given tag.
```C#
manager.RefreshTag(tagID);
```

-  Write a tag value, for a writable tag.
```C#
manager.WriteTagValue(tagID,tagValue);
```


- Stop the monitoring process
```C#
manager.StopMonitoring()
```

