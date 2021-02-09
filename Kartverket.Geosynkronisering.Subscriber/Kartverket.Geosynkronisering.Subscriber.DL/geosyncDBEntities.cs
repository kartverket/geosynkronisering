using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public class GeosyncDbEntities : IDisposable
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public List<Dataset> Dataset { get; set; }

        public static string ConnectionString
        {
            get { return System.Configuration.ConfigurationManager.ConnectionStrings["geosyncDBEntities"].ConnectionString; }
        }

        public GeosyncDbEntities()
        {
            SqlMapperExtensions.TableNameMapper = type => type.Name;

            CreateDatabaseIfNotExists();

            Dataset = ReadAll<Dataset>("Dataset");
        }

        private static void CreateDatabaseIfNotExists()
        {
            // test
            var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Logger.Info("CreateDatabaseIfNotExists-assemblyPath:{0}", assemblyPath);
            
            // Get path to correct folder if run as a service:
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            Logger.Info("CreateDatabaseIfNotExists-BaseDirectory:{0}", basePath);

            var filePath = ConnectionString.Split(';')[0].Split('=')[1];
            Logger.Info("CreateDatabaseIfNotExists-sqlite db filepath:{0} ConnectionString:{1}", filePath, ConnectionString );

            using (var Connection = new SQLiteConnection(ConnectionString))
                if (!File.Exists(filePath))
                {
                    Connection.Open();

                    using (var cmd = new SQLiteCommand(Database.Schema, Connection)) cmd.ExecuteNonQuery();
                }
        }

        public void Dispose()
        {
            Dataset = null;
        }        

        public void AddObject(Dataset ds)
        {
            Dataset.Add(ds);
        }

        public void SaveChanges()
        {
            foreach (var dataset in Dataset)
            {
                if (DatasetExists(dataset.DatasetId)) UpdateDataset(dataset);
                else InsertDataset(dataset);
            }
        }

        public static List<T> ReadAll<T>(string tableName)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<T>("SELECT * FROM " + tableName).ToList();
        }

        public static bool DatasetExists(int datasetId)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM Dataset WHERE DatasetId = {datasetId}").ToList().FirstOrDefault() == 1;
        }

        public static void InsertDataset(Dataset dataset)
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
            var deletedDataset = Dataset.FirstOrDefault(d => d.DatasetId == dataset.DatasetId);

            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(deletedDataset);

            Dataset.Remove(deletedDataset);
        }
    }
}