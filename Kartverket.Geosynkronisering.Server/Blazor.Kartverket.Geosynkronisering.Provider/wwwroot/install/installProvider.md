____________________________________________

**Installasjon av tilbyder**

Det anbefales at Web-pusher og Kommando-basert pusher bruker sqlite database.
Det beste er å legge opp en slik struktur der man legger geosync.sqlite på en egen mappe, sqlite:</br>
![Sqlite](sqlite.png)

Man kan angi direkte path til sqlite base ved å legge det inn i config-filene.</br>
Fra:</br>
`"connectionStrings": {
    "geosyncEntities": "Data Source=|DataDirectory|geosync.sqlite;Mode=ReadWriteCreate"
  },`</br>
Til f.eks: </br>
`"connectionStrings": {
    "geosyncEntities": "Data Source="geosyncEntities": "Data Source=D:\\Geosynkronisering\\TilbyderPushDes2022\\sqlite\\geosync.sqlite;Mode=ReadWriteCreate"Mode=ReadWriteCreate"
  },`

Config-fil WEB: appsettings.json</br>
Config-fil .NET core: appsettings.json


____________________________________________

**Installere Web tilbyder i IIS:**

Installer .Net core hosting bundle på serveren først.</br>
For .NET 6: https://dotnet.microsoft.com/en-us/download/dotnet


I Application Pools:</br>
![Iis Apppool](iis_apppool.png)

Under site:
![Iis Site](iis_site.png)
Advanced settings:
![Iis Advanced](iis_advanced.png)

**For Kommando-basert pusher:**

Dersom det er problem med å kjøre, installer:
https://dotnet.microsoft.com/download/dotnet/5.0