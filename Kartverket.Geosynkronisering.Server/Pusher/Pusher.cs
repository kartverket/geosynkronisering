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
using Pusher;
using Serilog;

namespace Provider_NetCore
{
    public class Pusher
    {
        public static Datasets_NgisSubscriber _currentSubscriber { get; private set; }

        public static List<ActiveChangelog> _activeChangelogs { get; private set; }

        private const string DatasetHeader = "application/vnd.kartverket.ngis.dataset+json";
        private const string ChangelogHeader = "application/vnd.kartverket.geosynkronisering+zip";
        private const string JsonHeader = "application/json";
        static readonly HttpClient _client = new();

        private static FeedbackController.Progress _feedProgress;// = new();
        public Pusher(FeedbackController.Progress feedProgress)
        {
            _feedProgress = feedProgress;
        }

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
            // prevent too big header
            if (_client.DefaultRequestHeaders.Contains("X-Client-Product-Version"))
                return;

            _client.DefaultRequestHeaders.Add("X-Client-Product-Version", "GeodataTest");
        }

        private static void SetCredentials()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(_currentSubscriber.subscriber.username + ":" + _currentSubscriber.subscriber.password)));
        }

        public static void Synchronize(List<Dataset> datasets)
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
                            var msg = $"Starting synchronization of dataset {_currentSubscriber.dataset.DatasetId} with url {_currentSubscriber.subscriber.url} try {tries + 1}/{maxTries}";

                            Console.WriteLine(msg);
                            Log.Information(msg);

                            _feedProgress?.OnUpdateLogListAsync(msg); // reports progress for anyone listening to event
                            _feedProgress?.OnUpdateLogListSync(msg); // reports progress for anyone listening to event

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
                                msg = $"Unable to synchronize {_currentSubscriber.dataset.DatasetId}. Final status: {finalStatus.status}.";
                                Console.WriteLine(msg);
                                Log.Information(msg);
                                _feedProgress?.OnUpdateLogListAsync(msg); // reports progress for anyone listening to event
                                _feedProgress?.OnUpdateLogListSync(msg); // reports progress for anyone listening to event

                                //if(!string.IsNullOrEmpty(finalStatus.message)) Console.WriteLine($"Final message: {finalStatus.message}.");
                            }


                        }

                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine(exception.Message);
                        Log.Error(exception, "Pusher.Synchronize");
                        ReportStatus(new ReportStatus
                        {
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
                status = Status.GET_LAST_TRANSNR
            };

            var subscriberStatus = GetDatasetStatus();

            ReportStatus(providerStatus);

            if (subscriberStatus.last_copy_transaction_number == lastIndex)
            {
                providerStatus.status = Status.NO_CHANGES;

                providerStatus.message = "No new changes found";
                Console.WriteLine(providerStatus.message);

                var myLogger = Log.Logger;
                Log.Information(providerStatus.message);

                _feedProgress?.OnUpdateLogListAsync(providerStatus.message); // reports progress for anyone listening to event
                _feedProgress?.OnUpdateLogListSync(providerStatus.message); // reports progress for anyone listening to event


                return providerStatus;
            }

            if (subscriberStatus.last_copy_transaction_number > lastIndex)
            {
                providerStatus.status = Status.UNKNOWN_ERROR;

                providerStatus.message = "Subscriber reports higher index than Provider";
                Console.WriteLine(providerStatus.message);
                Log.Information(providerStatus.message);
                Log.Information("subscriberStatus.last_copy_transaction_number:{0} lastindex provider:{1} ", subscriberStatus.last_copy_transaction_number, lastIndex);
                _feedProgress?.OnUpdateLogListAsync(providerStatus.message); // reports progress for anyone listening to event
                _feedProgress?.OnUpdateLogListSync(providerStatus.message); // reports progress for anyone listening to event


                return providerStatus;
            }

            providerStatus.status = Status.HAS_CHANGES;

            ReportStatus(providerStatus);

            providerStatus.status = Status.GENERATE_CHANGES;

            ReportStatus(providerStatus);

            var changelogStartIndex = (int)subscriberStatus.last_copy_transaction_number + 1;

            try
            {
                await GetNewChangelogAsync(changelogStartIndex, provider);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                providerStatus.status = Status.GENERATE_CHANGES_FAILED;

                return providerStatus;
            }

            var activeChangelog = GetActiveChangelog(changelogStartIndex);

            providerStatus.status = Status.WRITE_CHANGES;

            //providerStatus.last_copy_transaction_number = int.Parse(activeChangelog.endIndex);

            ReportStatus(providerStatus);

            return WriteChanges(activeChangelog, providerStatus);
        }

        private static async Task GetNewChangelogAsync(int changelogStartIndex, IChangelogProvider provider)
        {
            if (_activeChangelogs.Count > 0 && _activeChangelogs.Any(c => c.copy_transaction_number == changelogStartIndex)) return;

            var changelogOrder = OrderChangelog(provider, changelogStartIndex);

            var changelogStatus = await WaitForChangelog(changelogOrder);

            if (changelogStatus != Kartverket.GeosyncWCF.ChangelogStatusType.finished) throw new Exception("Unable to generate changelog");

            var changelog = GetChangelogManager().GetChangelog(changelogOrder.changelogId);

            _activeChangelogs.Add(new ActiveChangelog()
            {
                changelog = changelog,
                copy_transaction_number = changelogStartIndex,
                dataset = _currentSubscriber.dataset
            });
        }

        private static Kartverket.GeosyncWCF.ChangelogType GetActiveChangelog(int? last_copy_transaction_number)
        {
            return _activeChangelogs.FirstOrDefault(c => c.dataset.DatasetId == _currentSubscriber.dataset.DatasetId && c.copy_transaction_number == last_copy_transaction_number).changelog;
        }

        private static OrderChangelog OrderChangelog(IChangelogProvider provider, int last_copy_transaction_number)
        {
            provider.CreateChangelog(last_copy_transaction_number, _currentSubscriber.dataset.ServerMaxCount ?? 10000, "", _currentSubscriber.datasetid);

            return provider.OrderChangelog(last_copy_transaction_number, _currentSubscriber.dataset.ServerMaxCount ?? 10000, "", _currentSubscriber.datasetid);
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

        private static ReportStatus WriteChanges(Kartverket.GeosyncWCF.ChangelogType activeChangelog, ReportStatus reportStatus)
        {
            try
            {

                var changelogPath = GetChangelogPath(activeChangelog.downloadUri);

                var url = GetDatasetUrl("features") + $"?copy_transaction_number={int.Parse(activeChangelog.endIndex)}&dataset_version={_currentSubscriber.dataset.Version}&async=true&locking_type=all_lock&validation_mode=loose";
                //var url = GetDatasetUrl("features") + $"?copy_transaction_number={lastIndex}&dataset_version={_currentSubscriber.dataset.Version}&async=true&locking_type=all_lock";

                var stream = File.OpenRead(changelogPath);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(ChangelogHeader);

                var result = Client.PostAsync(url, streamContent).Result;

                TestForSuccess(result);

                var status = result.Headers.GetValues("Location").FirstOrDefault();
                Logger.Info("Pusher.WriteChanges status url from  result.Headers.GetValues(Location).FirstOrDefault(): {0}", status);

                var statusResult = Client.GetAsync(status).Result;

                var msg = "WAIT, processing push...";
                _feedProgress?.OnUpdateLogListAsync(msg); // reports progress for anyone listening to event
                _feedProgress?.OnUpdateLogListSync(msg); // reports progress for anyone listening to event

                Console.Write(msg);
                while (statusResult.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    statusResult = Client.GetAsync(status).Result;

                    Console.Write(".");
                    Task.Delay(2000).Wait();
                }
                Console.WriteLine();

                var message = statusResult.Content.ReadAsStringAsync().Result;
                Console.WriteLine("INFO: " + message);
                Log.Information("INFO: " + message);
                _feedProgress?.OnUpdateLogListAsync("INFO: " + message); // reports progress for anyone listening to event
                _feedProgress?.OnUpdateLogListSync("INFO: " + message); // reports progress for anyone listening to event

                try
                {
                    if (statusResult?.Content?.Headers?.ContentType?.MediaType != "text/html; charset=us-ascii")
                    {
                        // JSON doesn't like HTML
                        reportStatus.message = JsonSerializer.Deserialize<dynamic>(message);
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e, "Pusher.WriteChanges at JsonSerializer.Deserialize<dynamic>(message) status Url:" + status);
                }
                //reportStatus.message = JsonSerializer.Deserialize<dynamic>(message);

                reportStatus.status = TestForSuccess(statusResult)
                    ? Status.WRITE_CHANGES_OK
                    : Status.UNKNOWN_ERROR;

                //reportStatus.last_transaction_number = lastIndex;

                //reportStatus.last_copy_transaction_number = int.Parse(changelogType.endIndex);

                return reportStatus;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Pusher.WriteChanges:" + ex.Message);
                throw;
            }

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

            var msg = $"Reported status: {status.status}";
            Console.WriteLine(msg);
            Log.Information(msg);
            _feedProgress?.OnUpdateLogListAsync(msg); // reports progress for anyone listening to event
            _feedProgress?.OnUpdateLogListSync(msg); // reports progress for anyone listening to event

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

        public static List<NgisDataset> GetDatasets()
        {
            SetClientHeader(DatasetHeader);

            var datasetsUrl = _currentSubscriber.subscriber.url.TrimEnd('/') + $"/datasets";

            var response = Client.GetAsync(datasetsUrl).Result;

            TestForSuccess(response);

            var result = response.Content.ReadAsStringAsync().Result;

            var datasets = JsonSerializer.Deserialize<List<NgisDataset>>(result);

            return datasets;
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
            // prevent too big header
            Client.DefaultRequestHeaders.Accept.Clear();

            Client.DefaultRequestHeaders.Add("accept", header);
        }

        private static bool TestForSuccess(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 300) throw new Exception(response.ReasonPhrase);

            return true;
        }
    }
}