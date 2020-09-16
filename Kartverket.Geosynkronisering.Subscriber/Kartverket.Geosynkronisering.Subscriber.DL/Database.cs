namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    internal class Database
    {
        public const string Schema = @"-- Script Date: 13.12.2019 15:17  - ErikEJ.SqlCeScripting version 3.5.2.76
-- Database information:
-- Locale Identifier: 1033
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\git\geosynkronisering2\Kartverket.Geosynkronisering.Subscriber\Kartverket.Geosynkronisering.Subscriber\geosyncDB.sdf
-- ServerVersion: 4.0.8482.1
-- DatabaseSize: 36 KB
-- SpaceAvailable: 3,999 GB
-- Created: 13.12.2019 12:27

-- User Table information:
-- Number of tables: 1
-- Dataset: -1 row(s)

SELECT 1;
PRAGMA foreign_keys = OFF;
        BEGIN TRANSACTION;
        CREATE TABLE[Dataset] (
       
         [DatasetId] INTEGER PRIMARY KEY NOT NULL
, [Name] nvarchar(100)  NULL
, [SyncronizationUrl] nvarchar(250)  NULL
, [ClientWfsUrl] nvarchar(250)  NULL
, [MaxCount] int DEFAULT 1000  NULL
, [ProviderDatasetId] nvarchar(100)  NULL
, [TargetNamespace] nvarchar(256)  NULL
, [MappingFile] nvarchar(260)  NULL
, [LastIndex] bigint DEFAULT 0  NULL
, [InitialSynchFinished] int DEFAULT 0  NULL
, [AbortedEndIndex]
        bigint NULL
, [AbortedTransaction] bigint NULL
, [AbortedChangelogPath] nvarchar(4000)  NULL
, [ChangelogDirectory] nvarchar(4000)  NULL
, [AbortedChangelogId] nvarchar(255)  NULL
, [UserName] nvarchar(255)  NULL
, [Password] nvarchar(255)  NULL
, [Version] nvarchar(255)  NULL
, [Tolerance]
        bigint NULL
, [EpsgCode] nvarchar(255)  NULL
, [Decimals] nvarchar(255)  NULL
);
CREATE UNIQUE INDEX[UQ__Dataset__0000000000000008] ON[Dataset] ([DatasetId] ASC);
COMMIT;

";
    }
}