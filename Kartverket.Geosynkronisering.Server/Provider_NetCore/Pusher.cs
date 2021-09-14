using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChangelogManager;
using Kartverket.Geosynkronisering;

namespace Provider_NetCore
{
    internal class Pusher
    {
        static readonly Kartverket.Geosynkronisering.ChangelogManager changelogManager = GetChangelogManager();

        internal static void Synchronize(List<Dataset> datasets)
        {
            datasets.ForEach(async dataset =>
            {
                try
                {
                    var finalStatus = await Push(dataset);

                    ReportStatus(finalStatus);

                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);

                    ReportStatus(Status.UNKNOWN_ERROR);
                }
            });
        }

        private static async Task<Status> Push(Dataset dataset)
        {
            ReportStatus(Status.GET_LAST_TRANSNR);

            var provider = Utils.GetChangelogProvider(dataset);

            var lastIndex = GetLastIndex(dataset, provider);

            var status = GetDatasetStatusFor(dataset);

            if (status.copy_transaction_token == lastIndex) return Status.NO_CHANGES;

            if (status.copy_transaction_token > lastIndex) throw new Exception("Subscriber reports higher index than Provider");

            ReportStatus(Status.HAS_CHANGES);

            ReportStatus(Status.GENERATE_CHANGES);

            var changelogOrder = OrderChangelog(dataset, provider, lastIndex);

            var changelogStatus = await WaitForChangelog(changelogOrder);

            if (changelogStatus != Kartverket.GeosyncWCF.ChangelogStatusType.finished) return Status.GENERATE_CHANGES_FAILED;

            var changelog = changelogManager.GetChangelog(changelogOrder.changelogId);

            ReportStatus(Status.WRITE_CHANGES);

            return WriteChanges(changelog);
        }

        private static OrderChangelog OrderChangelog(Dataset dataset, IChangelogProvider provider, int lastIndex)
        {
            return provider.OrderChangelog(lastIndex + 1, dataset.ServerMaxCount ?? 10000, "", dataset.DatasetId);
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

        private static int GetLastIndex(Dataset dataset, IChangelogProvider provider)
        {
            return int.Parse(provider.GetLastIndex(dataset.DatasetId));
        }

        private static Status WriteChanges(Kartverket.GeosyncWCF.ChangelogType changelogType)
        {
            throw new NotImplementedException();

            var changelogPath = GetChangelogPath(changelogType);

            return Status.WRITE_CHANGES_OK;
        }

        private static object GetChangelogPath(Kartverket.GeosyncWCF.ChangelogType changelogType)
        {
            throw new NotImplementedException();
        }

        private static void ReportStatus(Status status)
        {
            throw new NotImplementedException();
        }

        internal static DatasetStatus GetDatasetStatusFor(Dataset dataset)
        {
            throw new NotImplementedException();
        }
    }
}