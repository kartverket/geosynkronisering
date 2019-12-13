using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public static class SubscriberDatasetManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public static Dataset GetDataset(int datasetId)
        {
            using (var localDb = new GeosyncDbEntities())
            {
                return (from d in localDb.Dataset where d.DatasetId == datasetId select d).FirstOrDefault();
            }
        }

        public static List<Dataset> GetAllDataset()
        {
            using (var localDb = new GeosyncDbEntities())
            {
                var datasets = (from d in localDb.Dataset select d).ToList();

                var geoClientDatasets = new List<Dataset>();

                foreach (var dataset in datasets)
                {
                    geoClientDatasets.Add(dataset);
                }

                return geoClientDatasets;
            }
        }

        public static string GetDatasource()
        {
            return GeosyncDbEntities.ConnectionString;
        }


        public static bool UpdateDataset(Dataset geoClientDataset)
        {
            using (var localDb = new GeosyncDbEntities())
            {
                var dataset =
                    (from d in localDb.Dataset where d.DatasetId == geoClientDataset.DatasetId select d)
                    .FirstOrDefault();
                if (dataset == null)
                    return false;

                dataset.MaxCount = geoClientDataset.MaxCount;
                dataset.LastIndex = geoClientDataset.LastIndex;
                dataset.Name = geoClientDataset.Name;
                dataset.ProviderDatasetId = geoClientDataset.ProviderDatasetId;
                dataset.SyncronizationUrl = geoClientDataset.SyncronizationUrl;
                dataset.ClientWfsUrl = geoClientDataset.ClientWfsUrl;
                dataset.TargetNamespace = geoClientDataset.TargetNamespace;
                dataset.MappingFile = geoClientDataset.MappingFile;
                dataset.AbortedEndIndex = geoClientDataset.AbortedEndIndex;
                dataset.AbortedTransaction = geoClientDataset.AbortedTransaction;
                dataset.AbortedChangelogPath = geoClientDataset.AbortedChangelogPath;
                dataset.ChangelogDirectory = geoClientDataset.ChangelogDirectory;
                dataset.AbortedChangelogId = geoClientDataset.AbortedChangelogId;
                dataset.UserName = geoClientDataset.UserName;
                if (geoClientDataset.Password != "******")
                    dataset.Password = geoClientDataset.Password;
                dataset.Version = geoClientDataset.Version;

                localDb.SaveChanges();
                return true;
            }
        }

        public static IList<int> GetListOfDatasetIDs()
        {
            using (var localDb = new GeosyncDbEntities())
            {
                var res = from d in localDb.Dataset select d.DatasetId;

                return res.ToList();
            }
        }

        public static string GetLastIndex(int datasetId)
        {
            using (var localDb = new GeosyncDbEntities())
            {
                if (localDb.Dataset == null) return null;

                var res = (from d in localDb.Dataset where d.DatasetId == datasetId select d.LastIndex).ToList();

                return res.FirstOrDefault() > 0 ? res.First().ToString() : "";
            }
        }

        public static IDictionary<int, string> GetDatasetNamesAsDictionary()
        {
            using (var localDb = new GeosyncDbEntities())
            {
                var dict = localDb.Dataset.Select(t => new {t.DatasetId, t.Name})
                    .ToDictionary(t => t.DatasetId, t => t.Name);
                return dict;
            }
        }

        public static bool AddDatasets(IBindingList datasetBindingList, IList<int> selectedDatasets, string providerUrl,
            string userName, string password)
        {
            using (var localDb = new GeosyncDbEntities())
            {
                foreach (var selected in selectedDatasets)
                {
                    var ds = (Dataset) datasetBindingList[selected];
                    try
                    {
                        ds.LastIndex = 0;
                        ds.ClientWfsUrl = "";
                        ds.UserName = userName;
                        ds.Password = password;
                        ds.SyncronizationUrl = providerUrl;
                        localDb.AddObject(ds);
                        localDb.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error saving selected datasets!");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool AddEmptyDataset()
        {
            using (var localDb = new GeosyncDbEntities())
            {
                var ds = new Dataset();
                try
                {
                    ds.LastIndex = 0;
                    ds.ClientWfsUrl = "";
                    localDb.AddObject(ds);
                    localDb.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error saving selected datasets!");
                    return false;
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
        public static bool RemoveDatasets(List<Dataset> datasetBindingList, IList<int> selectedDatasets)
        {
            using (var localDb = new GeosyncDbEntities())
            {
                foreach (var selected in selectedDatasets)
                {
                    var geoClientDataset = datasetBindingList[selected];

                    var dataset =
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
                        Logger.Error(ex, "Error removing selected datasets!");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}