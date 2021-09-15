BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Datasets" (
	"DatasetId"	int IDENTITY(1, 1) NOT NULL,
	"Name"	nvarchar(100),
	"SchemaFileUri"	nvarchar(256),
	"DatasetProvider"	nvarchar(256),
	"ServerMaxCount"	int,
	"DatasetConnection"	nvarchar(256),
	"DBSchema"	nvarchar(100),
	"TransformationConnection"	nvarchar(256),
	"DefaultCrs"	nvarchar(30),
	"UpperCornerCoords"	nvarchar(30),
	"LowerCornerCoords"	nvarchar(30),
	"TargetNamespace"	nvarchar(256),
	"TargetNamespacePrefix"	nvarchar(100),
	"Version"	nvarchar(4000),
	"Decimals"	nvarchar(4000),
	"Tolerance"	float NOT NULL
);
CREATE TABLE IF NOT EXISTS "ServerConfigs" (
	"FTPUrl"	nvarchar(1024),
	"FTPUser"	nvarchar(100),
	"FTPPwd"	nvarchar(100),
	"ID"	int NOT NULL
);
CREATE TABLE IF NOT EXISTS "Services" (
	"Title"	nvarchar(50),
	"Abstract"	nvarchar(200),
	"Keywords"	nvarchar(100),
	"Fees"	nvarchar(100),
	"AccessConstraints"	nvarchar(100),
	"ProviderName"	nvarchar(100),
	"ProviderSite"	nvarchar(255),
	"IndividualName"	nvarchar(100),
	"Phone"	nvarchar(15),
	"Facsimile"	nvarchar(15),
	"Deliverypoint"	nvarchar(100),
	"City"	nvarchar(100),
	"PostalCode"	nvarchar(4),
	"Country"	nvarchar(100),
	"EMail"	nvarchar(255),
	"OnlineResourcesUrl"	nvarchar(1024),
	"HoursOfService"	nvarchar(100),
	"ContactInstructions"	nvarchar(100),
	"Role"	nvarchar(100),
	"ServiceURL"	nvarchar(1024),
	"ServiceID"	nvarchar(100) NOT NULL,
	"Namespace"	nvarchar(4000),
	"SchemaLocation"	nvarchar(4000)
);
CREATE TABLE IF NOT EXISTS "StoredChangelogs" (
	"Name"	nvarchar(255),
	"OrderUri"	nvarchar(255),
	"StartIndex"	int,
	"DownloadUri"	nvarchar(255),
	"EndIndex"	int,
	"Status"	nvarchar(100),
	"Stored"	bit,
	"ChangelogId"	int IDENTITY(1, 1) NOT NULL,
	"DatasetId"	int,
	"DateCreated"	datetime
);
CREATE TABLE IF NOT EXISTS "Subscribers" (
	"id"	INTEGER NOT NULL UNIQUE,
	"Url"	TEXT NOT NULL,
	"Username"	TEXT NOT NULL,
	"Password"	TEXT NOT NULL,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Datasets_Subscribers" (
	"id"	INTEGER NOT NULL UNIQUE,
	"DatasetId"	INTEGER,
	"SubscriberId"	INTEGER,
	"SubscriberDatasetId"	INTEGER,
	PRIMARY KEY("id" AUTOINCREMENT),
	FOREIGN KEY("DatasetId") REFERENCES "Datasets"("DatasetId"),
	FOREIGN KEY("SubscriberId") REFERENCES "Subscribers"("id")
);
COMMIT;
