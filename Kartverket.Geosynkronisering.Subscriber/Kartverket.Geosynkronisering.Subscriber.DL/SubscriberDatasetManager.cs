using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public static class SubscriberDatasetManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public static SubscriberDataset GetDataset(string datasetName)
        {
            using (var localDb = new geosyncDBEntities())
            {
                Dataset dataset = (from d in localDb.Dataset where d.Name == datasetName select d).FirstOrDefault();
                if (dataset == null)
                    return null;

                var geoClientDataset = MapToGeoClientDataset(dataset);

                return geoClientDataset;
            }
        }

        public static SubscriberDataset GetDataset(int datasetId)
        {
            using (var localDb = new geosyncDBEntities())
            {
                Dataset dataset = (from d in localDb.Dataset where d.DatasetId == datasetId select d).FirstOrDefault();
                if (dataset == null)
                    return null;

                var geoClientDataset = MapToGeoClientDataset(dataset);
                return geoClientDataset;
            }
        }

        public static List<SubscriberDataset> GetAllDataset()
        {
            using (var localDb = new geosyncDBEntities())
            {
                List<Dataset> datasets = (from d in localDb.Dataset select d).ToList();

                var geoClientDatasets = new List<SubscriberDataset>();

                foreach (var dataset in datasets)
                {
                    geoClientDatasets.Add(MapToGeoClientDataset(dataset));
                }
                return geoClientDatasets;
            }
        }

        public static string GetDatasource()
        {
            using (var localDb = new geosyncDBEntities())
            {
                return localDb.Connection.DataSource;
            }
        }



        public static bool UpdateDataset(SubscriberDataset geoClientDataset)
        {
            using (var localDb = new geosyncDBEntities())
            {
                Dataset dataset =
                    (from d in localDb.Dataset where d.DatasetId == geoClientDataset.DatasetId select d)
                        .FirstOrDefault();
                if (dataset == null)
                    return false;

                dataset.MaxCount = geoClientDataset.MaxCount;
                dataset.LastIndex = geoClientDataset.LastIndex;
                dataset.Name = geoClientDataset.Name;
                dataset.ProviderDatasetId = geoClientDataset.ProviderDatasetId;
                dataset.SyncronizationUrl = geoClientDataset.SynchronizationUrl;
                dataset.ClientWfsUrl = geoClientDataset.ClientWfsUrl;
                dataset.TargetNamespace = geoClientDataset.TargetNamespace;
                dataset.MappingFile = geoClientDataset.MappingFile;

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
                                       ProviderDatasetId = dataset.ProviderDatasetId,
                                       TargetNamespace = dataset.TargetNamespace,
                                       MappingFile = dataset.MappingFile
                                   };
            return geoClientDataset;
        }

        public static int GetNextDatasetID()
        {
            int max = 0;
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset select d.DatasetId;
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
                var res = from d in localDb.Dataset select d.DatasetId;
                foreach (Int32 id in res)
                {
                    idList.Add(id);
                }
                return idList;
            }
        }

        public static string SyncronizationUrl(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == DatasetID select d.SyncronizationUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ProviderDatasetId(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == DatasetID select d.ProviderDatasetId;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string MappingFile(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == DatasetID select d.MappingFile;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static int GetMaxCount(int datasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = (from d in localDb.Dataset where d.DatasetId == datasetID select d.MaxCount).FirstOrDefault();
                return res.GetValueOrDefault();
            }
        }
        public static string GetLastIndex(int datasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == datasetID select d.LastIndex;
                if (res.FirstOrDefault() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ClientWfsUrl(int datasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == datasetID select d.ClientWfsUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static List<string> GetDatasetNames()
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = (from d in localDb.Dataset select d.Name).ToList();
                return res;
            }
        }

        public static string TargetNamespace(Int32 DatasetID)
        {
            using (var localDb = new geosyncDBEntities())
            {
                var res = from d in localDb.Dataset where d.DatasetId == DatasetID select d.TargetNamespace;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static bool AddDatasets(IBindingList datasetBindingList, IList<int> selectedDatasets)
        {
            using (var localDb = new geosyncDBEntities())
            {
                foreach (int selected in selectedDatasets)
                {
                    var ds = (Dataset)datasetBindingList[selected];
                    try
                    {
                        ds.DatasetId = GetNextDatasetID();
                        ds.LastIndex = 0;
                        ds.ClientWfsUrl = "http://localhost:8081/geoserver/wfs?"; //TODO: Flytt til config
                        localDb.AddObject(ds.EntityKey.EntitySetName, ds);
                        localDb.SaveChanges();
                        localDb.AcceptAllChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogException(LogLevel.Error, "Error saving selected datasets!", ex);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Removes the selected datasets from database.
        /// </summary>
        /// <param name="datasetBindingList">The dataset binding list.</param>
        /// <param name="selectedDatasets">The selected datasets.</param>
        /// <returns>True if OK, else False</returns>
        public static bool RemoveDatasets(List<SubscriberDataset> datasetBindingList, IList<int> selectedDatasets)
        {
            using (var localDb = new geosyncDBEntities())
            {
                foreach (int selected in selectedDatasets)
                {

                    var geoClientDataset = (SubscriberDataset)datasetBindingList[selected];

                    Dataset dataset =
                         (from d in localDb.Dataset where d.DatasetId == geoClientDataset.DatasetId select d)
                             .FirstOrDefault();
                    if (dataset == null)
                        return false;
                    try
                    {
                        localDb.DeleteObject(dataset);
                        localDb.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogException(LogLevel.Error, "Error removing selected datasets!", ex);
                        return false;
                    }
        
                }
            }

            return true;
        }

   
    }
}
