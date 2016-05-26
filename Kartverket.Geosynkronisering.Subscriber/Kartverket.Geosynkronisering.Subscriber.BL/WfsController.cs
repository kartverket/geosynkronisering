using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NLog;
using System.Xml;


namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class WfsController : FeedbackController.Progress
    {
        public static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback
        public SynchController ParentSynchController;

        /// <summary>
        /// Build wfs-t transaction from changelog, and do the transaction
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        public bool DoWfsTransactions(XElement changeLog, int datasetId)
        {
            bool success = false;

            try
            {

                var xDoc = constructWfsTransaction(changeLog);


                if (xDoc == null)
                {
                    return false;
                }


                //
                // Post to WFS-T server (e.g. deegree or GeoServer)
                //
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
                    xDoc.Root.RemoveAll();
                    writer.Close();
                    //GC.Collect();
                    // get response from request
                    HttpWebResponse httpWebResponse = CheckResponseForErrors(httpWebRequest);
                    success = createTransactionSummary(httpWebResponse);
                }
                catch (WebException webEx)
                {
                    logger.ErrorException("DoWfsTransactions WebException:", webEx);
                    throw new Exception("WebException error : " + webEx.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    logger.ErrorException("DoWfsTransactions Exception (inner):", ex);
                    return false;
                }

                return success;

            }
            catch (Exception ex)
            {
                logger.ErrorException("DoWfsTransactions failed:", ex);
                throw;
                return false;
            }
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
                            if (error.ToString().Contains("ExceptionReport"))
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
                                        "\r\n" + error.ToString();
                                    logger.Info(wfsMessage);
                                    logger.ErrorException("DoWfsTransactions WebException:" + wfsMessage, wex);
                                    this.ParentSynchController.OnUpdateLogList(root.InnerText);
                                    throw new WebException(root.InnerText);
                                }
                                else
                                {
                                    string wfsMessage =
                                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  Transaction Status:" +
                                        errorResponse.StatusCode + " " + errorResponse.StatusDescription +
                                        "\r\n" + error.ToString();
                                    logger.Info(wfsMessage);
                                    logger.ErrorException("DoWfsTransactions WebException:" + wfsMessage, wex);
                                    this.ParentSynchController.OnUpdateLogList(wfsMessage);
                                    throw new Exception("WebException error : " + wex.Message);

                                }
                            }
                            //Not an exception-report. Likely the service could not be found at the given url.
                            logger.Info(error);
                            logger.ErrorException("DoWfsTransactions WebException:" + error, wex);
                            this.ParentSynchController.OnUpdateLogList(
                                "Error occured. Message from server: " + error);
                            throw new Exception("WebException error : " + wex);

                        }
                    }
                }
                logger.ErrorException("DoWfsTransactions WebException:", wex);

                throw new Exception("WebException error : " + wex.Message);
            }
        }

        private XDocument constructWfsTransaction(XElement changeLog)
        {
            string geosyncNs = "{http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg}";

            string geosyncNsOld = "{http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg}";

            List<string> skipList =
                new List<string>(new string[] {"startIndex", "endIndex", "numberMatched", "numberReturned", "timeStamp"});

            XElement root = new XElement("{http://www.opengis.net/wfs/2.0}Transaction");

            XAttribute service = new XAttribute("service", "WFS");
            root.Add(service);

            XAttribute version = new XAttribute("version", "2.0.0");
            root.Add(version);

            foreach (XAttribute att in changeLog.Attributes())
            {
                if (skipList.Contains(att.Name.LocalName))
                    continue;
                root.Add(att);
            }
            if (changeLog.Element(geosyncNs + "transactions") == null)
            {
                ParentSynchController.OnUpdateLogList(
                    "WARNING: No transactions found for namespace " + geosyncNs + ". Trying to run transaction using " + geosyncNsOld);
                geosyncNs = geosyncNsOld;
            }

            foreach (XElement wfsOperation in changeLog.Element(
                geosyncNs + "transactions")
                .Elements())
            {
                root.Add(wfsOperation);
            }

            XDocument xDoc = new XDocument(root);

            string tempDir = Environment.GetEnvironmentVariable("TEMP");
            string fileName = tempDir + @"\" + "_wfsT-test1.xml";
            xDoc.Save(fileName);

            return xDoc;
        }

        private bool createTransactionSummary(HttpWebResponse httpWebResponse)
        {
            bool success = false;
            Stream responseStream = null;
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
            httpWebResponse.Close();
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

                    logger.Info(wfsMessage);
                    IEnumerable<XElement> transactions =
                        from item in transactionResponseElement.Descendants()
                        where
                            item.Name == nsWfs + "totalInserted" || item.Name == nsWfs + "totalUpdated" ||
                            item.Name == nsWfs + "totalReplaced" || item.Name == nsWfs + "totalDeleted"
                        select item;
                    string tranResult = "";
                    foreach (var tran in transactions)
                    {
                        //string tranResult = "unknown";
                        if (tranResult.Length > 0)
                        {
                            tranResult += " ";
                        }
                        if (tran.Name == nsWfs + "totalInserted")
                        {
                            tranResult += "totalInserted" + ":" + tran.Value;
                            this.ParentSynchController.TransactionsSummary.TotalInserted +=
                                Convert.ToInt32(tran.Value);
                        }
                        else if (tran.Name == nsWfs + "totalUpdated")
                        {
                            tranResult += "totalUpdated" + ":" + tran.Value;
                            //tranResult = "totalUpdated";
                            this.ParentSynchController.TransactionsSummary.TotalUpdated +=
                                Convert.ToInt32(tran.Value);
                        }
                        else if (tran.Name == nsWfs + "totalDeleted")
                        {
                            tranResult += "totalDeleted" + ":" + tran.Value;
                            this.ParentSynchController.TransactionsSummary.TotalDeleted +=
                                Convert.ToInt32(tran.Value);
                        }
                        else if (tran.Name == nsWfs + "totalReplaced")
                        {
                            tranResult += "totalReplaced" + ":" + tran.Value;
                            this.ParentSynchController.TransactionsSummary.TotalReplaced +=
                                Convert.ToInt32(tran.Value);
                        }
                        else
                        {
                            tranResult = "unknown";
                        }
                    }

                    // Raise event to eventual UI
                    var logMessage = tranResult;
                    this.ParentSynchController.OnUpdateLogList(logMessage);

                }
                else
                {
                    string wfsMessage =
                        "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  transactionSummary" +
                        " Transaction Status:" + httpWebResponse.StatusCode + "\r\n" + "No transactions ";
                    logger.Info(wfsMessage);
                    this.ParentSynchController.OnUpdateLogList(wfsMessage);
                }
            }
            else
            {
                string wfsMessage =
                    "DoWfsTransactions: deegree/Geoserver WFS-T Transaction feilet:  Transaction Status:" +
                    httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription + "\r\n" +
                    resultString.ToString();
                logger.Info(wfsMessage);
                this.ParentSynchController.OnUpdateLogList(wfsMessage);
            }
            return success;
        }
    }
}