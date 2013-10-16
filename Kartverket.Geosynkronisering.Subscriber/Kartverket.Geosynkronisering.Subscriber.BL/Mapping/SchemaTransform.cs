using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.BL.Mapping
{
    public class SchemaTransform
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        /// <summary>
        /// Transform the changelog file to a simplified form based on GeoServer mappiing file.
        /// </summary>
        /// <param name="fileName">Name of the file to transform.</param>
        /// <returns> Name of transformed changelog</returns>
        public string SchemaTransformSimplify(string fileName, int datasetId)
        {
            string newFileName = "";
            try
            {

                // Create new stopwatch
                Stopwatch stopWatch = new Stopwatch();


                //var stopwatch = System.Diagnostics.Stopwatch.StartNew();


                // 
                string path = System.Environment.CurrentDirectory;
                string mappingFileName;
                //mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" +
                //                         @"\ar5FeatureType-mapping-file.xml";

                // Get Mappingfile and TargetNamespace from database
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
                string namespaceUri = dataset.TargetNamespace;
                mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + dataset.MappingFile; //"SchemaMapping" + @"\" + dataset.MappingFile;


                // Set up GeoServer mapping
                // TODO: GetCapabilities should deliver NamespaceUri? Once, or get every time?
                GeoserverMapping geoserverMap = new GeoserverMapping();
                //geoserverMap.NamespaceUri = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                geoserverMap.NamespaceUri = namespaceUri;
                geoserverMap.SetXmlMappingFile(mappingFileName);

                // Simplify
                XElement newChangeLog = geoserverMap.Simplify(fileName);

                if (newChangeLog != null)
                {
                    string outPath = Path.GetDirectoryName(fileName);
                    newFileName = outPath + @"\" + "New_" + Path.GetFileName(fileName);
                    newChangeLog.Save(newFileName);
                    string msg = "Source: " + fileName;
                    msg += "\r\n" + "Target: " + newFileName;
                    msg += "\r\n" + "Mappingfile: " + mappingFileName;
                    msg += "\r\n" + "Schema: " + geoserverMap.NamespaceUri;
                    logger.Info("SchemaTransform Schema transformation OK {0}", msg);
                    //MessageBox.Show("Sucsessfull schema transformation." + "\r\n" + msg, "TestSimplifyChangelog");

                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                logger.Info("SchemaTransformSimplify RunTime: {0}", elapsedTime);


            }
            catch (Exception ex)
            {
                logger.ErrorException("SetXmlMappingFile:", ex);
                throw;
            }

            return newFileName;
        }
    }
}
