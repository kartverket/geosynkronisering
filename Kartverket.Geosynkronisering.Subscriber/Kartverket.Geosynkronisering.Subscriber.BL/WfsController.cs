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
        /// Get all wfs transactions (Insert, Update,  Delete, Replace) from changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private IEnumerable<XElement> GetWfsTransactions(XElement changeLog)
        {
            try
            {
                // Namespace must be set: xmlns:wfs="http://www.opengis.net/wfs/2.0"
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";

                IEnumerable<XElement> transactions =
                    from item in changeLog.Descendants()
                    where
                        item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" ||
                        item.Name == nsWfs + "Replace"
                    select item;

                return transactions;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Build wfs-t transaction from changelog, and do the transaction
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        public bool DoWfsTransactions(XElement changeLog, int datasetId)
        {
            bool sucsess = false;

            try
            {

                var xDoc = BuildWfsTransaction(changeLog, datasetId);
                //var xDoc2 = BuildWfsTransactionList(changeLog);
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

                        sucsess = true;
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

                return sucsess;

            }
            catch (Exception ex)
            {
                logger.ErrorException("DoWfsTransactions failed:", ex);
                throw;
                return false;
            }
        }

        /// <summary>
        /// Buld WFS Transaction XDocument from Changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// 
        /// <param name="datasetId"></param>
        /// <returns></returns>
        private XDocument BuildWfsTransaction(XElement changeLog, int datasetId = 0)
        {
            try
            {
                // NameSpace manipulation: http://msdn.microsoft.com/en-us/library/system.xml.linq.xnamespace.xmlns(v=vs.100).aspx
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";


                // Get the xsi:schemaLocation: http://www.falconwebtech.com/post/2010/06/03/Adding-schemaLocation-attribute-to-XElement-in-LINQ-to-XML.aspx
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XAttribute xAttributeXsi = changeLog.Attribute(xsi + "schemaLocation");

                //
                // Generate the XDocument
                //
                XDocument xDoc = new XDocument(
                    new XElement(nsWfs + "Transaction", new XAttribute("version", "2.0.0"),
                        new XAttribute("service", "WFS"),
                        new XAttribute(XNamespace.Xmlns + "wfs", "http://www.opengis.net/wfs/2.0"), xAttributeXsi)
                    );

                //
                // Get the WFS-T transactions and add them to the root
                //
                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);

                #region substitute_Replace_with_DeleteandInsert

                // 20151006-Leg: Substutute wfs:replace with wfs:Delete and wfs:Insert.
                bool replaced = WfsSubstituteReplaceWithDeleteAndInsert(changeLog, datasetId, transactions);
                logger.Info("WfsSubstituteReplaceWithDeleteAndInsert() returned {0}", replaced);

                #endregion // substitute_Replace_with_DeleteandInsert

                XElement xRootElement = xDoc.Root;

                foreach (var tran in transactions)
                {
                    xRootElement.Add(tran);
                }

                //
                // Estimate number of transactions for each feature type
                //

                var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                    group item.Name.LocalName //operation
                        by item.Elements().ElementAt(0).Name.LocalName
                    //(Key) typeName-for Insert it follows in the next Element 
                    into g
                    select g;

                var updateGroups = from item in changeLog.Descendants(nsWfs + "Update")
                    group item.Name.LocalName //operation
                        by item.Attribute("typeName").Value
                    //(Key) typeName-for Update it follows in the typeName attribute
                    into g
                    select g;

                var deleteGroups = from item in changeLog.Descendants(nsWfs + "Delete")
                    group item.Name.LocalName //operation
                        by item.Attribute("typeName").Value
                    //(Key) typeName-for Delete it follows in the typeName attribute
                    into g
                    select g;

                //20151006-Leg: wfs:Replace
                var replaceGroups = from item in changeLog.Descendants(nsWfs + "Replace")
                    group item.Name.LocalName //operation
                        by item.Elements().ElementAt(0).Name.LocalName
                    //(Key) typeName-for Insert it follows in the next Element 
                    into g
                    select g;

                if (insertGroups.Any())
                {
                    foreach (var group in insertGroups)
                    {
                        // Insert: count of number of Insert transactions:
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(),
                            group.Key);
                    }
                }

                if (updateGroups.Any())
                {
                    foreach (var group in updateGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(),
                            group.Key);
                    }
                }

                if (deleteGroups.Any())
                {
                    foreach (var group in deleteGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(),
                            group.Key);
                    }
                }

                //20151006-Leg: wfs:Replace
                if (replaceGroups.Any())
                {
                    foreach (var group in replaceGroups)
                    {
                        // Insert: count of number of Insert transactions:
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(),
                            group.Key);
                    }
                }


                //
                // Get the namespaces, and update the XDocument:
                // See: http://www.hanselman.com/blog/GetNamespacesFromAnXMLDocumentWithXPathDocumentAndLINQToXML.aspx
                // 
                var result = changeLog.Attributes().
                    Where(a => a.IsNamespaceDeclaration).
                    GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                        a => XNamespace.Get(a.Value)).
                    ToDictionary(g => g.Key,
                        g => g.First());

                XElement xEl = xDoc.Root;
                foreach (var xns in result)
                {
                    try
                    {
                        xEl.Add(new XAttribute(XNamespace.Xmlns + xns.Key, xns.Value));
                    }
                    catch (Exception)
                    {
                        //TODO: FIX
                    }
                }

                // TODO: It's not necesary to save the file here, but nice for debugging
                if (transactions.Count() <= 50)
                {
                    string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
                    string fileName = tempDir + @"\" + "_wfsT-test1.xml";
                    xDoc.Save(fileName);
                }

                return xDoc;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace + "BuildWfsTransaction");
            }
        }

        /// <summary>
        /// Substutute wfs:replace with wfs:Delete and wfs:Insert.
        /// </summary>
        /// <param name="changeLog">The change log.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="transactions">The transactions.</param>
        private static bool WfsSubstituteReplaceWithDeleteAndInsert(XElement changeLog, int datasetId,
            IEnumerable<XElement> transactions)
        {
            bool replaced = false;
            try
            {
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
                // 20151006-Leg: wfs:Replace
                // Substutute wfs:replace with wfs:Delete and wfs:Insert
                // wfs:Delete: Filter part of wfs:Replace
                // wfs:Insert: All execpt Filter of wfs:Replace

                string nsPrefixApp = "";
                XNamespace nsApp = null;
                if (datasetId > 0)
                {
                    var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
                    string namespaceUri = dataset.TargetNamespace;
                    nsPrefixApp = changeLog.GetPrefixOfNamespace(namespaceUri);
                    nsApp = namespaceUri;
                }

                foreach (var ele in transactions.ToList())
                {
                    if ((ele.Name == nsWfs + "Replace") && !ele.IsEmpty)
                    {
                        XNamespace nsFes = "http://www.opengis.net/fes/2.0";

                        //
                        // wfs:delete part
                        //
                        var filter = ele.DescendantsAndSelf(nsFes + "Filter");
                        var typename = ele.Elements().ElementAt(0).Name.LocalName;
                        var xNameFeaturetype = ele.Elements().ElementAt(0).Name; // Gets the full name of this element.
                        Console.WriteLine("featureType: {0} wfs:{1}", typename, ele.Name);
                        var handle = ele.Attribute("handle").Value;
                        Console.WriteLine("handle: {0}", handle);
                        var ns = ele.Elements().ElementAt(0).Name;
                        XElement deleteElement;
                        if (nsApp == null)
                        {
                            // nameSpace must be set for deegree, so wthis will not work
                            deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", handle),
                                new XAttribute("typeName", typename));
                        }
                        else
                        {
                            deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", handle),
                                new XAttribute("typeName", nsPrefixApp + ":" + typename),
                                new XAttribute(XNamespace.Xmlns + nsPrefixApp, nsApp));
                        }


                        deleteElement.Add(filter); // Filter part
                        ele.Parent.AddBeforeSelf(deleteElement);

                        //
                        // wfs:Insert part
                        //
                        XElement insertElement = new XElement(nsWfs + "Insert", new XAttribute("handle", handle));
                        IEnumerable<XElement> replaceElem = ele.Descendants().Take(1);

                        insertElement.Add(replaceElem);
                        ele.Parent.AddBeforeSelf(insertElement);

                        ele.Remove(); //remove wfs:replace
                        replaced = true;
                    }
                }
                return replaced;
            }
            catch (Exception ex)
            {
                logger.ErrorException("WfsSubstituteReplaceWithDeleteAndInsert failed:", ex);
                throw new Exception(ex.Message + ex.StackTrace + "WfsSubstituteReplaceWithDeleteAndInsert");
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
                            else
                            {
                                logger.Info(error);
                                logger.ErrorException("DoWfsTransactions WebException:" + error, wex);
                                this.ParentSynchController.OnUpdateLogList(
                                    "Error occured. Message from server: " + error);
                                throw new Exception("WebException error : " + wex);
                            }
                        }
                    }
                }
                logger.ErrorException("DoWfsTransactions WebException:", wex);

                throw new Exception("WebException error : " + wex.Message);
            }
        }
    }
}