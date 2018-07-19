using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    internal class GeosyncDbEntities : IDisposable
    {
        public GeosyncDbEntities()
        {
            SqlMapperExtensions.TableNameMapper = type => type.Name;

            // Populate Dataset-List from database
            Connection = new SqlCeConnection
            {
                ConnectionString = System.Configuration.ConfigurationManager.
                    ConnectionStrings["geosyncDBEntities"].ConnectionString
            };

            Dataset = ReadAll<Dataset>("Dataset");
        }

        public void Dispose()
        {
            Connection.Close();
            Dataset = null;
        }

        public List<Dataset> Dataset { get; set; }
        public static SqlCeConnection Connection { get; set; }

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
            using (IDbConnection db = new SqlCeConnection(Connection.ConnectionString))
            {
                return db.Query<T>("SELECT * FROM " + tableName).ToList();
            }
        }

        public static bool DatasetExists(int datasetId)
        {
            using (IDbConnection db = new SqlCeConnection(Connection.ConnectionString))
            {
                return db.Query<int>($"SELECT 1 FROM Dataset WHERE DatasetId = {datasetId}").ToList().FirstOrDefault() == 1;
            }
        }

        private static void InsertDataset(Dataset dataset)
        {
            Connection.Insert(dataset);
        }

        private static void UpdateDataset(Dataset dataset)
        {
            Connection.Update(dataset);
        }

        public void DeleteObject(Dataset dataset)
        {
            Connection.Delete(dataset);
            Dataset.Remove(dataset);
        }
    }
}