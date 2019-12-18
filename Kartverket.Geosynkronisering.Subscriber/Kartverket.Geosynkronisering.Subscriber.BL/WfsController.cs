using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NLog;
using System.Xml;
using Kartverket.GeosyncWCF;


namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class WfsController : FeedbackController.Progress
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback
        public SynchController ParentSynchController;

        /// <summary>
        /// Build wfs-t transaction from changelog, and do the transaction
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public bool DoWfsTransactions(XElement changeLog, int datasetId)
        {
            var success = false;

            var xDoc = ConstructWfsTransaction(changeLog);

            SaveWfsTransactionToDisk(xDoc.Root, datasetId);

            // Post to WFS-T server (e.g. deegree or GeoServer)
            try
            {
                //20121122-Leg::  Get subscriber deegree / GeoServer url from db
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                String url = dataset.ClientWfsUrl;

                var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "text/xml"; //"application/x-www-form-urlencoded";
                httpWebRequest.Timeout = System.Threading.Timeout.Infinite;
                //httpWebRequest.ReadWriteTimeout = System.Threading.Timeout.Infinite;
                //httpWebRequest.AllowWriteStreamBuffering = false;
                //httpWebRequest.SendChunked = true;

                var writer = new StreamWriter(httpWebRequest.GetRequestStream());
                    
                xDoc.Save(writer, SaveOptions.DisableFormatting);
                    
                writer.Close();
                //GC.Collect();

                // get response from request
                HttpWebResponse httpWebResponse = CheckResponseForErrors(httpWebRequest);

                success = CreateTransactionSummary(httpWebResponse);
            }
            catch (WebException webEx)
            {
                if (webEx.Message.Contains("null")) throw new Exception("WebException error: This error most often occurs when there's something wrong with namespaces. Check deegree configuration. Exception: " + webEx.Message);

                CheckApplicationSchema(datasetId);

                if (webEx.Message.ToLowerInvariant().Contains("geometry") && webEx.Message.ToLowerInvariant().Contains("invalid")) SendErrorReport(datasetId, webEx.Message);

                throw new Exception("WebException error: " + webEx.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DoWfsTransactions Exception (inner): ");

                if (ex.Source != "System") SendErrorReport(datasetId, ex.Message);

                return false;
            }

            return success;
        }

        private void CheckApplicationSchema(int datasetId)
        {
            var dataset = SubscriberDatasetManager.GetDataset(datasetId);

            var client = SynchController.buildClient(dataset);

            var providerDatasets = CapabilitiesDataBuilder.ReadGetCapabilities(client);

            if (providerDatasets.Any(d =>
                d.TargetNamespace == dataset.TargetNamespace &&
                d.ProviderDatasetId == dataset.ProviderDatasetId)) return;

            var errorMessage = $"No datasets on provider for datasetId {dataset.ProviderDatasetId} with applicationSchema {dataset.TargetNamespace}";
            SendErrorReport(datasetId, errorMessage);

            throw new Exception($"WebException error: {errorMessage}");
        }

        private void SendErrorReport(int datasetId, string errorMessage)
        {
            var dataset = SubscriberDatasetManager.GetDataset(datasetId);

            var changelogId = GetChangelogId(dataset);

            ParentSynchController.SendReport(new ReportType
            {
                datasetId = dataset.ProviderDatasetId,
                changelogId = changelogId,
                description = errorMessage,
                subscriberType = "Felleskomponent",
                type = ReportTypeEnumType.error
            }, datasetId);
        }

        private static string GetChangelogId(Dataset dataset)
        {
            return string.IsNullOrEmpty(dataset.AbortedChangelogId) ? string.IsNullOrEmpty(dataset.AbortedChangelogPath) ? "unknown"
                            : dataset.AbortedChangelogPath.Split('\\')[dataset.AbortedChangelogPath.Split('\\').Length - 1].Replace(".zip", "") : dataset.AbortedChangelogId;
        }

        private HttpWebResponse CheckResponseForErrors(HttpWebRequest httpWebRequest)
        {
            try //20160411-Leg: Improved error handling
            {
                return (HttpWebResponse) httpWebRequest.GetResponse();
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse) wex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            if (error.Contains("ExceptionReport"))
                            {

                                // Check if response is an exception
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.LoadXml(error);
                                XmlNode root = xmldoc.DocumentElement;
                                if (root != null && root.Name.Contains("ows:ExceptionReport"))
                                {
                                    string wfsMessage =
                                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  Transaction Status:" +
                                        errorResponse.StatusCode + " " + errorResponse.StatusDescription +
                                        "\r\n" + error;
                                    Logger.Info(wfsMessage);
                                    Logger.Error("DoWfsTransactions WebException:" + wfsMessage, wex);
                                    this.ParentSynchController.OnUpdateLogList(root.InnerText);
                                    throw new WebException(root.InnerText);
                                }
                                else
                                {
                                    string wfsMessage =
                                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  Transaction Status:" +
                                        errorResponse.StatusCode + " " + errorResponse.StatusDescription +
                                        "\r\n" + error;
                                    Logger.Info(wfsMessage);
                                    Logger.Error("DoWfsTransactions WebException:" + wfsMessage, wex);
                                    ParentSynchController.OnUpdateLogList(wfsMessage);
                                    throw new Exception("WebException error : " + wex.Message);

                                }
                            }
                            //Not an exception-report. Likely the service could not be found at the given url.
                            Logger.Info(error);
                            Logger.Error("DoWfsTransactions WebException:" + error, wex);
                            ParentSynchController.OnUpdateLogList(
                                "Error occured. Message from server: " + error);
                            throw new Exception("WebException error : " + wex);

                        }
                    }
                }
                Logger.Error("DoWfsTransactions WebException:", wex);

                throw new Exception("WebException error : " + wex.Message);
            }
        }

        private XDocument ConstructWfsTransaction(XElement changeLog)
        {
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";

            List<string> skipList =
                new List<string>(new[] {"startIndex", "endIndex", "numberMatched", "numberReturned", "timeStamp"});

            XElement root = ConstructWfsTransactionRoot();
                
            foreach (XAttribute att in changeLog.Attributes())
            {
                if (skipList.Contains(att.Name.LocalName))
                    continue;
                root.Add(att);
            }

            foreach (XElement wfsOperation in changeLog.Element(
                nsChlogf + "transactions")
                .Elements())
            {
                root.Add(wfsOperation);
            }

            return new XDocument(root);
        }

        private void SaveWfsTransactionToDisk(XElement root, int datasetId)
        {
            XDocument xDoc = new XDocument(root);
            string tempDir = Environment.GetEnvironmentVariable("TEMP");
            string fileName = tempDir + @"\" + "Kartverket.Geosynkronisering.Subscriber.DatasetId." + datasetId + ".LastTransaction.xml";
            xDoc.Save(fileName);
        }

        private XElement ConstructWfsTransactionRoot()
        {
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";

            XElement root = new XElement(nsWfs + "Transaction");

            XAttribute service = new XAttribute("service", "WFS");
            root.Add(service);

            XAttribute version = new XAttribute("version", "2.0.0");
            root.Add(version);

            return root;
        }

        private bool CreateTransactionSummary(HttpWebResponse httpWebResponse)
        {
            bool success = false;
            var resultString = new StringBuilder("");
            using (var resultStream = httpWebResponse.GetResponseStream())
            {
                if (resultStream != null)
                {
                    int count;
                    do
                    {
                        var buffer = new byte[8192];
                        count = resultStream.Read(buffer, 0, buffer.Length);
                        if (count == 0) continue;
                        resultString.Append(Encoding.UTF8.GetString(buffer, 0, count));
                    } while (count > 0);
                }
            }
            //httpWebResponse.Close();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK &&
                resultString.ToString().Contains("ExceptionReport") == false)
            {

                success = true;
                XElement transactionResponseElement = XElement.Parse(resultString.ToString());

                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                IEnumerable<XElement> transactionSummaries =
                    from item in transactionResponseElement.Descendants(nsWfs + "TransactionSummary")
                    select item;

                if (transactionSummaries.Any())
                {
                    string transactionSummary =
                        transactionSummaries.ElementAt(0).ToString(SaveOptions.DisableFormatting);

                    string wfsMessage =
                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction: transactionSummary" +
                        " Transaction Status:" + httpWebResponse.StatusCode + "\r\n" + transactionSummary;

                    Logger.Info(wfsMessage);
                    IEnumerable<XElement> transactions =
                        from item in transactionResponseElement.Descendants()
                        where
                            item.Name == nsWfs + "totalInserted" || item.Name == nsWfs + "totalUpdated" ||
                            item.Name == nsWfs + "totalReplaced" || item.Name == nsWfs + "totalDeleted"
                        select item;
                    string tranResult = "";
                    foreach (var tran in transactions)
                        tranResult += UpdateSyncTransactionSummary(tran);

                    // Raise event to eventual UI
                    var logMessage = tranResult;
                    ParentSynchController.OnUpdateLogList(logMessage);

                }
                else
                {
                    string wfsMessage =
                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  transactionSummary" +
                        " Transaction Status:" + httpWebResponse.StatusCode + "\r\n" + "No transactions ";
                    Logger.Info(wfsMessage);
                    ParentSynchController.OnUpdateLogList(wfsMessage);
                }
            }
            else
            {
                string wfsMessage =
                    "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  Transaction Status:" +
                    httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription + "\r\n" +
                    resultString;
                Logger.Info(wfsMessage);
                ParentSynchController.OnUpdateLogList(wfsMessage);
            }
            httpWebResponse.Close();
            return success;
        }

        private string UpdateSyncTransactionSummary(XElement tran)
        {
            int tmpValue = Convert.ToInt32(tran.Value);
            switch (tran.Name.LocalName)
            {
                case "totalInserted":
                    ParentSynchController.TransactionsSummary.TotalInserted += tmpValue;
                    break;
                case "totalUpdated":
                    ParentSynchController.TransactionsSummary.TotalUpdated += tmpValue;
                    break;
                case "totalDeleted":
                    ParentSynchController.TransactionsSummary.TotalDeleted += tmpValue;
                    break;
                case "totalReplaced":
                    ParentSynchController.TransactionsSummary.TotalReplaced += tmpValue;
                    break;
                default:
                    return "unknown ";
            }
            return tran.Name.LocalName + " " + tmpValue + " ";
        }
    }
}