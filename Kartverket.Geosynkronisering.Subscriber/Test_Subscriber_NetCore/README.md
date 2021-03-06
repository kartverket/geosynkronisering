﻿
# Geosynchronization CORESubscriber 

Lightweight console-subscriber for Geosynchronization written in .NET Core.

Version 1.2.3

The Subscriber.DL and Subscriber.BL has been rewritten to .net standard 2.0 for compability with .net core,
and are now used by the new core subscriper and by the Windows Application.


The previous subscriber for non-windows will be deprecated.
https://github.com/kartverket/CORESubscriber


## Installation
### Prebuilt binaries

Find releases for Windows and Linux here:

https://github.com/kartverket/geosynkronisering/releases

## Usage

### Help

#### Commandline
```
Subscriber_NetCore.exe help
Subscriber_NetCore.exe help $command
```

### Listing datasets

#### Commandline
```
Subscriber_NetCore.exe list || list $serviceUrl $username $password
```
If no more arguments are given, lists local datasets. Else lists datasets on specified provider
### Adding datasets

#### Commandline
```
Subscriber_NetCore.exe add $serviceUrl $username $password $wfsUrl [ $datasetid:name ]
```
Adds datasets from provider. If no datasetId is specified, all are added.

If we want to give the dataset a specific name we do this in the form ${datasetId}:${Name} using colon as seperator.

### Synchronize specified datasets

#### Commandline
```
Subscriber_NetCore.exe sync $datasetid1 $datasetid2 ... [--f]
```
Sync dataset(s) using local datasetId (found using list)
--f Skip prompt

### Synchronize all datasets

#### Commandline
```
Subscriber_NetCore.exe auto
```
Used for batch-running. Syncs all datasets without prompt


### Reset datasets

#### Commandline
```
Subscriber_NetCore.exe reset $datasetid1 $datasetid2 ... [--f]
```
Reset dataset(s)
--f Skip prompt

### Removing datasets

#### Commandline
```
Subscriber_NetCore.exe remove $datasetid1 $datasetid2 ... [--f]
```
Remove dataset(s)
--f Skip prompt

### Setting fields in database
```
Subscriber_NetCore.exe set $datasetId1 $datasetId2 ...  $fieldName1 $fieldValue1 $fieldName2 $fieldValue2 ...
```
Set fields for dataset(s) in sqlite database, e.g. path for field ChangelogDirectory

## Build

```
dotnet restore
dotnet build
```
