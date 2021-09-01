using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dapper;
using Dapper.Contrib.Extensions;

namespace ChangelogManager
{
    public class geosyncEntities : IDisposable
    {
        public List<Dataset> Datasets { get; set; }
        public List<StoredChangelog> StoredChangelogs { get; set; }
        public List<Service> Services { get; set; }
        public List<ServerConfig> ServerConfigs { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static string ConnectionString
        {

            get { return GetConnectionStringFromSetting(); }

            // get { return System.Configuration.ConfigurationManager.ConnectionStrings["geosyncEntities"].ConnectionString; }

        }

        private static string GetConnectionStringFromSetting()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["geosyncEntities"].ConnectionString;

            // Microsoft.Data.Sqlite just passes the filename directly to the native SQLite engine.
            // It doesn't process the |DataDirectory| replacement string, so fix it
            var builder = new SQLiteConnectionStringBuilder(connectionString);
            builder.DataSource = builder.DataSource
                .Replace(
                    "|DataDirectory|",
                    AppDomain.CurrentDomain.GetData("DataDirectory") as string);
            connectionString = builder.ToString();

            return connectionString;

        }



        public geosyncEntities()
        {
            SqlMapperExtensions.TableNameMapper = type => type.Name;


            // Dapper: Problems with finding the right table: https://medium.com/@marco_be/dapper-contrib-custom-table-names-without-attribute-651746a8428b
            SqlMapperExtensions.TableNameMapper = entityType =>
            {
                if (entityType == typeof(Dataset))
                {
                    return "Datasets";
                }
                if (entityType == typeof(Service))
                {
                    return "Services";
                }

                if (entityType == typeof(ServerConfig))
                {
                    return "ServerConfigs";
                }

                if (entityType == typeof(StoredChangelog))
                {
                    return "StoredChangelogs";
                }


                throw new Exception($"Not supported entity type {entityType}");
            };


            //CreateDatabaseIfNotExists();

            Datasets = ReadAll<Dataset>("Datasets");

            StoredChangelogs = ReadAll<StoredChangelog>("StoredChangelogs");
            Services = ReadAll<Service>("Services");
            ServerConfigs = ReadAll<ServerConfig>("ServerConfigs");

        }
        public static List<T> ReadAll<T>(string tableName)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<T>("SELECT * FROM " + tableName).ToList();
        }

        //
        // DataSet part
        //
        private static bool DatasetExists(int datasetId)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM Datasets WHERE DatasetId = {datasetId}").ToList().FirstOrDefault() == 1;
        }

        private static void InsertDataset(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Insert(dataset);
        }

        public static void UpdateDataset(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(dataset);
        }

        public static void DeleteDataset(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(dataset);
        }

        public void DeleteObject(Dataset dataset)
        {
            var deletedDataset = Datasets.FirstOrDefault(d => d.DatasetId == dataset.DatasetId);

            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedDataset);

            Datasets.Remove(deletedDataset);
        }

        public void SaveChanges()
        {
            foreach (var dataset in Datasets)
            {
                if (DatasetExists(dataset.DatasetId)) UpdateDataset(dataset);
                else InsertDataset(dataset);
            }
        }

        public void AddObject(Dataset ds)
        {
            Datasets.Add(ds);
        }

        //StoredChangelogs part
        //public void DeleteObject(StoredChangelog changelog)
        //{
        //    var deletedChangelog = StoredChangelogs.FirstOrDefault(d => d.ChangelogId == changelog.ChangelogId);
        //    using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedChangelog);
        //    StoredChangelogs.Remove(deletedChangelog);
        //}

        //public void AddObject<T>(List<T>t)
        //{
        //    t.Add(t);
        //    //StoredChangelogs.Add(changelog);
        //}


    }

    public class ServerConfigsEntities : geosyncEntities
    {
        public static void UpdateServerConfig(ServerConfig serverConfig)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(serverConfig);
        }
    }

    public class ServicesEntities : geosyncEntities
    {
        public static void UpdateServices(Service service)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(service);
        }

    }

    public class StoredChangelogsEntities : geosyncEntities
    {
        public void AddObject(StoredChangelog changelog)
        {
            StoredChangelogs.Add(changelog);
        }

        public void DeleteObject(StoredChangelog changelog)
        {
            var deletedChangelog = StoredChangelogs.FirstOrDefault(d => d.ChangelogId == changelog.ChangelogId);
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedChangelog);
            StoredChangelogs.Remove(deletedChangelog);
        }

        public void SaveChanges()
        {
            foreach (var changelog in StoredChangelogs)
            {
                if (ChangelogExists(changelog.ChangelogId)) UpdateChangelog(changelog);
                else InsertChangelog(changelog);
            }
        }

        public static bool ChangelogExists(int changelogId)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM StoredChangelogs WHERE ChangelogId = {changelogId}").ToList().FirstOrDefault() == 1;
        }

        public static void UpdateChangelog(StoredChangelog changelog)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(changelog);
        }

        public static void InsertChangelog(StoredChangelog changelog)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Insert(changelog);
        }

        public static void DeleteChangelog(StoredChangelog changelog)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(changelog);
        }


    }

}

