using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    internal class GeosyncDbEntities : IDisposable
    {
        public List<Dataset> Dataset { get; set; }

        public static string ConnectionString;

        public GeosyncDbEntities()
        {
            SqlMapperExtensions.TableNameMapper = type => type.Name;

            ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["geosyncDBEntities"].ConnectionString;

            CreateDatabaseIfNotExists();

            Dataset = ReadAll<Dataset>("Dataset");
        }

        private static void CreateDatabaseIfNotExists()
        {
            var filePath = Directory.GetCurrentDirectory() + "\\" + ConnectionString.Split(';')[0].Split('=')[1];

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

        private static void InsertDataset(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Insert(dataset);
        }

        private static void UpdateDataset(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Update(dataset);
        }

        public void DeleteObject(Dataset dataset)
        {
            using (IDbConnection db = new SQLiteConnection(ConnectionString)) db.Delete(dataset);

            Dataset.Remove(dataset);
        }
    }
}