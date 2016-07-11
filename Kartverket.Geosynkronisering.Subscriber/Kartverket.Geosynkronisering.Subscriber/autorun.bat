@ECHO OFF
REM Tries to synk all datasets.
REM To specify which, replace auto with datasetId (this can be a space-separated list)

Kartverket.Geosynkronisering.Subscriber.exe auto

REM pause