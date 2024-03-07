BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Subscribers" (
	"id"	INTEGER NOT NULL UNIQUE,
	"Url"	TEXT NOT NULL,
	"Username"	TEXT NOT NULL,
	"Password"	TEXT NOT NULL,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Datasets_Subscribers" (
	"id"	INTEGER NOT NULL UNIQUE,
	"DatasetId"	INTEGER NOT NULL,
	"SubscriberId"	INTEGER NOT NULL,
	"SubscriberDatasetId"	TEXT NOT NULL,
	"SubscriberDatasetName"	TEXT,
	FOREIGN KEY("SubscriberId") REFERENCES "Subscribers"("id"),
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "ServerConfigs" (
	"FTPUrl"	TEXT,
	"FTPUser"	TEXT,
	"FTPPwd"	TEXT,
	"ID"	INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS "Services" (
	"Title"	TEXT,
	"Abstract"	TEXT,
	"Keywords"	TEXT,
	"Fees"	TEXT,
	"AccessConstraints"	TEXT,
	"ProviderName"	TEXT,
	"ProviderSite"	TEXT,
	"IndividualName"	TEXT,
	"Phone"	TEXT,
	"Facsimile"	TEXT,
	"Deliverypoint"	TEXT,
	"City"	TEXT,
	"PostalCode"	TEXT,
	"Country"	TEXT,
	"EMail"	TEXT,
	"OnlineResourcesUrl"	TEXT,
	"HoursOfService"	TEXT,
	"ContactInstructions"	TEXT,
	"Role"	TEXT,
	"ServiceURL"	TEXT,
	"ServiceID"	TEXT,
	"Namespace"	TEXT,
	"SchemaLocation"	TEXT
);
CREATE TABLE IF NOT EXISTS "StoredChangelogs" (
	"Name"	TEXT,
	"OrderUri"	TEXT,
	"StartIndex"	INTEGER,
	"DownloadUri"	TEXT,
	"EndIndex"	INTEGER,
	"Status"	TEXT,
	"Stored"	INTEGER,
	"ChangelogId"	INTEGER NOT NULL,
	"DatasetId"	INTEGER,
	"DateCreated"	TEXT,
	PRIMARY KEY("ChangelogId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Datasets" (
	"DatasetId"	INTEGER NOT NULL,
	"Name"	TEXT,
	"SchemaFileUri"	TEXT,
	"DatasetProvider"	TEXT,
	"ServerMaxCount"	INTEGER,
	"DatasetConnection"	TEXT,
	"DBSchema"	TEXT,
	"TransformationConnection"	TEXT,
	"DefaultCrs"	TEXT,
	"UpperCornerCoords"	TEXT,
	"LowerCornerCoords"	TEXT,
	"TargetNamespace"	TEXT,
	"TargetNamespacePrefix"	TEXT,
	"Version"	TEXT,
	"Decimals"	TEXT,
	"Tolerance"	REAL NOT NULL,
	PRIMARY KEY("DatasetId" AUTOINCREMENT)
);
COMMIT;
