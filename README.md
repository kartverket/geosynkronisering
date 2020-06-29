# Geosynkronisering

## Subscriber Version 1.2.3
The Subscriber.DL and Subscriber.BL has been rewritten to .NET Standard 2.0 for compability with .NET Core,
and are now used by the new core subscriper and by the new Windows Application.

For better compability with .NET Standard and .NET Core:
- Changed subscriber database to SQLite from SQL server Compact.
- Updated and replaced some nuget packages.
- Fixed events for .NET core version.



## Downloads
More here for new version download...

Old Windows version:
https://github.com/kartverket/geosynkronisering/wiki/Download

## Alternative subscriber for non-windows
https://github.com/kartverket/geosynkronisering/tree/fixDotnetstandard/Kartverket.Geosynkronisering.Subscriber/Test_Subscriber_NetCore

Deprecated version: https://github.com/kartverket/CORESubscriber

## Provider Version 1.2.3
- Updated to .NET 4.7.2.
- Fixed timeStamp error.
- Fixed regression error introduced in Commit 59bc728) in BuildChangeLogFile.
- nuget DotNetZip updated to v.1.13.4.
- Feedback report from subscriber fixed for Provider in UX and by mail. Feedback report log on seperate file.
