using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Activation;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Web;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections;
using Kartverket.Geosynkronisering.ChangelogProviders;
using System.Xml.Serialization;
using System.Reflection;

namespace Kartverket.Geosynkronisering
{
    
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.geosynkronisering.no", IncludeInSchema = true)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract]
    public class Syncronization
    {
        /// <summary>
        /// Proccesses request using GET method
        /// </summary>
        /// <returns>Response to the request.</returns>
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Xml)]
        [OperationContract]
        public Message GetRequest()
        {
            return ProcessRequest(null,"");
        }

        /// <summary>
        /// Proccesses request using POST method
        /// </summary>
        /// <param name="request">Message with request details</param>
        /// <returns>Response to the request.</returns>
        [WebInvoke(Method = "POST", UriTemplate = "/", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [OperationContract]
        public Message PostRequest(Message request)
        {
            return ProcessRequest(request,"");
        }

        [WebGet(UriTemplate = "/ds/{dataset}", ResponseFormat = WebMessageFormat.Xml)]
        [OperationContract]
        public Message GetRequestByDataset(string dataset)
        {
            return ProcessRequest(null, dataset);
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Message ProcessRequest(Message messageRequest, string dataset)
        {
            Message result = null;
            XmlDocument xmlDoc = null;

            try
            {

                NameValueCollection queryParameters = ParseQueryString(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.Query);
                XmlElement xmlRequest = null;

                if (messageRequest != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(messageRequest.GetReaderAtBodyContents());
                    xmlRequest = doc.DocumentElement;

                    foreach (XmlNode node in xmlRequest.ChildNodes)
                    {
                        queryParameters.Add(node.Name, node.InnerText.ToString());
                    }
                }

                //  Apply global defaults
                if (queryParameters["service"] == null)
                {
                    queryParameters.Add("service", "REP");
                }
                if (queryParameters["version"] == null)
                {
                    queryParameters.Add("version", "2.0.0");
                }

                string requestName = queryParameters["request"];
                string serviceName = queryParameters["service"];
                string requestVersion;

                //  Get version if possible
                if (xmlRequest != null)
                {
                    requestVersion = xmlRequest.GetAttribute("version");
                }
                else
                {
                    requestVersion = queryParameters["version"];
                }

                if (dataset == "")
                {
                    if (queryParameters["dataset"] == null)
                    {
                        dataset = Properties.Settings.Default.defaultDataset;
                    }
                    else 
                    {
                        dataset = queryParameters["dataset"];
                    }
                    
                }
 
                IChangelogProvider changelogprovider;
                geosyncEntities db = new geosyncEntities();
                var datasets = from d in db.Datasets where d.Name == dataset select d;
                string initType = datasets.First().DatasetProvider;

                int datasetId = Convert.ToInt32(dataset);

                //Initiate provider from config/dataset
                Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
                changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
                changelogprovider.SetDb(db);

                ChangelogManager mng = new ChangelogManager(db);

                switch (requestName)
                {
                    case "GetCapabilities":
                        xmlDoc = mng.GetCapabilities();
                        break;
                    case "GetLastIndex":
                        var resp = changelogprovider.GetLastIndex(datasetId);
                        XmlSerializer serializer = new XmlSerializer(resp.GetType());
                        StringBuilder sb = new StringBuilder();
                        StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                        serializer.Serialize(sw, resp);
                        xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(sb.ToString());
                        break;
                    case "DescribeFeatureType":
                        //xmlDoc = mng.DescribeFeatureType(datasetId);
                        break;
                    case "ListStoredChangelogs":
                        xmlDoc = ListStoredChangelogs(mng, datasetId);
                        break;
                    case "GetChangelog":
                        xmlDoc = GetChangelog(queryParameters, ref mng);
                        break;
                    case "OrderChangelog":
                        xmlDoc = OrderChangelog(queryParameters, ref changelogprovider, datasetId);
                        break;
                    case "GetChangelogStatus":
                        xmlDoc = GetChangelogStatus(queryParameters, ref mng);
                        break;
                    case "AcknowledgeChangelogDownloaded":
                        mng.AcknowledgeChangelogDownloaded(2);
                        break;
                    case "CancelChangelog":
                        mng.CancelChangelog(2);
                        break;
                }

                if (xmlDoc != null)
                {
                    result = Message.CreateMessage(MessageVersion.None, null, xmlDoc.DocumentElement);
                    result.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Xml);
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
                }

               
            }
            catch (System.Exception exp)
            {
                Kartverket.GeosyncWCF.ExceptionReport owsexp = new Kartverket.GeosyncWCF.ExceptionReport();
                owsexp.lang = "no";
                owsexp.version = "1.0.0";
                List<Kartverket.GeosyncWCF.ExceptionType> ret = new List<Kartverket.GeosyncWCF.ExceptionType>();
                Kartverket.GeosyncWCF.ExceptionType e = new Kartverket.GeosyncWCF.ExceptionType();
                e.exceptionCode = "NoApplicableCode"; // OperationNotSupported, MissingParameterValue, InvalidParameterValue, ResourceNotFound, NoApplicableCode
                e.locator = exp.Source;
                List<String> rettext = new List<String>();
                rettext.Add(exp.Message);
                rettext.Add(exp.StackTrace);
                e.ExceptionText = rettext.ToArray();
                ret.Add(e);
                owsexp.Exception = ret.ToArray();

                XmlSerializer serializer = new XmlSerializer(owsexp.GetType());
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                serializer.Serialize(sw, owsexp);
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sb.ToString());

                result = Message.CreateMessage(MessageVersion.None, null, xmlDoc.DocumentElement);
                result.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Xml);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            }

            return result;
        
        }

        private XmlDocument ListStoredChangelogs(ChangelogManager changelogprovider, int datasetId)
        {
            var resp = changelogprovider.ListStoredChangelogs(datasetId);
            XmlSerializer serializer = new XmlSerializer(resp.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            serializer.Serialize(sw, resp);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());
            return xmlDoc;
        }

        private XmlDocument GetChangelogStatus(NameValueCollection queryParameters, ref ChangelogManager changelogprovider)
        {
            int changelogid = -1;
            try
            {
                changelogid = int.Parse(queryParameters["changelogid"]);
            }
            catch (System.Exception exp)
            {
                throw new System.Exception("MissingParameterValue : changelogid", exp);
            }
            if (changelogid == -1)
            {
                throw new System.Exception("MissingParameterValue : changelogid");
            }

            var resp = changelogprovider.GetChangelogStatus(changelogid);
            
            XmlSerializer serializer = new XmlSerializer(resp.GetType());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            serializer.Serialize(sw, resp);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());

            return xmlDoc;
        }

        private XmlDocument OrderChangelog(NameValueCollection queryParameters, ref IChangelogProvider changelogprovider, int datasetId )
        {
            int startIndex = -1;
            try
            {
                startIndex = int.Parse(queryParameters["startIndex"]);
            }
            catch (System.Exception exp)
            {
                throw new System.Exception("MissingParameterValue : startIndex", exp);
            }
            if (startIndex == -1)
            {
                throw new System.Exception("MissingParameterValue : startIndex");
            }

            int count = -1;
            try
            {
                count = int.Parse(queryParameters["count"]);
            }
            catch (System.Exception exp)
            {
                throw new System.Exception("MissingParameterValue : count", exp);
            }
            if (startIndex == -1)
            {
                throw new System.Exception("MissingParameterValue : count");
            }

            String filter = "";
            filter = queryParameters["filter"];
            //TODO håndtere filter

            var resp = changelogprovider.OrderChangelog(startIndex, count, filter, datasetId);

            XmlSerializer serializer = new XmlSerializer(resp.GetType());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            serializer.Serialize(sw, resp);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());

            return xmlDoc;
        
        
        }

        private XmlDocument GetChangelog(NameValueCollection queryParameters, ref ChangelogManager changelogprovider)
        {
            int changelogid = -1;
            try
            {
                changelogid = int.Parse(queryParameters["changelogid"]);
            }
            catch (System.Exception exp)
            {
                throw new System.Exception("MissingParameterValue : changelogid",exp);
            }
            if (changelogid == -1)
            {
                throw new System.Exception("MissingParameterValue : changelogid");
            }
            var resp = changelogprovider.GetChangelog(changelogid);

            XmlSerializer serializer = new XmlSerializer(resp.GetType());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            serializer.Serialize(sw, resp);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());

            return xmlDoc;
        }


        private static NameValueCollection ParseQueryString(string qstring)
        {
            NameValueCollection outc = new NameValueCollection();

            Regex r = new Regex(@"[?]?(?<name>[^=&;]+)(=(?<value>[^&;]+))?(&|;)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            IEnumerator matches = r.Matches(qstring).GetEnumerator();
            while (matches.MoveNext() && matches.Current != null)
            {
                string name = ((Match)matches.Current).Result("${name}");
                string value = ((Match)matches.Current).Result("${value}");
                value = System.Web.HttpUtility.UrlDecode(value);

                outc.Add(name, value);
            }

            return outc;
        }

    }
}
