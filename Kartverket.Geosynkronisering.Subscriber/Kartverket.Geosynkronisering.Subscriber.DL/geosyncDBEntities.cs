using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    internal class GeosyncDbEntities : IDisposable
    {
        public List<Dataset> Dataset { get; set; }

        public static string ConnectionString;

        public GeosyncDbEntities()
        {

            SqlMapperExtensions.TableNameMapper = type => type.Name;

            ConnectionString = new SqliteConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings["geosyncDBEntities"].ConnectionString).ToString();

            CreateDatabaseIfNotExists();

            Dataset = ReadAll<Dataset>("Dataset");
        }

        private static void CreateDatabaseIfNotExists()
        {
            using (var Connection = new SqliteConnection(ConnectionString))
                if (!File.Exists(Connection.DataSource))
                {
                    Connection.Open();

                    using (var cmd = new SqliteCommand(File.ReadAllText("databaseSchema.sqlce"), Connection)) cmd.ExecuteNonQuery();
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

            using (IDbConnection db = new SqliteConnection(ConnectionString))
                return db.Query<T>("SELECT * FROM " + tableName).ToList();

        }

        public static bool DatasetExists(int datasetId)
        {
            using (IDbConnection db = new SqliteConnection(ConnectionString))
                return db.Query<int>($"SELECT 1 FROM Dataset WHERE DatasetId = {datasetId}").ToList().FirstOrDefault() == 1;
        }

        private static void InsertDataset(Dataset dataset)
        {
            using (IDbConnection db = new SqliteConnection(ConnectionString)) db.Insert(dataset);
        }

        private static void UpdateDataset(Dataset dataset)
        {
            using (IDbConnection db = new SqliteConnection(ConnectionString)) db.Update(dataset);
        }

        public void DeleteObject(Dataset dataset)
        {
            using (IDbConnection db = new SqliteConnection(ConnectionString)) db.Delete(dataset);

            Dataset.Remove(dataset);
        }
    }
}