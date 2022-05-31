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

        private const string DatasetHeader = "application/vnd.kartverket.ngis.dataset+json";
        private const string ChangelogHeader = "application/vnd.kartverket.geosynkronisering+zip";
        private const string JsonHeader = "application/json";
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

                        var finished = false;

                        var tries = 0;

                        var maxTries = 3;

                        while (!finished && tries < maxTries)
                        {
                            Console.WriteLine($"Starting synchronization of dataset {_currentSubscriber.dataset} ({tries +1}/{maxTries})");

                            var finalStatus = await Push();

                            ReportStatus(finalStatus);

                            switch (finalStatus.status)
                            {
                                case Status.GET_LAST_TRANSNR:                                
                                case Status.HAS_CHANGES:
                                case Status.GENERATE_CHANGES:
                                case Status.WRITE_CHANGES:
                                case Status.WRITE_CHANGES_OK:
                                    break;
                                case Status.GENERATE_CHANGES_FAILED:
                                case Status.UNKNOWN_ERROR:                                
                                default:
                                    {
                                        tries++;
                                        break;
                                    }

                                case Status.NO_CHANGES:
                                    {
                                        finished = true;
                                        break;
                                    }
                            }

                            if (tries == maxTries)
                            {
                                Console.WriteLine($"Unable to synchronize {_currentSubscriber.dataset.DatasetId}. Final status: {finalStatus.status}.");

                                //if(!string.IsNullOrEmpty(finalStatus.message)) Console.WriteLine($"Final message: {finalStatus.message}.");
                            }

                            
                        }

                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine(exception.Message);

                        ReportStatus(new ReportStatus{
                            status = Status.UNKNOWN_ERROR,
                            message = exception.Message
                        });
                    }
                });
            });
        }

        private static async Task<ReportStatus> Push()
        {
            var provider = Utils.GetChangelogProvider(_currentSubscriber.dataset);

            var lastIndex = GetLastIndex(provider);

            var providerStatus = new ReportStatus
            {
                status = Status.GET_LAST_TRANSNR,
                last_transaction_number = lastIndex                
            };

            var subscriberStatus = GetDatasetStatus();

            ReportStatus(providerStatus);
            
            if (subscriberStatus.last_copy_transaction_number == lastIndex)
            {
                providerStatus.status = Status.NO_CHANGES;

                providerStatus.message = "No new changes found";
                
                return providerStatus;
            }

            if (subscriberStatus.last_copy_transaction_number > lastIndex)
            {
                providerStatus.status = Status.UNKNOWN_ERROR;

                providerStatus.message = "Subscriber reports higher index than Provider";

                return providerStatus;
            }

            providerStatus.status = Status.HAS_CHANGES;

            ReportStatus(providerStatus);

            providerStatus.status = Status.GENERATE_CHANGES;

            ReportStatus(providerStatus);

            try
            {
                await GetNewChangelogAsync(subscriberStatus, provider);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                providerStatus.status = Status.GENERATE_CHANGES_FAILED;

                return providerStatus;
            }

            providerStatus.status = Status.WRITE_CHANGES;

            ReportStatus(providerStatus);

            return WriteChanges(GetActiveChangelog(subscriberStatus.last_copy_transaction_number), lastIndex, providerStatus);            
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

        private static ReportStatus WriteChanges(Kartverket.GeosyncWCF.ChangelogType changelogType, int lastIndex, ReportStatus reportStatus)
        {
            var changelogPath = GetChangelogPath(changelogType.downloadUri);

            var url = GetDatasetUrl("features") + $"?copy_transaction_number={lastIndex}&dataset_version={_currentSubscriber.dataset.Version}&async=true&locking_type=all_lock&validation_mode=loose";
            //var url = GetDatasetUrl("features") + $"?copy_transaction_number={lastIndex}&dataset_version={_currentSubscriber.dataset.Version}&async=true&locking_type=all_lock";

            var stream = File.OpenRead(changelogPath);

            var streamContent = new StreamContent(stream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(ChangelogHeader);

            var result = Client.PostAsync(url, streamContent).Result;

            TestForSuccess(result);

            var status = result.Headers.GetValues("Location").FirstOrDefault();

            var statusResult = Client.GetAsync(status).Result;

            while (statusResult.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                statusResult = Client.GetAsync(status).Result;

                Task.Delay(2000).Wait();
            }

            var message = statusResult.Content.ReadAsStringAsync().Result;

            Console.WriteLine("INFO: " + message);

            reportStatus.message = JsonSerializer.Deserialize<dynamic>(message);

          
            reportStatus.status = TestForSuccess(statusResult) 
                ? Status.WRITE_CHANGES_OK
                : Status.UNKNOWN_ERROR;

            reportStatus.last_transaction_number = lastIndex;

            reportStatus.last_copy_transaction_number = int.Parse(changelogType.endIndex);

            return reportStatus;
        }

        private static string GetChangelogPath(string downloadUri)
        {
            var zipFileName = downloadUri.Split('/').Last();

            var filLocation = AppDomain.CurrentDomain.BaseDirectory + "Changelogfiles\\" + zipFileName;

            return filLocation;
        }

        private static void ReportStatus(ReportStatus status)
        {
            var url = GetDatasetUrl($"job-status");

            var jsonString = GetJsonStringContent(status);

            var response = Client.PostAsync(url, jsonString).Result;

            Console.WriteLine($"Reported status: {status.status}");

            //if(!string.IsNullOrEmpty(status.message)) Console.WriteLine($"Reported message: {status.message}");

            TestForSuccess(response);
        }

        private static StringContent GetJsonStringContent(ReportStatus status)
        {
            return new StringContent(JsonSerializer.Serialize(status, GetJsonSerializerOptions()), Encoding.UTF8, JsonHeader);
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
            SetClientHeader(DatasetHeader);

            var response = Client.GetAsync(GetDatasetUrl()).Result;

            TestForSuccess(response);

            var result = response.Content.ReadAsStringAsync().Result;

            var status = JsonSerializer.Deserialize<DatasetStatus>(result);

            if (status.last_copy_transaction_number == null) status.last_copy_transaction_number = -1;

            return status;
        }

        private static void SetClientHeader(string header)
        {
            Client.DefaultRequestHeaders.Add("accept", header);
        }

        private static bool TestForSuccess(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 300) throw new Exception(response.ReasonPhrase);

            return true;
        }
    }
}