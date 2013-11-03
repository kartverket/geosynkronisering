using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Kartverket.Geosynkronisering.Subscriber.BL.Utils
{
    public class XmlUtils
    {
        /// <summary>
        /// Show XML in WebBrowser control
        /// </summary>
        /// <param name="xmlText"></param>
        public static MemoryStream VisXML(string xmlText)
        {
            if (string.IsNullOrEmpty(xmlText)) throw new Exception("Response from server is empty!");
            // Load the xslt used by IE to render the xml
            var xTrans = new XslCompiledTransform();
            string path = System.Environment.CurrentDirectory;
            string xls = path.Substring(0, path.LastIndexOf("bin")) + "Files" + "\\defaultss.xlst";

            xTrans.Load(xls);
            // Read the xml string data into an XML reader object 
            var sr = new System.IO.StringReader(xmlText);
            XmlReader xReader = XmlReader.Create(sr);
            // Apply / transform the XML data
            var ms = new System.IO.MemoryStream();
            xTrans.Transform(xReader, null, ms);
            // Reset the position
            ms.Position = 0;
            return ms;
        }

        public static string GetRequest(string uri)
        {
            string response;
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                webRequest.Timeout = 1000 * 1000;
                webRequest.ContentType = "text/xml";
                webRequest.Method = "GET";
                webRequest.KeepAlive = true;
                var res = webRequest.GetResponse() as HttpWebResponse;
                var reader = new StreamReader(res.GetResponseStream());
                response = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            // Check if response is empty
            if (string.IsNullOrEmpty(response))
            {
                throw new WebException("Empty response", WebExceptionStatus.Success);
            }
            // Check if response is an exception
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(response);
            XmlNode root = xmldoc.DocumentElement;
            if (root.Name == "ExceptionReport")
            {
                throw new WebException(root.InnerText);
            }
            return response;
        }

        /// <summary>
        /// Read XML data from file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns file content in XML string format</returns>
        public static string GetTextFromXMLFile(string file)
        {
            var reader = new StreamReader(file);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }
    }
}
