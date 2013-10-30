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
using System.Xml.XPath;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class WfsController
    {
        public static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

       /// <summary>
        /// Get all wfs:Insert
       /// </summary>
       /// <param name="changeLog"></param>
       /// <returns></returns>
        private int GetWfsInsert(XElement changeLog)
        {
            try
            {
                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);

                return transactions.Count();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);              
            }
        }

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
                    where item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" || item.Name == nsWfs + "Replace"
                    select item;

                return transactions;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);              
            }
        }


        /// <summary>
        /// get chlogf:transactions
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private int GetTransactions(XElement changeLog, out string attributes)
        {
            try
            {
                int count = 0;
                //
                // get chlogf:transactions
                //
                XNamespace nschlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
                IEnumerable<XElement> chlogfTransactions =
                    from item in changeLog.Descendants(nschlogf + "transactions")
                    select item;
                Console.WriteLine("chlogf:transactions:");

                var sb = new StringBuilder();
                foreach (var chlog in chlogfTransactions)
                {
                    Console.WriteLine(chlog.Attribute("service"));
                    sb.Append(chlog.Attribute("service"));
                    ++count;
                }
                attributes = sb.ToString();

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);          
            }
        }

        /// <summary>
        /// get chlogf:TransactionCollection - this is the root
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private int GetTransactionCollection(XElement changeLog, out string attributes)
        {
            try
            {
                var sb = new StringBuilder();

                //
                // get chlogf:TransactionCollection - this is the root
                //
                sb.Append(changeLog.Attribute("numberMatched"));
                sb.Append(changeLog.Attribute("numberReturned"));
                sb.Append(changeLog.Attribute("startIndex"));
                sb.Append(changeLog.Attribute("endIndex"));


                // Get the xsi:schemaLocation: http://www.falconwebtech.com/post/2010/06/03/Adding-schemaLocation-attribute-to-XElement-in-LINQ-to-XML.aspx
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                sb.AppendLine();
                sb.Append(changeLog.Attribute(xsi + "schemaLocation"));
                sb.AppendLine();

                //
                // Get the namespaces:
                //

                // Distinct() doesn't work with namespaces decleared as var!!
                IEnumerable<XNamespace> namespaces = (from x in changeLog.DescendantsAndSelf()
                                                      select x.Name.Namespace).Distinct();

                sb.AppendLine("namespaces:");
                foreach (var ns in namespaces)
                {
                    sb.AppendLine(ns.ToString());
                }

                attributes = sb.ToString();
                return 0;

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

                var xDoc = BuildWfsTransaction(changeLog);
                var xDoc2 = BuildWfsTransactionList(changeLog);
                if (xDoc == null)
                {
                    return false;
                }


                //
                // Post to GeoServer
                //
                try
                {
                    Int64 endChangeId = Convert.ToInt64(xDoc2[0].XPathSelectElement("//*[@handle][1]").Attribute("handle").Value);
                    //20121122-Leg::  Get subscriber GeoServer url from db
                 
                    var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                    String url = dataset.ClientWfsUrl;
              
                    //String url = Properties.Settings.Default.urlGeoserverSubscriber; // "http://localhost:8081/geoserver/app/ows?service=WFS";

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "text/xml"; //"application/x-www-form-urlencoded";
                    var writer = new StreamWriter(httpWebRequest.GetRequestStream());
                    xDoc.Save(writer);
                    writer.Close();

                    // get response from request
                    HttpWebResponse httpWebResponse = null;
                    Stream responseStream = null;
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

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

                    if (httpWebResponse.StatusCode == HttpStatusCode.OK && resultString.ToString().Contains("ExceptionReport") == false)
                    {
                        //TODO en får alltid status 200 OK fra geoserver
                        //En må sjekke om en har fått ExceptionReport
                        //<?xml version="1.0" encoding="UTF-8"?>
                        //<ows:ExceptionReport version="2.0.0"
                        //  xsi:schemaLocation="http://www.opengis.net/ows/1.1 http://localhost:8081/geoserver/schemas/ows/1.1.0/owsAll.xsd"
                        //  xmlns:ows="http://www.opengis.net/ows/1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                        //  <ows:Exception exceptionCode="0">
                        //    <ows:ExceptionText>Error performing insert: Error inserting features
                        //Error inserting features
                        //ERROR: insert or update on table &amp;quot;navneenhet&amp;quot; violates foreign key constraint &amp;quot;navneenhet_navnetype_fkey&amp;quot;
                        //  Detail: Key (navnetype)=() is not present in table &amp;quot;navnetype&amp;quot;.</ows:ExceptionText>
                        //  </ows:Exception>
                        //</ows:ExceptionReport>

                        sucsess = true;
                        XElement transactionResponseElement = XElement.Parse(resultString.ToString());

                        XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                        IEnumerable<XElement> transactionSummaries =
                            from item in transactionResponseElement.Descendants(nsWfs + "TransactionSummary")
                            select item;

                        if (transactionSummaries.Any())
                        {
                            string message = "Geoserver WFS-T Transaction: ";
                            string transactionSummary = transactionSummaries.ElementAt(0).ToString(SaveOptions.DisableFormatting);
                            //MessageBox.Show(message + "\r\n" + transactionSummary, "Transaction Status: " + httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                            logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + transactionSummary, httpWebResponse.StatusCode);
                            ////VisXML(tran.ToString(SaveOptions.DisableFormatting));
                            // For more debugging:
                            //logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode);

                            //listBoxLog.Items.Add("TransactionSummary:");
                            IEnumerable<XElement> transactions =
                                from item in transactionResponseElement.Descendants()
                                where item.Name == nsWfs + "totalInserted" || item.Name == nsWfs + "totalUpdated" || item.Name == nsWfs + "totalReplaced" || item.Name == nsWfs + "totalDeleted"
                                select item;
                            foreach (var tran in transactions)
                            {
                                string tranResult = "unknown";
                                if (tran.Name == nsWfs + "totalInserted")
                                {
                                    tranResult = "totalInserted";
                                }
                                else if (tran.Name == nsWfs + "totalUpdated")
                                {
                                    tranResult = "totalUpdated";
                                }
                                else if (tran.Name == nsWfs + "totalDeleted")
                                {
                                    tranResult = "totalDeleted";
                                }
                                else if (tran.Name == nsWfs + "totalReplaced")
                                {
                                    tranResult = "totalReplaced";
                                }
                            }
                        }
                        else
                        {
                            string message = "Geoserver WFS-T Transaction feilet: ";
                            logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + "No transactions ", httpWebResponse.StatusCode);                            
                        }
                    }
                    else
                    {
                        string message = "Geoserver WFS-T Transaction feilet: ";
                        logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                    }
                }
                catch (WebException webEx)
                {
                    logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                    throw new Exception("WebException error : " + webEx.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    //Response.Write(exception.Message);
                    //Response.Write(exception.ToString());
                    logger.ErrorException("GetLastIndexFromProvider WebException:", ex);
                    return false;
                }

                return sucsess;

            }
            catch (Exception)
            {

                throw;
                return false;
            }
        }

        /// <summary>
        /// Buld WFS Transaction XDocument from Changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private XDocument BuildWfsTransaction(XElement changeLog)
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
                    new XElement(nsWfs + "Transaction", new XAttribute("version", "2.0.0"), new XAttribute("service", "WFS"),
                                 new XAttribute(XNamespace.Xmlns + "wfs", "http://www.opengis.net/wfs/2.0"), xAttributeXsi)
                    );

                //
                // Get the WFS-T transactions and add them to the root
                //
                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);
                XElement xRootElement = xDoc.Root;

                foreach (var tran in transactions)
                {
                    xRootElement.Add(tran);
                }

                //
                // Estimate number of transactions for each feature type
                //


                var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                                   group item.Name.LocalName                 //operation
                              by item.Elements().ElementAt(0).Name.LocalName //(Key) typeName-for Insert it follows in the next Element 
                                       into g
                                       select g;
                // If wfs:member comes before typeName for Insert:
                //var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                //                   group item.GetDatasetName.LocalName                 //operation
                //              by item.Elements().Descendants().ElementAt(0).GetDatasetName.LocalName //(Key) typeName-for Insert it follows in the next Element 
                //                       into g
                //                       select g;

                //var inserts = (from x in changeLog.Descendants(nsWfs + "Insert")
                //                                      select x.GetDatasetName.LocalName);
                //System.Diagnostics.Debug.WriteLine("  features of {0}", inserts.Count());

                var updateGroups = from item in changeLog.Descendants(nsWfs + "Update")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Update it follows in the typeName attribute
                                       into g
                                       select g;

                var deleteGroups = from item in changeLog.Descendants(nsWfs + "Delete")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Delete it follows in the typeName attribute
                                       into g
                                       select g;
                if (insertGroups.Any())
                {
                    foreach (var group in insertGroups)
                    {
                        // Insert: count of number of Insert transactions:
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                    }
                }

                if (updateGroups.Any())
                {
                    foreach (var group in updateGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                    }
                }

                if (deleteGroups.Any())
                {
                    foreach (var group in deleteGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
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

        #region Håvard
        /// <summary>
        /// Buld WFS Transaction XDocument from Changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private List<XDocument> BuildWfsTransactionList(XElement changeLog)
        {
            try
            {
                List<XDocument> xDocList = new List<XDocument>();

                // NameSpace manipulation: http://msdn.microsoft.com/en-us/library/system.xml.linq.xnamespace.xmlns(v=vs.100).aspx
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";


                // Get the xsi:schemaLocation: http://www.falconwebtech.com/post/2010/06/03/Adding-schemaLocation-attribute-to-XElement-in-LINQ-to-XML.aspx
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XAttribute xAttributeXsi = changeLog.Attribute(xsi + "schemaLocation");

                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);

                foreach (var tran in transactions)
                {
                //
                // Generate the XDocument
                //
                XDocument xDoc = new XDocument(
                    new XElement(nsWfs + "Transaction", new XAttribute("version", "2.0.0"), new XAttribute("service", "WFS"),
                                 new XAttribute(XNamespace.Xmlns + "wfs", "http://www.opengis.net/wfs/2.0"), xAttributeXsi)
                    );

                //
                // Get the WFS-T transactions and add them to the root
                //
               
                XElement xRootElement = xDoc.Root;

                
                    xRootElement.Add(tran);

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

                    xDocList.Add(xDoc);
                }

                //
                // Estimate number of transactions for each feature type
                //


                var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                                   group item.Name.LocalName                 //operation
                              by item.Elements().ElementAt(0).Name.LocalName //(Key) typeName-for Insert it follows in the next Element 
                                       into g
                                       select g;
                // If wfs:member comes before typeName for Insert:
                //var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                //                   group item.GetDatasetName.LocalName                 //operation
                //              by item.Elements().Descendants().ElementAt(0).GetDatasetName.LocalName //(Key) typeName-for Insert it follows in the next Element 
                //                       into g
                //                       select g;

                //var inserts = (from x in changeLog.Descendants(nsWfs + "Insert")
                //                                      select x.GetDatasetName.LocalName);
                //System.Diagnostics.Debug.WriteLine("  features of {0}", inserts.Count());

                var updateGroups = from item in changeLog.Descendants(nsWfs + "Update")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Update it follows in the typeName attribute
                                       into g
                                       select g;

                var deleteGroups = from item in changeLog.Descendants(nsWfs + "Delete")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Delete it follows in the typeName attribute
                                       into g
                                       select g;
                if (insertGroups.Any())
                {
                    foreach (var group in insertGroups)
                    {
                        // Insert: count of number of Insert transactions:
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                    }
                }

                if (updateGroups.Any())
                {
                    foreach (var group in updateGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                    }
                }

                if (deleteGroups.Any())
                {
                    foreach (var group in deleteGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                    }
                }

                

                // TODO: It's not necesary to save the file here, but nice for debugging
                /*if (transactions.Count() <= 50)
                {
                    string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
                    string fileName = tempDir + @"\" + "_wfsT-test1.xml";
                    xDoc.Save(fileName);
                }*/

                return xDocList;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace + "BuildWfsTransaction");
            }
        }

        /// <summary>
        /// Build wfs-t transaction from changelog, and do the transaction
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        public bool DoWfsTransactions2(XElement changeLog, int datasetId)
        {
            bool sucsess = false;

            try
            {
                var xDocList = BuildWfsTransactionList(changeLog);
                /*if (xDoc == null)
                {
                    return false;
                }*/


                //
                // Post to GeoServer
                //
                try
                {

                    foreach (XDocument xDoc in xDocList)
                    {
                        Int64 endChangeId = Convert.ToInt64(xDoc.XPathSelectElement("//*[@handle][1]").Attribute("handle").Value);
                        var reader = xDoc.CreateReader();
                        XmlNamespaceManager manager = new XmlNamespaceManager(reader.NameTable);
                        manager.AddNamespace("gml", "http://www.opengis.net/gml/3.2");
                        //Search recursively for first occurence of attribute gml:id 
                        XElement element = xDoc.XPathSelectElement("//*[@handle][1]"/*, manager*/);
                        //20121122-Leg::  Get subscriber GeoServer url from db

                        var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                        String url = dataset.ClientWfsUrl;

                        //String url = Properties.Settings.Default.urlGeoserverSubscriber; // "http://localhost:8081/geoserver/app/ows?service=WFS";

                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                        httpWebRequest.Method = "POST";
                        httpWebRequest.ContentType = "text/xml"; //"application/x-www-form-urlencoded";
                        var writer = new StreamWriter(httpWebRequest.GetRequestStream());
                        xDoc.Save(writer);
                        writer.Close();

                        // get response from request
                        HttpWebResponse httpWebResponse = null;
                        Stream responseStream = null;
                        httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

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

                        if (httpWebResponse.StatusCode == HttpStatusCode.OK && resultString.ToString().Contains("ExceptionReport") == false)
                        {
                            //TODO en får alltid status 200 OK fra geoserver
                            //En må sjekke om en har fått ExceptionReport
                            //<?xml version="1.0" encoding="UTF-8"?>
                            //<ows:ExceptionReport version="2.0.0"
                            //  xsi:schemaLocation="http://www.opengis.net/ows/1.1 http://localhost:8081/geoserver/schemas/ows/1.1.0/owsAll.xsd"
                            //  xmlns:ows="http://www.opengis.net/ows/1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                            //  <ows:Exception exceptionCode="0">
                            //    <ows:ExceptionText>Error performing insert: Error inserting features
                            //Error inserting features
                            //ERROR: insert or update on table &amp;quot;navneenhet&amp;quot; violates foreign key constraint &amp;quot;navneenhet_navnetype_fkey&amp;quot;
                            //  Detail: Key (navnetype)=() is not present in table &amp;quot;navnetype&amp;quot;.</ows:ExceptionText>
                            //  </ows:Exception>
                            //</ows:ExceptionReport>

                            sucsess = true;
                            XElement transactionResponseElement = XElement.Parse(resultString.ToString());

                            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                            IEnumerable<XElement> transactionSummaries =
                                from item in transactionResponseElement.Descendants(nsWfs + "TransactionSummary")
                                select item;

                            if (transactionSummaries.Any())
                            {
                                string message = "Geoserver WFS-T Transaction: ";
                                string transactionSummary = transactionSummaries.ElementAt(0).ToString(SaveOptions.DisableFormatting);
                                //MessageBox.Show(message + "\r\n" + transactionSummary, "Transaction Status: " + httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                                logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + transactionSummary, httpWebResponse.StatusCode);
                                ////VisXML(tran.ToString(SaveOptions.DisableFormatting));
                                // For more debugging:
                                //logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode);

                                //listBoxLog.Items.Add("TransactionSummary:");
                                IEnumerable<XElement> transactions =
                                    from item in transactionResponseElement.Descendants()
                                    where item.Name == nsWfs + "totalInserted" || item.Name == nsWfs + "totalUpdated" || item.Name == nsWfs + "totalReplaced" || item.Name == nsWfs + "totalDeleted"
                                    select item;
                                foreach (var tran in transactions)
                                {
                                    string tranResult = "unknown";
                                    if (tran.Name == nsWfs + "totalInserted")
                                    {
                                        tranResult = "totalInserted";
                                    }
                                    else if (tran.Name == nsWfs + "totalUpdated")
                                    {
                                        tranResult = "totalUpdated";
                                    }
                                    else if (tran.Name == nsWfs + "totalDeleted")
                                    {
                                        tranResult = "totalDeleted";
                                    }
                                    else if (tran.Name == nsWfs + "totalReplaced")
                                    {
                                        tranResult = "totalReplaced";
                                    }
                                }

                                dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
                                int temp = Convert.ToInt32(endChangeId);
                                dataset.LastIndex = temp;
                                dataset.Name = "Kystcotur";
                                DL.SubscriberDatasetManager.UpdateDataset(dataset);
                            }
                            else
                            {
                                string message = "Geoserver WFS-T Transaction feilet: ";
                                logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + "No transactions ", httpWebResponse.StatusCode);
                            }
                        }
                        else
                        {
                            string message = "Geoserver WFS-T Transaction feilet: ";
                            logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                        }
                    }
                }
                catch (WebException webEx)
                {
                    logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                    throw new Exception("WebException error : " + webEx.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    //Response.Write(exception.Message);
                    //Response.Write(exception.ToString());
                    logger.ErrorException("GetLastIndexFromProvider WebException:", ex);
                    return false;
                }

                return sucsess;

            }
            catch (Exception)
            {

                throw;
                return false;
            }
        }
        #endregion
    }
}
