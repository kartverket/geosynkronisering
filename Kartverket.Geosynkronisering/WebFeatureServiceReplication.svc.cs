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
using NLog;


namespace Kartverket.Geosynkronisering
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(Namespace = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/produkt")]
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WebFeatureServiceReplication" in code, svc and config file together.
    public class WebFeatureServiceReplication : Kartverket.GeosyncWCF.WebFeatureServiceReplicationPort
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
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
            try
            {
                int numDatasetId = 0;
                int.TryParse(datasetId, out numDatasetId);
                geosyncEntities db = new geosyncEntities();
                ChangelogManager mng = new ChangelogManager(db);
                return mng.DescribeFeatureType(numDatasetId).ToString(); //TODO Får ikke til å returnere XElement eller XmlDocument her. Virker som serialiseringen får problemer... 
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public GeosyncWCF.ListStoredChangelogsResponse ListStoredChangelogs(Kartverket.GeosyncWCF.ListStoredChangelogsRequest request)
        {
            try
            {
                string dataset = request.datasetId;
                int datasetId = 0;
                int.TryParse(dataset, out datasetId);
                geosyncEntities db = new geosyncEntities();
                ChangelogManager mng = new ChangelogManager(db);
                return mng.ListStoredChangelogs(datasetId);//TODO mangler input datasettid
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Kartverket.GeosyncWCF.GetLastIndexResponse GetLastIndex(Kartverket.GeosyncWCF.GetLastIndexRequest request)
        {
            try
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
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public void AcknowlegeChangelogDownloaded(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogId)
        {
            try
            {
                int id;
                if (int.TryParse(changelogId.changelogId, out id))
                {
                    geosyncEntities db = new geosyncEntities();
                    ChangelogManager mng = new ChangelogManager(db);
                    mng.AcknowledgeChangelogDownloaded(id);
                }
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public void CancelChangelog(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            try
            {
                int id;
                if (int.TryParse(changelogid.changelogId, out id))
                {
                    geosyncEntities db = new geosyncEntities();
                    ChangelogManager mng = new ChangelogManager(db);
                    mng.AcknowledgeChangelogDownloaded(id);
                }
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Kartverket.GeosyncWCF.ChangelogType GetChangelog(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            try
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
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Kartverket.GeosyncWCF.ChangelogStatusType GetChangelogStatus(Kartverket.GeosyncWCF.ChangelogIdentificationType changelogid)
        {
            try
            {
                int id = -1;
                int.TryParse(changelogid.changelogId, out id);

                if (id == -1)
                {
                    throw new System.Exception("MissingParameterValue : changelogid");
                }
                geosyncEntities db = new geosyncEntities();
                ChangelogManager mng = new ChangelogManager(db);
                return mng.GetChangelogStatus(id);
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelog(Kartverket.GeosyncWCF.ChangelogOrderType order)
        {
            try
            {
                return OrderChangelogAsyncCaller(order);
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }            
        }

        private Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelogSync(Kartverket.GeosyncWCF.ChangelogOrderType order)
        {
            try
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
                res.changelogId = resp.changelogId.ToString();
                return res;
            }
            catch (System.Exception ex)
            {
                throw new FaultException(ex.Message);
            }            
        }


        #region Async Code for OrderChangeLog

        public Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangeLogAsync(IChangelogProvider changelogprovider, int startindex, int count, string to_doFilter, int datasetID)
        {
            var resp = changelogprovider.OrderChangelog(startindex, count, "", datasetID);

            Kartverket.GeosyncWCF.ChangelogIdentificationType res = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
            res.changelogId = resp.changelogId.ToString();
            return res;

        }
      

        private Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelogAsyncCaller(Kartverket.GeosyncWCF.ChangelogOrderType order)
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

            var resp = changelogprovider.CreateChangelog(startindex, count, "", id);
            
            

            // Create the delegate.
            OrderChangelogAsync caller = new OrderChangelogAsync(OrderChangeLogAsync);
            ProcessState state = new ProcessState(resp.changelogId);
            state.Request = caller;
            
            IAsyncResult result = caller.BeginInvoke(changelogprovider, startindex, count, "", id, new AsyncCallback(CallbackProcessStatus), state);

            Kartverket.GeosyncWCF.ChangelogIdentificationType res = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
            res.changelogId = resp.changelogId.ToString();
            return res;
                                  
          
        }

        private void CallbackProcessStatus(IAsyncResult ar)
        {
            // Retrieve the delegate.


            ProcessState result = (ProcessState)ar.AsyncState;
            OrderChangelogAsync caller = result.Request;
            Kartverket.GeosyncWCF.ChangelogIdentificationType returnResult = caller.EndInvoke(ar);           
            result.Result = returnResult;
            result.Completed = true;           
        }




        public class ProcessState
        {
            private Kartverket.Geosynkronisering.OrderChangelogAsync m_request;
            private bool completed = false;
            private int id;

            private Kartverket.GeosyncWCF.ChangelogIdentificationType _Result;

            public ProcessState(int ID)
            {
                id = ID;
            }

            public Kartverket.GeosyncWCF.ChangelogIdentificationType Result
            {
                get { return _Result; }
                set { _Result = value; }
            }

            public int ID
            {
                get { return id; }
                set { id = value; }
            }

            public Kartverket.Geosynkronisering.OrderChangelogAsync Request
            {
                get { return m_request; }
                set { m_request = value; }
            }

            public bool Completed
            {
                get { return completed; }
                set { completed = value; }
            }
        }

          

   
        
    }
    
    public delegate Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelogAsync(IChangelogProvider changelogprovider, int startindex, int count, string to_doFilter, int datasetID);
     #endregion
     
}
