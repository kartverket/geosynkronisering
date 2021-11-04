using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChangelogManager;
using Kartverket.Geosynkronisering;

namespace Provider_NetCore
{
    internal class Pusher
    {
        public static Datasets_NgisSubscriber _currentSubscriber { get; private set; }

        public static List<ActiveChangelog> _activeChangelogs { get; private set; }

        static readonly HttpClient _client = new();

        static HttpClient Client
        {
            get
            {
                SetCredentials();
                SetClientHeaders();
                return _client;
            }
        }

        private static void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Add("X-Client-Product-Version", "GeodataTest");
            _client.DefaultRequestHeaders.Add("accept", "*/*");

        }

        private static void SetCredentials()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(_currentSubscriber.subscriber.username + ":" + _currentSubscriber.subscriber.password)));
        }

        internal static void Synchronize(List<Dataset> datasets)
        {
            datasets.ForEach(dataset =>
            {
                var subscribers = SqlHelper.GetSubscribers(dataset.DatasetId);

                _activeChangelogs = new List<ActiveChangelog>();

                subscribers.ForEach(async subscriber =>
                {
                    try
                    {
                        _currentSubscriber = subscriber;

                        var finalStatus = await Push();

                        ReportStatus(finalStatus);
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine(exception.Message);

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

            if (status.last_copy_transaction_number == lastIndex) return Status.NO_CHANGES;

            if (status.last_copy_transaction_number > lastIndex) throw new Exception("Subscriber reports higher index than Provider");

            ReportStatus(Status.HAS_CHANGES);

            ReportStatus(Status.GENERATE_CHANGES);

            try
            {
                await GetNewChangelogAsync(status, provider);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return Status.GENERATE_CHANGES_FAILED;
            }

            ReportStatus(Status.WRITE_CHANGES);

            return WriteChanges(GetActiveChangelog(status.last_copy_transaction_number), lastIndex);
        }

        private static async Task GetNewChangelogAsync(DatasetStatus status, IChangelogProvider provider)
        {
            if (_activeChangelogs.Count > 0 && _activeChangelogs.Any(c => c.copy_transaction_number == status.last_copy_transaction_number)) return;

            var changelogOrder = OrderChangelog(provider, (int) status.last_copy_transaction_number);

            var changelogStatus = await WaitForChangelog(changelogOrder);

            if (changelogStatus != Kartverket.GeosyncWCF.ChangelogStatusType.finished) throw new Exception("Unable to generate changelog");

            var changelog = GetChangelogManager().GetChangelog(changelogOrder.changelogId);

            _activeChangelogs.Add(new ActiveChangelog()
            {
                changelog = changelog,
                copy_transaction_number = (int) status.last_copy_transaction_number,
                dataset = _currentSubscriber.dataset
            });
        }

        private static Kartverket.GeosyncWCF.ChangelogType GetActiveChangelog(int? last_copy_transaction_number)
        {
            return _activeChangelogs.FirstOrDefault(c => c.dataset.DatasetId == _currentSubscriber.dataset.DatasetId && c.copy_transaction_number == last_copy_transaction_number).changelog;
        }

        private static OrderChangelog OrderChangelog(IChangelogProvider provider, int copy_transaction_token)
        {
            provider.CreateChangelog(copy_transaction_token + 1, _currentSubscriber.dataset.ServerMaxCount ?? 10000, "", _currentSubscriber.datasetid);
            
            return provider.OrderChangelog(copy_transaction_token + 1, _currentSubscriber.dataset.ServerMaxCount ?? 10000, "", _currentSubscriber.datasetid);
        }

        private static async Task<Kartverket.GeosyncWCF.ChangelogStatusType> WaitForChangelog(OrderChangelog changelogOrder)
        {
            var changelogStatus = GetChangelogManager().GetChangelogStatus(changelogOrder.changelogId);

            while (changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.working
                || changelogStatus == Kartverket.GeosyncWCF.ChangelogStatusType.queued)
            {
                await Task.Delay(2000);

                changelogStatus = GetChangelogManager().GetChangelogStatus(changelogOrder.changelogId);
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

            var url = GetDatasetUrl("features") + $"?copy_transaction_number={lastIndex}&dataset_version={_currentSubscriber.dataset.Version}&async=true&locking_type=all_lock";

            var stream = File.OpenRead(changelogPath);

            var streamContent = new StreamContent(stream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.kartverket.geosynkronisering+zip");

            var result = Client.PostAsync(url, streamContent).Result;

            TestForSuccess(result);

            var status = result.Headers.GetValues("Location").FirstOrDefault();

            var statusResult = Client.GetAsync(status).Result;

            while (statusResult.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                statusResult = Client.GetAsync(status).Result;

                Task.Delay(2000).Wait();
            }

            Console.WriteLine("INFO: " + statusResult.Content.ReadAsStringAsync().Result);

            return TestForSuccess(statusResult) 
                ? Status.WRITE_CHANGES_OK
                : Status.UNKNOWN_ERROR;
        }

        private static string GetChangelogPath(string downloadUri)
        {
            var zipFileName = downloadUri.Split('/').Last();

            var filLocation = AppDomain.CurrentDomain.BaseDirectory + "Changelogfiles\\" + zipFileName;

            return filLocation;
        }

        private static void ReportStatus(Status status)
        {
            var response = Client.PostAsync(GetDatasetUrl($"job-status"), GetJsonStringContent(status)).Result;

            TestForSuccess(response);
        }

        private static StringContent GetJsonStringContent(Status status)
        {
            return new StringContent(JsonSerializer.Serialize(status, GetJsonSerializerOptions()), Encoding.UTF8, "application/json");
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                Converters = {
                    new JsonStringEnumConverter(),
                }
            };
        }

        private static string GetDatasetUrl(string postFix = null)
        {
            var url = _currentSubscriber.subscriber.url.TrimEnd('/') + $"/datasets/{_currentSubscriber.subscriberdatasetid}";

            if (string.IsNullOrEmpty(postFix)) return url;

            return url + "/" + postFix;
        }

        internal static DatasetStatus GetDatasetStatus()
        {
            var response = Client.GetAsync(GetDatasetUrl()).Result;

            TestForSuccess(response);

            var result = response.Content.ReadAsStringAsync().Result;

            var status = JsonSerializer.Deserialize<DatasetStatus>(result);

            if (status.last_copy_transaction_number == null) status.last_copy_transaction_number = -1;

            return status;
        }

        private static bool TestForSuccess(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 300) throw new Exception(response.ReasonPhrase);

            return true;
        }
    }
}