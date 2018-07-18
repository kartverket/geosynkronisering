
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 07/18/2018 10:48:16
-- Generated from EDMX file: C:\git\geosynk\Kartverket.Geosynkronisering.Server\Kartverket.Geosynkronisering\GeosyncModel.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [Datasets];
GO
    DROP TABLE [ServerConfigs];
GO
    DROP TABLE [Services];
GO
    DROP TABLE [StoredChangelogs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Datasets'
CREATE TABLE [Datasets] (
    [DatasetId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100)  NULL,
    [SchemaFileUri] nvarchar(256)  NULL,
    [DatasetProvider] nvarchar(256)  NULL,
    [ServerMaxCount] int  NULL,
    [DatasetConnection] nvarchar(256)  NULL,
    [DBSchema] nvarchar(100)  NULL,
    [TransformationConnection] nvarchar(256)  NULL,
    [DefaultCrs] nvarchar(30)  NULL,
    [UpperCornerCoords] nvarchar(30)  NULL,
    [LowerCornerCoords] nvarchar(30)  NULL,
    [TargetNamespace] nvarchar(256)  NULL,
    [TargetNamespacePrefix] nvarchar(100)  NULL,
    [Version] nvarchar(4000)  NULL,
    [Decimals] int  DEFAULT 3  NULL,
    [Tolerance] int  DEFAULT -1  NULL 
);
GO

-- Creating table 'ServerConfigs'
CREATE TABLE [ServerConfigs] (
    [FTPUrl] nvarchar(1024)  NULL,
    [FTPUser] nvarchar(100)  NULL,
    [FTPPwd] nvarchar(100)  NULL,
    [ID] int  NOT NULL
);
GO

-- Creating table 'Services'
CREATE TABLE [Services] (
    [Title] nvarchar(50)  NULL,
    [Abstract] nvarchar(200)  NULL,
    [Keywords] nvarchar(100)  NULL,
    [Fees] nvarchar(100)  NULL,
    [AccessConstraints] nvarchar(100)  NULL,
    [ProviderName] nvarchar(100)  NULL,
    [ProviderSite] nvarchar(255)  NULL,
    [IndividualName] nvarchar(100)  NULL,
    [Phone] nvarchar(15)  NULL,
    [Facsimile] nvarchar(15)  NULL,
    [Deliverypoint] nvarchar(100)  NULL,
    [City] nvarchar(100)  NULL,
    [PostalCode] nvarchar(4)  NULL,
    [Country] nvarchar(100)  NULL,
    [EMail] nvarchar(255)  NULL,
    [OnlineResourcesUrl] nvarchar(1024)  NULL,
    [HoursOfService] nvarchar(100)  NULL,
    [ContactInstructions] nvarchar(100)  NULL,
    [Role] nvarchar(100)  NULL,
    [ServiceURL] nvarchar(1024)  NULL,
    [ServiceID] nvarchar(100)  NOT NULL,
    [Namespace] nvarchar(4000)  NULL,
    [SchemaLocation] nvarchar(4000)  NULL
);
GO

-- Creating table 'StoredChangelogs'
CREATE TABLE [StoredChangelogs] (
    [Name] nvarchar(255)  NULL,
    [OrderUri] nvarchar(255)  NULL,
    [StartIndex] int  NULL,
    [DownloadUri] nvarchar(255)  NULL,
    [EndIndex] int  NULL,
    [Status] nvarchar(100)  NULL,
    [Stored] bit  NULL,
    [ChangelogId] int IDENTITY(1,1) NOT NULL,
    [DatasetId] int  NULL,
    [DateCreated] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [DatasetId] in table 'Datasets'
ALTER TABLE [Datasets]
ADD CONSTRAINT [PK_Datasets]
    PRIMARY KEY ([DatasetId] );
GO

-- Creating primary key on [ID] in table 'ServerConfigs'
ALTER TABLE [ServerConfigs]
ADD CONSTRAINT [PK_ServerConfigs]
    PRIMARY KEY ([ID] );
GO

-- Creating primary key on [ServiceID] in table 'Services'
ALTER TABLE [Services]
ADD CONSTRAINT [PK_Services]
    PRIMARY KEY ([ServiceID] );
GO

-- Creating primary key on [ChangelogId] in table 'StoredChangelogs'
ALTER TABLE [StoredChangelogs]
ADD CONSTRAINT [PK_StoredChangelogs]
    PRIMARY KEY ([ChangelogId] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------