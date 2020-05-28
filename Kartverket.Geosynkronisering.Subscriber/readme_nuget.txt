20200504-Lars Eggan, NOIS

A nuget package has been built for Kartverket.Geosynkronisering.Subscriber.1.2.3.
Kartverket.Geosynkronisering.Subscriber.1.2.3.nupkg
Kartverket.Geosynkronisering.Subscriber.nuspec

For net472:
geosyncDB.sqlite (optional)
Kartverket.Geosynkronisering.Subscriber.exe
Kartverket.Geosynkronisering.Subscriber.exe.config
Kartverket.Geosynkronisering.Subscriber.DL.dll
Kartverket.Geosynkronisering.Subscriber.DL.dll

For netstandard2.0:
Kartverket.Geosynkronisering.Subscriber.DL.dll
Kartverket.Geosynkronisering.Subscriber.DL.dll


To publish:
nuget add Kartverket.Geosynkronisering.Subscriber.1.2.3.nupkg -source \\R61GIS-TFSBLD\Data\BuildDrop\GisNuGetFeed\

To delete:
nuget delete Kartverket.Geosynkronisering.Subscriber 1.2.3 -source \\R61GIS-TFSBLD\Data\BuildDrop\GisNuGetFeed\
