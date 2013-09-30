﻿using System;
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
         
            return OrderChangelogSync(order);
            
        }

        private Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelogSync(Kartverket.GeosyncWCF.ChangelogOrderType order)
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


        #region Asynchron handling of OrderChangeLog
        private delegate Kartverket.GeosyncWCF.ChangelogIdentificationType OrderChangelogAsync(Kartverket.GeosyncWCF.ChangelogOrderType order);

        protected class ProcessState
        {
            private OrderChangelogAsync request;
            private bool completed = false;
            private string id;
            private string msg;
            private string tableName;
            private string IDFld;
            private string CompletedFld;
            private string MessageFld;
            private Kartverket.GeosyncWCF.ChangelogIdentificationType _Result;

            public Kartverket.GeosyncWCF.ChangelogIdentificationType Result
            {
                get { return _Result; }
                set { _Result = value; }
            }

            public ProcessState(string ID)
            {
                id = ID;
                tableName = "processtatus";
                IDFld = "processid";
                CompletedFld = "completed";
                MessageFld = "message";

            }

            public bool DeleteFromDB()
            {
                try
                {
                    //Implement db delete order.
                    return true;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Concat("Feil ved sletting av record: ", ex.Message), ex);
                    return false;
                }

            }

            public bool UpdateDB()
            {
                try
                {
                    //Implement Update status
                    return true;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Concat("Feil ved oppdatring av record: ", ex.Message), ex);
                    return false;
                }
            }

            public bool WriteToDB()
            {
                try
                {
                    //Implement write status
                    IDictionary<string, object> fields = new Dictionary<string, object>();
                    fields.Add(IDFld, id);
                    fields.Add(CompletedFld, Convert.ToInt32(completed));
                    fields.Add(MessageFld, msg);

                    return true;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Concat("Feil ved skriving til db: ", ex.Message), ex);
                    return false;
                }



            }

            public bool ReadFromDB()
            {

                try
                {
                    //Implemnet read for db

                    return true;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Concat("Feil ved lesing fra DB: ", ex.Message), ex);
                    return false;
                }

            }


            public string Message
            {
                get { return msg; }
                set { msg = value; }
            }

            public string ID
            {
                get { return id; }
                set { id = value; }
            }

            //public OrderChangelogAsync Request
            //{
            //    get { return request; }
            //    set { request = value; }
            //}

            public bool Completed
            {
                get { return completed; }
                set { completed = value; }
            }
        }

        private string OrderChangeLogAsync(Kartverket.GeosyncWCF.ChangelogOrderType order)
        {

            // Create the delegate.
            string ID = "1"; //Bestillingsnummer
            ProcessState state = new ProcessState(ID);
            state.Message = "Startet";
            state.Completed = false;

            // Create the delegate.
            OrderChangelogAsync caller = new OrderChangelogAsync(OrderChangelogSync);
           // state.Request = caller;
            IAsyncResult result = caller.BeginInvoke(order, new AsyncCallback(CallbackProcessStatus), state);
            return "#PROCESSID:" + ID;
        }

        private void CallbackProcessStatus(IAsyncResult ar)
        {
            // Retrieve the delegate.


            //ProcessState result = (ProcessState)ar.AsyncState;
            ////OrderChangelogAsync caller = result.Request;
            //Kartverket.GeosyncWCF.ChangelogIdentificationType returnResult = caller.EndInvoke(ar);
            //string msg = "";
            //result.Result = returnResult;
            //result.Completed = true;

            //if (!result.UpdateDB())
            //{
            //    logger.Error("Kunne ikke skrive status for prosessfil.");
            //    result.Message = "#ERROR: Kunne ikke skrive status for prosessfil på land";
            //}

        }

        #endregion


        
    }
}