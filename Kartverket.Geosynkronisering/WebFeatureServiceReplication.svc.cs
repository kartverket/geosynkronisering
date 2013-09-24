using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Reflection;
using System.ServiceModel.Activation;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Kartverket.Geosynkronisering
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(Namespace = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/produkt")]
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WebFeatureServiceReplication" in code, svc and config file together.
    public class WebFeatureServiceReplication : Kartverket.GeosyncWCF.WebFeatureServiceReplicationPort
    {

        public Kartverket.GeosyncWCF.REP_CapabilitiesType GetCapabilities(Kartverket.GeosyncWCF.GetCapabilitiesType1 getcapabilities1)
        {

            Geosynkronisering.Database.CapabilitiesDataBuilder cdb = new Geosynkronisering.Database.CapabilitiesDataBuilder();
            GeosyncWCF.REP_CapabilitiesType cbt = cdb.GetCapabilities();

            return cbt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="describefeaturetype1"></param>
        /// <returns></returns>
        public object DescribeFeatureType(string datasetId, Kartverket.GeosyncWCF.DescribeFeatureTypeType describefeaturetype1)
        {
            int numDatasetId = 0;
            int.TryParse(datasetId, out numDatasetId);
            geosyncEntities db = new geosyncEntities();
            ChangelogManager mng = new ChangelogManager(db);
            return mng.DescribeFeatureType(numDatasetId).ToString(); //TODO Får ikke til å returnere XElement eller XmlDocument her. Virker som serialiseringen får problemer... 
        }


        public GeosyncWCF.ListStoredChangelogsResponse ListStoredChangelogs(Kartverket.GeosyncWCF.ListStoredChangelogsRequest request)
        {
            string dataset=request.datasetId;
            int datasetId = 0;
            int.TryParse(dataset, out datasetId);
            geosyncEntities db = new geosyncEntities();
            ChangelogManager mng = new ChangelogManager(db);
            return mng.ListStoredChangelogs(datasetId);//TODO mangler input datasettid
        }

        public Kartverket.GeosyncWCF.GetLastIndexResponse GetLastIndex(Kartverket.GeosyncWCF.GetLastIndexRequest request)
        {
            string dataset = request.datasetId;
            if (dataset == "") dataset = Properties.Settings.Default.defaultDataset;
            int id = 0;
            int.TryParse(dataset, out id);

            IChangelogProvider changelogprovider;
            geosyncEntities db = new geosyncEntities();
            var datasets = from d in db.Datasets where d.DatasetId == id select d;
            string initType = datasets.First().DatasetProvider;
            //Initiate provider from config/dataset
            Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
            changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
            changelogprovider.SetDb(db);
            int datasetId = Convert.ToInt32(dataset);

            var resp = changelogprovider.GetLastIndex(datasetId);

            Kartverket.GeosyncWCF.GetLastIndexResponse res = new Kartverket.GeosyncWCF.GetLastIndexResponse();
            res.@return = resp;
            return res;
        }


        public void AcknowlegeChangelogDownloaded(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogId)
        {
            int id;
            if (int.TryParse(changelogId.changelogId, out id))
            {
                geosyncEntities db = new geosyncEntities();
                ChangelogManager mng = new ChangelogManager(db);
                mng.AcknowledgeChangelogDownloaded(id);
            }
        }

        public void CancelChangelog(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            int id;
            if (int.TryParse(changelogid.changelogId, out id))
            {
                geosyncEntities db = new geosyncEntities();
                ChangelogManager mng = new ChangelogManager(db);
                mng.AcknowledgeChangelogDownloaded(id);
            }
        }

        public Kartverket.GeosyncWCF.ChangelogType GetChangelog(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            int id = -1;
            int.TryParse(changelogid.changelogId, out id);

            if (id == -1)
            {
                throw new System.Exception("MissingParameterValue : changelogid");
            }
            geosyncEntities db = new geosyncEntities();
            ChangelogManager mng = new ChangelogManager(db);
            return mng.GetChangelog(id);
        }

        public Kartverket.GeosyncWCF.ChangelogStatusType GetChangelogStatus(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            int id=-1;
            int.TryParse(changelogid.changelogId, out id);
            
            if (id == -1)
            {
                throw new System.Exception("MissingParameterValue : changelogid");
            }
            geosyncEntities db = new geosyncEntities();
            ChangelogManager mng = new ChangelogManager(db);
            return mng.GetChangelogStatus(id);
        }

        public Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelog(Kartverket.GeosyncWCF.ChangelogOrderType order)
        {
         
            int id = -1;
            int.TryParse(order.datasetId, out id);
            IChangelogProvider changelogprovider;
            geosyncEntities db = new geosyncEntities();
            var datasets = from d in db.Datasets where d.DatasetId == id select d;
            string initType = datasets.First().DatasetProvider;
            //Initiate provider from config/dataset
            Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
            changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
            changelogprovider.SetDb(db);
            int startindex = -1;
            int count = -1;

            int.TryParse(order.startIndex, out startindex);
            int.TryParse(order.count, out count);

            if (startindex == -1)
            {
                throw new System.Exception("MissingParameterValue : startindex");
            }
            if (count == -1)
            {
                throw new System.Exception("MissingParameterValue : count");
            }

            var resp = changelogprovider.OrderChangelog(startindex, count, "", id);

            Kartverket.GeosyncWCF.ChangelogIdentificationType res = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
            res.changelogId = resp.changelogId;
            return res;
            
        }


        
    }
}
