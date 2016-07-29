
## Easy OPC
Eeasy OPC is a .NET wrapper over the [OPC Foundation UA .NET](https://github.com/OPCFoundation/UA-.NET) library.


### Prerequiesites
- A 2048 bits key size valid certificate, for the "CurrentUser" store.


### How to use it

1. Create an instance of MonitoringManager class:

```C#
IMonitoringManager manager =  new MonitoringManager();
```

2. Initialize the instance with data about he monitored OPC server:

```C#
IResult result = manager.Init(ipAddress,iPort,OPCServerInstanceName,CertificateName);
```

 or you can use the Settings class :

```C#
ISettings settings =  new Settings(ipAddress,iPort,OPCServerInstanceName,CertificateName);

manager.Init(settings);
```
3. Retrieve the tags from the OPC Server:

```C#
List<Tag> tags = manager.AvailableTags;	
```

4. Subscrie to tags changes :

```C#
manager.SuSubscribeToChangeEvents((changedTag)=>{
	Console.WriteLine(String.Format("Tag {0} has a new value of {1} ",changedTag.DisplayName,changedTag.Value.ToString()));
});
```

5. Start the OPC server monitoring process. 

```C#
IResult result = manager.StartMonitoring();

if(!result.Success)
{ 
 	throw result.Exception;
}
```
The above code monitors all the available tags for changes.
To monitor just for a certain tag or tags changes you can use the ``StartMonitoring()`` method overload.

```C#
List<Tag> selectedTags = manager.AvailableTags.Where(tag=> ... some condition here ..);
manager.StartMonitoring(selectedTags);
```


