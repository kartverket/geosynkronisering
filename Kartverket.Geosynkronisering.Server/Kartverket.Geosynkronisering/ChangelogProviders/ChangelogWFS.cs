using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    public class ChangelogWFS
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public XElement GetFeatureCollectionFromWFS(string nsPrefixTargetNamespace, string nsAppStr, string wfsUrl, ref Dictionary<string, string> typeIdDict, List<string> gmlIds, int datasetId)
        {
            logger.Info("GetFeatureCollectionFromWFS START");
            XNamespace nsApp = nsAppStr;
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";
            
            //
            // NameSpace prefix must be equal to providers prefix, get it from the settings database.
            // i.e. "app" will not work if TargetNamespacePrefix = "kyst" or "ar5"
            //
            //string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            //changeLog.GetPrefixOfNamespace(nsApp);
            if (String.IsNullOrWhiteSpace(nsPrefixTargetNamespace))
            {
                nsPrefixTargetNamespace = "app"; //Shouldn't happen, but works with GeoServer, and is compatible with earlier versions of code
            }

            XDocument wfsGetFeatureDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(nsWfs + "GetFeature",
                new XAttribute("version", "2.0.0"),
                new XAttribute("service", "WFS"),
                new XAttribute(XNamespace.Xmlns + nsPrefixTargetNamespace, nsApp),
                new XAttribute(XNamespace.Xmlns + "wfs", nsWfs),
                new XAttribute(XNamespace.Xmlns + "fes", nsFes),
                new XAttribute("resolveDepth", "*")
                // new XElement(nsWfs + "GetFeature", new XAttribute("version", "2.0.0"), new XAttribute("service", "WFS"), new XAttribute(XNamespace.Xmlns + "app", nsApp), new XAttribute(XNamespace.Xmlns + "wfs", nsWfs), new XAttribute(XNamespace.Xmlns + "fes", nsFes)
                )
            );

            PopulateDocumentForGetFeatureRequest(nsPrefixTargetNamespace, gmlIds, ref typeIdDict, wfsGetFeatureDocument, datasetId);

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(wfsUrl) as HttpWebRequest;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "text/xml";
                StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream());
                wfsGetFeatureDocument.Save(writer);
                //wfsGetFeatureDocument.Save("C:\\temp\\gvtest_query.xml");
                logger.Debug("GetFeature: " + wfsGetFeatureDocument.ToString());
                writer.Close();

                HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;

                XElement getFeatureResponse = XElement.Load(response.GetResponseStream());
                //getFeatureResponse.Save("C:\\temp\\gvtest_response.xml");
                logger.Info("GetFeatureCollectionFromWFS END");
                return getFeatureResponse;
            }
            catch (System.Exception exp)
            {
                //20130821-Leg:Added logging of wfsGetFeatureDocument
                logger.ErrorException("GetFeatureCollectionFromWFS: wfsGetFeatureDocument:" + wfsGetFeatureDocument.ToString() + "\r\n" + "GetFeatureCollectionFromWFS function failed:", exp);
                throw new System.Exception("GetFeatureCollectionFromWFS function failed", exp);
            }
        }

        private void PopulateDocumentForGetFeatureRequest(string nsPrefixTargetNamespace, List<string> gmlIds, ref Dictionary<string, string> typeIdDict, XDocument wfsGetFeatureDocument, int datasetId)
        {
            PopulateDocumentForGetFeatureRequest_simple(nsPrefixTargetNamespace, gmlIds, ref typeIdDict, wfsGetFeatureDocument, datasetId);
            /*XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";

            string nsPrefixTargetNamespace = Database.DatasetsData.TargetNamespacePrefix(datasetId);
            string nsPrefixTargetNamespaceComplete = nsPrefixTargetNamespace + ":";
            if (String.IsNullOrWhiteSpace(nsPrefixTargetNamespace))
            {
                nsPrefixTargetNamespace = "app"; //Shouldn't happen, but works with GeoServer
                nsPrefixTargetNamespaceComplete = "";
            }

           
            //if (String.IsNullOrWhiteSpace(nsPrefixTargetNamespace))
            //{
            //    nsPrefixTargetNamespaceComplete = "";
            //}
            
            string lokalidValrefContent = nsPrefixTargetNamespaceComplete + "identifikasjon/" + nsPrefixTargetNamespaceComplete +
                                            "Identifikasjon/" + nsPrefixTargetNamespaceComplete + "lokalId";

            //Build dictionary with list of localids for each typename
            Dictionary<string, List<string>> localidsForTypename = new Dictionary<string, List<string>>();
            foreach (string gmlId in gmlIds)
            {
                string typename = "";
                int pos = gmlId.IndexOf(".");
                typename = gmlId.Substring(0, pos);
                string localId = gmlId.Substring(pos + 1);

                //Add list for typename if list does not exist
                List<string> localIds;
                if (localidsForTypename.TryGetValue(typename, out localIds))
                {
                    localIds.Add(localId);
                }
                else
                {
                    localIds = new List<string>();
                    localIds.Add(localId);
                    localidsForTypename.Add(typename, localIds);
                    typeNames.Add(typename);
                }
            }

            //Go through localidsForTypename and build GetFeatureRequest
            foreach (KeyValuePair<string, List<string>> typenameLocalIds in localidsForTypename)
            {
                string typename = typenameLocalIds.Key;
                List<string> localIds = typenameLocalIds.Value;
                int numLocalIds = localIds.Count;
                XElement filterElement = new XElement(nsFes + "Filter");
                if (numLocalIds == 1)
                {
                    string localId = localIds.ElementAt(0);
                    //ValueReference content has namespace prefix
                    filterElement.Add(new XElement(nsFes + "PropertyIsEqualTo", new XElement(nsFes + "ValueReference", lokalidValrefContent), new XElement(nsFes + "Literal", localId)));
                    //filterElement.Add(new XElement("PropertyIsEqualTo", new XElement("ValueReference", "identifikasjon/Identifikasjon/lokalId"), new XElement("Literal", localId)));
                }
                else
                {
                    XElement orElement = new XElement(nsFes + "Or");
                    foreach (string localId in localIds)
                    {
                        //ValueReference content has namespace prefix
                        orElement.Add(new XElement(nsFes + "PropertyIsEqualTo", new XElement(nsFes + "ValueReference", lokalidValrefContent), new XElement(nsFes + "Literal", localId)));
                        //orElement.Add(new XElement("PropertyIsEqualTo", new XElement("ValueReference", "identifikasjon/Identifikasjon/lokalId"), new XElement("Literal", localId)));
                    }
                    filterElement.Add(orElement);
                }

                wfsGetFeatureDocument.Element(nsWfs + "GetFeature").Add(new XElement(nsWfs + "Query", new XAttribute("typeNames", nsPrefixTargetNamespace + ":" + typename), filterElement));
                //wfsGetFeatureDocument.Element(nsWfs + "GetFeature").Add(new XElement(nsWfs + "Query", new XAttribute("typeNames", "app:" + typename), filterElement));
            }*/
        }

        /// <summary>
        /// Execute get feature request for each feature
        /// </summary>
        /// <param name="gmlIds">List of gml ids</param>
        /// <param name="typeIdDict">Dictonary containing type and gml id</param>
        /// <param name="wfsGetFeatureDocument"></param>
        private void PopulateDocumentForGetFeatureRequest_simple(string nsPrefixTargetNamespace, List<string> gmlIds, ref Dictionary<string, string> typeIdDict, XDocument wfsGetFeatureDocument, int datasetId)
        {            
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";

            string nsPrefixTargetNamespaceComplete = nsPrefixTargetNamespace + ":";
            if (String.IsNullOrWhiteSpace(nsPrefixTargetNamespace))
            {
                nsPrefixTargetNamespace = "app"; //Shouldn't happen, but works with GeoServer
                nsPrefixTargetNamespaceComplete = "";
            }

            XElement orElement = new XElement("Or");
            string typename = "";

            // 20131101-Leg: ValueReference content has namespace prefix
            string lokalidValrefContent = nsPrefixTargetNamespaceComplete + "identifikasjon/" + nsPrefixTargetNamespaceComplete +
                                           "Identifikasjon/" + nsPrefixTargetNamespaceComplete + "lokalId";
            
            foreach (string gmlId in gmlIds)
            {
                
                int pos = gmlId.IndexOf(".");
                typename = gmlId.Substring(0, pos);
                string localId = gmlId.Substring(pos + 1);

                typeIdDict.Add(localId, typename);

                XElement filterElement = new XElement(nsFes + "Filter");
                
                // 20131101-Leg: ValueReference content has namespace prefix
                filterElement.Add(new XElement(nsFes + "PropertyIsEqualTo", new XElement(nsFes + "ValueReference", lokalidValrefContent), new XElement(nsFes + "Literal", localId)));
                // filterElement.Add(new XElement("PropertyIsEqualTo", new XElement("ValueReference", "identifikasjon/Identifikasjon/lokalId"), new XElement("Literal", localId)));
                
                wfsGetFeatureDocument.Element(nsWfs + "GetFeature").Add(new XElement(nsWfs + "Query", new XAttribute("typeNames", nsPrefixTargetNamespace + ":" + typename), filterElement));               
            }                      
        }            
    }
}