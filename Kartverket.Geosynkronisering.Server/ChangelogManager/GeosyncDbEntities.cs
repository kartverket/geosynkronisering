using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChangelogManager
{
    public class geosyncEntities : IDisposable
    {
        public List<Dataset> Datasets { get; set; }
        public List<StoredChangelog> StoredChangelogs { get; set; }
        public List<Service> Services { get; set; }
        public List<ServerConfig> ServerConfigs { get; set; }
        public List<NgisSubscriber> Subscribers { get; set; }
        public List<Datasets_NgisSubscriber> Datasets_Subscribers { get; set; }

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
            var connectionString = "";
            bool isConfigOK;
            
            try
            {
                // Supports .net Core appsettings.json
                var config = JsonConfig.SetupJsonConfig();
                connectionString = config.GetValue<string>("connectionStrings:geosyncEntities");
                //connectionString = config.GetSection("connectionStrings:geosyncEntities").ToString();
                isConfigOK = true;
            }
            catch (System.NullReferenceException e)
            {
                // Older applications typically uses app.config / web.config with XML
                connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["geosyncEntities"]
                    .ConnectionString;
                isConfigOK = true;
            }

            catch (Exception e)
            {
                throw;
            }
            

            // Microsoft.Data.Sqlite just passes the filename directly to the native SQLite engine.
            // It doesn't process the |DataDirectory| replacement string, so fix it
            if (isConfigOK)
            {
            
                
                if (false)
                {
                    // Test: Will not work for .NET Core if not set in main program with appDomain.CurrentDomain.SetData("DataDirectory")
                    var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                }
                
                var builder = new SQLiteConnectionStringBuilder(connectionString);
                builder.DataSource = builder.DataSource
                    .Replace(
                        "|DataDirectory|",
                        AppDomain.CurrentDomain.GetData("DataDirectory") as string);
                connectionString = builder.ToString();

            }

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

                if (entityType == typeof(NgisSubscriber))
                {
                    return "Subscribers";
                }

                if (entityType == typeof(Datasets_NgisSubscriber))
                {
                    return "Datasets_Subscribers";
                }

                throw new Exception($"Not supported entity type {entityType}");
            };


            //CreateDatabaseIfNotExists();

            Datasets = ReadAll<Dataset>("Datasets");

            StoredChangelogs = ReadAll<StoredChangelog>("StoredChangelogs");
            Services = ReadAll<Service>("Services");
            ServerConfigs = ReadAll<ServerConfig>("ServerConfigs");
            Subscribers = ReadAll<NgisSubscriber>("Subscribers");
            Datasets_Subscribers = ReadAll<Datasets_NgisSubscriber>("Datasets_Subscribers");

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

        public virtual void SaveChanges()
        {
            foreach (var dataset in Datasets)
            {
                if (DatasetExists(dataset.DatasetId)) UpdateDataset(dataset);
                else InsertDataset(dataset);
            }
        }

        public static void SaveDataset(Dataset dataset)
        {
            if (DatasetExists(dataset.DatasetId)) UpdateDataset(dataset);
            else InsertDataset(dataset);
        }

        public void AddObject(Dataset ds)
        {
            Datasets.Add(ds);
        }

        //
        //StoredChangelogs part
        //
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

    /// <summary>
    /// ServerConfigsEntities
    /// </summary>
    public class ServerConfigsEntities : geosyncEntities
    {
        public static void UpdateServerConfig(ServerConfig serverConfig)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(serverConfig);
        }


        //public void AddObject(ServerConfig serverConfig)
        //{
        //    ServerConfigs.Add(serverConfig);
        //}
        //public static bool ServerConfigExists(int id)
        //{
        //    using (IDbConnection db = new SQLiteConnection(ConnectionString))
        //        return db.Query<int>($"SELECT 1 FROM ServerConfigs WHERE ID = {id}").ToList().FirstOrDefault() == 1;
        //}
    }

    /// <summary>
    /// ServicesEntities
    /// </summary>
    public class ServicesEntities : geosyncEntities
    {
        public static void UpdateServices(Service service)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(service);
        }

    }

    /// <summary>
    /// StoredChangelogsEntities
    /// </summary>
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

        public override void SaveChanges()
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


  

    /// <summary>
    /// Subscribers part
    /// </summary>
    public class Subscribers : geosyncEntities
    {
        public static void DeleteSubscriber(NgisSubscriber subscriber)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(subscriber);
        }

        public void DeleteObject(NgisSubscriber subscriber)
        {
            var deletedSubscriber = Subscribers.FirstOrDefault(d => d.id == subscriber.id);

            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedSubscriber);

            Subscribers.Remove(deletedSubscriber);
        }

        public static bool SubscriberExists(int id)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM Subscribers WHERE id = {id}").ToList().FirstOrDefault() == 1;
        }

        private static void InsertSubscriber(NgisSubscriber subscriber)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Insert(subscriber);
        }

        public static void UpdateSubscriber(NgisSubscriber subscriber)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(subscriber);
        }

        public static void SaveSubscriber(NgisSubscriber subscriber)
        {
            if (SubscriberExists(subscriber.id)) UpdateSubscriber(subscriber);
            else InsertSubscriber(subscriber);
        }

        public override void SaveChanges()
        {
            foreach (var subscriber in Subscribers)
            {
                if (SubscriberExists(subscriber.id)) UpdateSubscriber(subscriber);
                else InsertSubscriber(subscriber);
            }
        }
    }

    /// <summary>
    /// DatasetSubscribers
    /// </summary>
    public class DatasetSubscribers : geosyncEntities
    {
        public static void DeleteSubscriber(Datasets_NgisSubscriber subscriber)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(subscriber);
        }
        public void DeleteObject(Datasets_NgisSubscriber subscriber)
        {
            var deletedSubscriber = Datasets_Subscribers.FirstOrDefault(d => d.id == subscriber.id);

            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedSubscriber);

            Datasets_Subscribers.Remove(deletedSubscriber);
        }

        public static bool SubscriberExists(int id)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM Datasets_Subscribers WHERE id = {id}").ToList().FirstOrDefault() == 1;
        }

        private static void InsertSubscriber(Datasets_NgisSubscriber datasetSubscriber)
        {
         
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Insert(datasetSubscriber);
            
            //using (IDbConnection db = new SQLiteConnection(ConnectionString))
            //{
            //    var entities = new geosyncEntities();

            //    var subscriber = entities.Subscribers.Where(d => d.id == datasetSubscriber.subscriberid);
            //    datasetSubscriber.subscriberid = subscriber.FirstOrDefault().id;
            //    datasetSubscriber.dataset = entities.Datasets.FirstOrDefault(s => s.DatasetId == datasetSubscriber.datasetid);
            //    datasetSubscriber.subscriber = entities.Subscribers.FirstOrDefault(s => s.id == datasetSubscriber.subscriberid);
            //    db.Insert(datasetSubscriber); // TODO: error here

            //    //d.subscriber = entities.Subscribers.FirstOrDefault(s => s.id == d.subscriberid);
            //    //d.dataset = entities.Datasets.FirstOrDefault(s => s.DatasetId == d.datasetid);
            //}
                
        }

        public static void UpdateSubscriber(Datasets_NgisSubscriber subscriber)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(subscriber);
        }

        public static void SaveSubscriber(Datasets_NgisSubscriber subscriber)
        {
            if (SubscriberExists(subscriber.id)) UpdateSubscriber(subscriber);
            else InsertSubscriber(subscriber);
        }

        public override void SaveChanges()
        {
            foreach (var subscriber in Datasets_Subscribers)
            {
                if (SubscriberExists(subscriber.id)) UpdateSubscriber(subscriber);
                else InsertSubscriber(subscriber);
            }
        }

    }
}

