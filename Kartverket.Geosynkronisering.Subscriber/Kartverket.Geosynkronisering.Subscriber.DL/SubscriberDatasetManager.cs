using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public class SubscriberDatasetManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
     
        public SubscriberDataset GetDataset(string datasetName)
        {
            using (var localDb = new  geosyncDBEntities())
            {
                Dataset dataset = (from d in localDb.Datasets where d.Name == datasetName select d).FirstOrDefault();
                if (dataset == null)
                    return null;

                var geoClientDataset = MapToGeoClientDataset(dataset);

                return geoClientDataset;
            }
        }

        public SubscriberDataset GetDataset(int datasetId)
        {
            using (var localDb = new geosyncDBEntities())
            {
                Dataset dataset = (from d in localDb.Datasets where d.DatasetId == datasetId select d).FirstOrDefault();
                if (dataset == null)
                    return null;

                var geoClientDataset = MapToGeoClientDataset(dataset);
                return geoClientDataset;
            }
        }

        public string GetDatasource()
        {
            using (var localDb = new geosyncDBEntities())
            {
                //return localDb.Connection.DataSource;
                return "";
            }
        }

        public bool UpdateDataset(SubscriberDataset geoClientDataset)
        {
            using (var localDb = new geosyncDBEntities())
            {
                Dataset dataset =
                    (from d in localDb.Datasets where d.DatasetId == geoClientDataset.DatasetId select d)
                        .FirstOrDefault();
                if (dataset == null)
                    return false;

                dataset.MaxCount = geoClientDataset.MaxCount;
                dataset.LastIndex = geoClientDataset.LastIndex;
                dataset.Name = geoClientDataset.Name;
                dataset.ProviderDatasetId = geoClientDataset.ProviderDatasetId;
                dataset.SyncronizationUrl = geoClientDataset.SynchronizationUrl;
                dataset.ClientWfsUrl = geoClientDataset.ClientWfsUrl;

                localDb.SaveChanges();
                return true;
            }
        }

        private static SubscriberDataset MapToGeoClientDataset(Dataset dataset)
        {
            var geoClientDataset = new SubscriberDataset
                                   {
                                       DatasetId = dataset.DatasetId,
                                       Name = dataset.Name,
                                       LastIndex = dataset.LastIndex.HasValue ? dataset.LastIndex.Value : -1,
                                       SynchronizationUrl = dataset.SyncronizationUrl,
                                       ClientWfsUrl = dataset.ClientWfsUrl,
                                       MaxCount = dataset.MaxCount.HasValue ? dataset.MaxCount.Value : -1,
                                       ProviderDatasetId =
                                           dataset.ProviderDatasetId.HasValue ? dataset.ProviderDatasetId.Value : -1
                                   };
            return geoClientDataset;
        }

        public static int GetNextDatasetID()
        {
            int max = 0;
            using (var localDb = new  geosyncDBEntities())
            {
                var res = from d in localDb.Datasets select d.DatasetId;
                foreach (Int32 id in res)
                {
                    if (max < id) max = id;
                }
            }
            return max + 1;
        }

        public static IList<Int32> GetListOfDatasetIDs()
        {
            using (var localDb = new geosyncDBEntities())
            {
                IList<Int32> idList = new List<Int32>();
                var res = from d in localDb.Datasets select d.DatasetId;
                foreach (Int32 id in res)
                {
                    idList.Add(id);
                }
                return idList;
            }
        }

        public static string Name(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.Name;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string SyncronizationUrl(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.SyncronizationUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ProviderDatasetId(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.ProviderDatasetId;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string MappingFile(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.MappingFile;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string MaxCount(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.MaxCount;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string LastIndex(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.LastIndex;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ClientWfsUrl(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.ClientWfsUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string TargetNamespace(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Datasets where d.DatasetId == DatasetID select d.TargetNamespace;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static bool AddDatasets(geosyncDBEntities db, IBindingList datasetBindingList, IList<int> selectedDatasets)
        {

            Dataset ds = null;
            foreach (int selected in selectedDatasets)
            {
                ds = (Dataset)datasetBindingList[selected];
                try
                {
                    ds.DatasetId = GetNextDatasetID();
                    ds.LastIndex = 0;
                    ds.ClientWfsUrl = "http://localhost:8081/geoserver/wfs?"; //TODO: Flytt til config
                    db.AddObject(ds.EntityKey.EntitySetName, ds);
                    db.SaveChanges();
                    db.AcceptAllChanges();
                }
                catch (Exception ex)
                {
                    logger.LogException(LogLevel.Error, "Error saving selected datasets!", ex);
                    return false;
                }
            }

            return true;
        }
    }
}
