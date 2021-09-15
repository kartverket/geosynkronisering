using System;
using System.Collections.Generic;
using System.IO;
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
        static readonly HttpClient _client = new();

        static readonly Kartverket.Geosynkronisering.ChangelogManager _changelogManager = GetChangelogManager();

        public static Datasets_NgisSubscriber _currentSubscriber { get; private set; }
        public static List<ActiveChangelog> _activeChangelogs { get; private set; }

        internal static void Synchronize(List<Dataset> datasets)
        {
            datasets.ForEach(dataset =>
            {
                var subscribers = SqlHelper.GetSubscribers(dataset.DatasetId);

                _activeChangelogs = new List<ActiveChangelog>();

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
            var provider = Utils.GetChangelogProvider(_currentSubscriber.dataset);

            var lastIndex = GetLastIndex(provider);

            ReportStatus(Status.GET_LAST_TRANSNR);

            var status = GetDatasetStatus();

            if (status.copy_transaction_token == lastIndex) return Status.NO_CHANGES;

            if (status.copy_transaction_token > lastIndex) throw new Exception("Subscriber reports higher index than Provider");

            ReportStatus(Status.HAS_CHANGES);

            ReportStatus(Status.GENERATE_CHANGES);

            try
            {
                await GetNewChangelogAsync(status, provider);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return Status.GENERATE_CHANGES_FAILED;
            }

            ReportStatus(Status.WRITE_CHANGES);

            return WriteChanges(GetActiveChangelog(status), lastIndex);
        }

        private static async Task GetNewChangelogAsync(DatasetStatus status, IChangelogProvider provider)
        {
            if (_activeChangelogs.Count > 0 && _activeChangelogs.Any(c => c.copy_transaction_token == status.copy_transaction_token)) return;

            var changelogOrder = OrderChangelog(provider, status.copy_transaction_token);

            var changelogStatus = await WaitForChangelog(changelogOrder);

            if (changelogStatus != Kartverket.GeosyncWCF.ChangelogStatusType.finished) throw new Exception("Unable to generate changelog");

            var changelog = _changelogManager.GetChangelog(changelogOrder.changelogId);

            _activeChangelogs.Add(new ActiveChangelog()
            {
                changelog = changelog,
                copy_transaction_token = status.copy_transaction_token
            });
        }

        private static Kartverket.GeosyncWCF.ChangelogType GetActiveChangelog(DatasetStatus status)
        {
            return _activeChangelogs.FirstOrDefault(c => c.copy_transaction_token == status.copy_transaction_token).changelog;
        }

        private static OrderChangelog OrderChangelog(IChangelogProvider provider, int copy_transaction_token)
        {
            return provider.OrderChangelog(copy_transaction_token + 1, _currentSubscriber.dataset.ServerMaxCount ?? 10000, "", _currentSubscriber.datasetid);
        }

        private static async Task<Kartverket.GeosyncWCF.ChangelogStatusType> WaitForChangelog(OrderChangelog changelogOrder)
        {
            var changelogStatus = _changelogManager.GetChangelogStatus(changelogOrder.changelogId);

            while (changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.working
                || changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.queued)
            {
                await Task.Delay(2000);

                changelogStatus = _changelogManager.GetChangelogStatus(changelogOrder.changelogId);
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
            return int.Parse(provider.GetLastIndex(_currentSubscriber.datasetid));
        }

        private static Status WriteChanges(Kartverket.GeosyncWCF.ChangelogType changelogType, int lastIndex)
        {
            var changelogPath = GetChangelogPath(changelogType.downloadUri);

            var url = GetDatasetUrl("features") + $"?copy_transaction_number={lastIndex}&dataset_version={_currentSubscriber.dataset.Version}&async=true";

            var stream = File.OpenRead(changelogPath);

            var streamContent = new StreamContent(stream);

            var defaultHeaders = _client.DefaultRequestHeaders;

            _client.DefaultRequestHeaders.Add("Content-Type", "application/vnd.kartverket.geosynkronisering+zip");

            var result = _client.PostAsync(url, streamContent).Result;

            _client.DefaultRequestHeaders.Clear();

            foreach (var header in defaultHeaders) _client.DefaultRequestHeaders.Add(header.Key, header.Value);

            TestForSuccess(result);

            return Status.WRITE_CHANGES_OK;
        }

        private static string GetChangelogPath(string downloadUri)
        {
            var zipFileName = downloadUri.Split('/').Last();

            var filLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Changelogfiles\\" + zipFileName;

            return filLocation;
        }

        private static void ReportStatus(Status status)
        {
            var response = _client.PostAsync(GetDatasetUrl($"job-status"), GetJsonStringContent(status)).Result;

            TestForSuccess(response);
        }

        private static StringContent GetJsonStringContent(Status status)
        {
            return new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
        }

        private static string GetDatasetUrl(string postFix = null)
        {
            var url = _currentSubscriber.subscriber.url.TrimEnd('/') + $"/datasets/{_currentSubscriber.subscriberdatasetid}";

            if (string.IsNullOrEmpty(postFix)) return url;

            return url + "/" + postFix;
        }

        internal static DatasetStatus GetDatasetStatus()
        {
            var response = _client.GetAsync(GetDatasetUrl()).Result;

            TestForSuccess(response);

            return JsonSerializer.Deserialize<DatasetStatus>(response.Content.ReadAsStringAsync().Result);
        }

        private static void TestForSuccess(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK
                            || response.StatusCode != System.Net.HttpStatusCode.Accepted
                            ) throw new Exception(response.ReasonPhrase);
        }
    }
}