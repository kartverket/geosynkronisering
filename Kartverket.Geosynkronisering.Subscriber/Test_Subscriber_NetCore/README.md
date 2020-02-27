
# Geosynkronisering CORESubscriber 

Lightweight console-subscriber for Geosynchronization written in .NET Core.

Version 1.2.3

The Subscriber.DL and Subscriber.BL has been rewritten to .net standard 2.0 for compability with .net core,
and is now used by the new core subscriper and by the Windows Application.


The previous subscriber for non-windows will be deprecated.
https://github.com/kartverket/CORESubscriber


## Installation
### Prebuilt binaries

Find releases for Windows and Linux here:

coming...

## Usage
### Adding datasets

#### Commandline
```
Subscriber_NetCore.exe add $serviceUrl $username $password $wfsUrl $datasetid
```

### Removing datasets

#### Commandline
```
Subscriber_NetCore.exe remove $datasetid1 $datasetid2 ... 
```

### Listing datasets

#### Commandline
```
Subscriber_NetCore.exe list $serviceUrl $username $password $wfsUrl $datasetid
```

### Reset datasets

#### Commandline
```
Subscriber_NetCore.exe reset $datasetid1 $datasetid2 ... 
```

### Synchronize datasets

#### Commandline
```
Subscriber_NetCore.exe sync $datasetid1 $datasetid2 ... 
```

### Synchronize all datasets

#### Commandline
```
Subscriber_NetCore.exe all
```

### Help

#### Commandline
```
Subscriber_NetCore.exe help
Subscriber_NetCore.exe help $command
```

## Build

more her...
