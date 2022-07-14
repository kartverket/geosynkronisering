# Geosynkronisering Provider 2.1 Beta.
-	The UI-part of the provider has got a new Blazor WEB application supporting Push.
All the tables for push can be edited in the program in addition to the existing tables.
-	For logging NLog has been replaced with the more modern Serilog in all projects.
-	The Push code has been added to a new .NET core project.
-	The Push-component now raises events that can be handled by both the non UI Provider push program and the new  and the Blazor program.

# Geosynkronisering Provider 2.0 Beta.
- The non UI-part of the Provider has been rewritten to .NET Standard 2.0 for compatibility with .NET Core, and are now used by the new core Provider  and by the ASP.Net Web Application.
- In the UI added Timestamp on Changelog.
For better compatibility with .NET Standard and .NET Core / .NET 5:
- Changed provider settings database to SQLite from SQL server Compact and replaced Entity Framework with Dapper.
- Updated and replaced NuGet packages.
Tools: Visual Studio 2019 .NET Framework 4.7.2 / .NET Standard 2.0 / .NET 5.


# Geosynkronisering

[Changelog](./CHANGELOG.md)

[UML Model](https://rawgit.com/kartverket/geosynkronisering/master/uml/HTML/index.htm)

[Downloads](https://github.com/kartverket/geosynkronisering/releases)

[Documentation for alternative subscriber for non-windows](https://github.com/kartverket/geosynkronisering/tree/master/Kartverket.Geosynkronisering.Subscriber/Test_Subscriber_NetCore)

[Deprecated version](https://github.com/kartverket/CORESubscriber)

## Oppdatering til sqlite i Subsscriber f.o.m. v 1.2.3

I versjon 1.2.3 ble abonnenten sin subscriber database endret til SQLite fra SQL Server Compact.

Konvertering skjer på følgende måte:

Last ned sqlite-tools-win32-x86-3310100.zip fra SQLite Download Page https://sqlite.org/download.html: A bundle of command-line tools for managing SQLite database files, including the command-line shell program, the sqldiff.exe program, and the sqlite3_analyzer.exe program.

SQL Server Compact to SQLite: http://erikej.blogspot.com/2013/03/sql-server-compact-code-snippet-of-week.html In order to create a SQLite database for the script file created (c:\temp\nwlite.sql), you can use the sqlite3.exe command line utility like so: sqlite3 nwlite.db < nwlite.sql

Eksempel:

Kopier inn følgende på samme mappe (i dette eksempelet NyTest):

Program: ExportSqlCe40.exe sqldiff.exe sqlite3.exe sqlite3_analyzer.exe

SQL server compact database: geosyncDB.sdf

Lage scriptet test.sql: C:\testSQLCompact2SQlite\NyTest>ExportSqlCe40.exe "Data Source=C:\testSQLCompact2SQlite\nytest\geosyncDB.sdf" test.sql sqlite

Import inn i ny sqlite base geosyncDB.db: sqlite3 geosyncDB.db < test.sql

## External tools

[A nice tool for creating featureStores and sql from xsd](https://github.com/JuergenWeichand/deegree-cli-utility)

[Handy tool for editing Sql Server Compact files (use the 4.0 version)](https://github.com/ErikEJ/SqlCeToolbox/releases)

## Development

### Autogenerated classes

#### WSDL svcutil

Diff showing needed edits to autogenerated code from svcutil: https://github.com/kartverket/geosynkronisering/commit/6b39e295fa45dc15b80a0e5eac6c95864386b855

### Building .NET Core Subscriber

Make sure you have .NET Core installed:

https://www.microsoft.com/net/core

### Publish

See https://github.com/dotnet/docs/blob/master/docs/core/rid-catalog.md#using-rids for RID

#### Windows 10 64bit

```
git clone https://github.com/kartverket/geosynkronisering.git

cd geosynkronisering/Kartverket.Geosynkronisering.Subscriber/Test_Subscriber_NetCore

dotnet publish -c Release --self-contained -r win10-x64
```

#### Ubuntu

```
git clone https://github.com/kartverket/geosynkronisering.git

cd geosynkronisering/Kartverket.Geosynkronisering.Subscriber/Test_Subscriber_NetCore

dotnet publish -c Release --self-contained -r ubuntu-x64
```

#### Mac

```
git clone https://github.com/kartverket/geosynkronisering.git

cd geosynkronisering/Kartverket.Geosynkronisering.Subscriber/Test_Subscriber_NetCore

dotnet publish -c Release --self-contained -r osx-x64
```
