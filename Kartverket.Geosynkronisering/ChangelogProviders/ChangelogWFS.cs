﻿using NLog;
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

        public XElement GetFeatureCollectionFromWFS(string wfsUrl, ref List<string> typeNames, List<string> gmlIds, int datasetId)
        {
            logger.Info("GetFeatureCollectionFromWFS START");
            XNamespace nsApp = Database.DatasetsData.TargetNamespace(datasetId);
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";

            XDocument wfsGetFeatureDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(nsWfs + "GetFeature", new XAttribute("version", "2.0.0"), new XAttribute("service", "WFS"), new XAttribute(XNamespace.Xmlns + "app", nsApp), new XAttribute(XNamespace.Xmlns + "wfs", nsWfs), new XAttribute(XNamespace.Xmlns + "fes", nsFes)
                )
            );

            PopulateDocumentForGetFeatureRequest(gmlIds, ref typeNames, wfsGetFeatureDocument);

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(wfsUrl) as HttpWebRequest;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "text/xml";
                StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream());
                wfsGetFeatureDocument.Save(writer);
                //wfsGetFeatureDocument.Save("C:\\temp\\gvtest_query.xml");
                logger.Info("GetFeature: " + wfsGetFeatureDocument.ToString());
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

        private void PopulateDocumentForGetFeatureRequest(List<string> gmlIds, ref List<string> typeNames, XDocument wfsGetFeatureDocument)
        {
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
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
                    filterElement.Add(new XElement("PropertyIsEqualTo", new XElement("ValueReference", "identifikasjon/Identifikasjon/lokalId"), new XElement("Literal", localId)));
                }
                else
                {
                    XElement orElement = new XElement("Or");
                    foreach (string localId in localIds)
                    {
                        orElement.Add(new XElement("PropertyIsEqualTo", new XElement("ValueReference", "identifikasjon/Identifikasjon/lokalId"), new XElement("Literal", localId)));
                    }
                    filterElement.Add(orElement);
                }

                wfsGetFeatureDocument.Element(nsWfs + "GetFeature").Add(new XElement(nsWfs + "Query", new XAttribute("typeNames", "app:" + typename), filterElement));
            }
        }
    }
}