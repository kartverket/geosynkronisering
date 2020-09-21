# Changelog

## 1.2.3

[Commits](https://github.com/kartverket/geosynkronisering/compare/1.2...1.2.3)

### Provider Version 1.2.3
Updated to .NET 4.7.2.
Fixed timeStamp error.
Fixed regression error introduced in Commit 59bc728) in BuildChangeLogFile.
nuget DotNetZip updated to v.1.13.4.
Feedback report from subscriber fixed for Provider in UX and by mail. Feedback report log on seperate file.

### Subscriber Version 1.2.3

The Subscriber.DL and Subscriber.BL has been rewritten to .NET Standard 2.0 for compability with .NET Core, and are now used by the new core subscriper and by the new Windows Application.

For better compability with .NET Standard and .NET Core:

Changed subscriber database to SQLite from SQL server Compact.
Updated and replaced some nuget packages.
Fixed events for .NET core version.

## 1.2

[Commits](https://github.com/kartverket/geosynkronisering/compare/1.1...1.2)

### Nye metoder:

GetPrecision: Returnerer antall desimaler, toleransen og EPSG-koden til koordinatene for gjeldende datasett

GetDatasetVersion: Spør etter datasettets versjon.Gjør at abonnenten kan finne ut om den må tømme og starte på nytt.

OrderChangelog2: Bestiller endringslogg for et datasett med datasetVersion. Returnerer -1 ved feil datasetVersion.

SendReport: Send rapport fra abonnent til tilbyder dersom det oppstår en feil på abonnenten

### Utviklingsverktøy:
Visual Studio 2017
.NET Framevork 4.6.2 (for å støtte TLS 1.2)
