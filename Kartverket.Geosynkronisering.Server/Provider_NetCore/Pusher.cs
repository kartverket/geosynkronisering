using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChangelogManager;
using Kartverket.Geosynkronisering;

namespace Provider_NetCore
{
    internal class Pusher
    {
        static readonly Kartverket.Geosynkronisering.ChangelogManager changelogManager = GetChangelogManager();

        public static Dataset _CurrentDataset { get; private set; }
        public static Datasets_NgisSubscriber _currentSubscriber { get; private set; }

        internal static void Synchronize(List<Dataset> datasets)
        {
            datasets.ForEach(dataset =>
            {
                _CurrentDataset = dataset;

                var subscribers = SqlHelper.GetSubscribers(_CurrentDataset.DatasetId);

                subscribers.ForEach(async s =>
                {
                    try
                    {
                        _currentSubscriber = s;

                        var finalStatus = await Push();

                        ReportStatus(finalStatus);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);

                        ReportStatus(Status.UNKNOWN_ERROR);
                    }
                });
            });
        }

        private static async Task<Status> Push()
        {
            ReportStatus(Status.GET_LAST_TRANSNR);

            var provider = Utils.GetChangelogProvider(_CurrentDataset);

            var lastIndex = GetLastIndex(provider);

            var status = GetDatasetStatus();

            if (status.copy_transaction_token == lastIndex) return Status.NO_CHANGES;

            if (status.copy_transaction_token > lastIndex) throw new Exception("Subscriber reports higher index than Provider");

            ReportStatus(Status.HAS_CHANGES);

            ReportStatus(Status.GENERATE_CHANGES);

            var changelogOrder = OrderChangelog(provider, lastIndex);

            var changelogStatus = await WaitForChangelog(changelogOrder);

            if (changelogStatus != Kartverket.GeosyncWCF.ChangelogStatusType.finished) return Status.GENERATE_CHANGES_FAILED;

            var changelog = changelogManager.GetChangelog(changelogOrder.changelogId);

            ReportStatus(Status.WRITE_CHANGES);

            return WriteChanges(changelog);
        }

        private static OrderChangelog OrderChangelog(IChangelogProvider provider, int lastIndex)
        {
            return provider.OrderChangelog(lastIndex + 1, _CurrentDataset.ServerMaxCount ?? 10000, "", _CurrentDataset.DatasetId);
        }

        private static async Task<Kartverket.GeosyncWCF.ChangelogStatusType> WaitForChangelog(OrderChangelog changelogOrder)
        {
            var changelogStatus = changelogManager.GetChangelogStatus(changelogOrder.changelogId);

            while (changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.working
                || changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.queued)
            {
                await Task.Delay(2000);

                changelogStatus = changelogManager.GetChangelogStatus(changelogOrder.changelogId);
            }

            return changelogStatus;
        }

        private static Kartverket.Geosynkronisering.ChangelogManager GetChangelogManager()
        {
            using StoredChangelogsEntities db = new();

            return new Kartverket.Geosynkronisering.ChangelogManager(db);
        }

        private static int GetLastIndex(IChangelogProvider provider)
        {
            return int.Parse(provider.GetLastIndex(_CurrentDataset.DatasetId));
        }

        private static Status WriteChanges(Kartverket.GeosyncWCF.ChangelogType changelogType)
        {
            throw new NotImplementedException();

            var changelogPath = GetChangelogPath(changelogType.downloadUri);

            return Status.WRITE_CHANGES_OK;
        }

        private static object GetChangelogPath(string downloadUri)
        {
            var zipFileName = downloadUri.Split('/').Last();

            var filLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Changelogfiles\\" + zipFileName;

            return filLocation;
        }

        private static void ReportStatus(Status status)
        {
            var statusUrl = _currentSubscriber.subscriber.url.TrimEnd('/') + $"/datasets/{_CurrentDataset.DatasetId}/job-status";

            var json = JsonSerializer.Serialize(status); 

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = new HttpClient();

            var response = client.PostAsync(statusUrl, stringContent).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.OK
                || response.StatusCode != System.Net.HttpStatusCode.Accepted
                ) throw new Exception(response.ReasonPhrase);

        }

        internal static DatasetStatus GetDatasetStatus()
        {
            throw new NotImplementedException();
        }
    }
}