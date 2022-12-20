____________________________________________

**Endringslogg**

Inneholder alle merkbare endringer av løsningen

____________________________________________
<details>
  <summary markdown="span">Beta 2.1.5 - desember 2022</summary>

**Nytt**
-  Dokumentasjon med hjelp
-  Alle nuget-pakker oppdatert for .NET 6.
</details>

<details>
  <summary markdown="span">Beta 2.1.4 - desember 2022</summary>

**Nytt**
-  På siden Datasett abonnenter kan man nå velge SubscriberDatasetName fra en lise som valideres mot NGIS OPenAPI datasett tilgjengelig for gjeldende abonnent.
-  Loggfiler blir nå skrevet til mappen Logs\ undder der applikasjonen er installert.
-  Diverse forbedringer og feilrettinger.

</details>

<details>
  <summary markdown="span">Beta 2.1.3 - oktober 2022</summary>

**Nytt**
-  Fixed problem with push of large datasets (prevent too big headers).
-  Better logging.
-  Portion size for creation of initial changelog is now read from database.

</details>

<details>
  <summary markdown="span">Beta 2.1.0 - juli 2022</summary>

**Nytt**
- The UI-part of the provider has got a new Blazor WEB application supporting Push. All the tables for push can be edited in the program in addition to the existing tables.
- For logging NLog has been replaced with the more modern Serilog in all projects.
- The Push code has been added to a new .NET core project.
- The Push-component now raises events that can be handled by both the non UI Provider push program and the new and the Blazor program.

</details>



