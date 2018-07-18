using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Xml;
using Kartverket.GeosyncWCF;
using Kartverket.Geosynkronisering.Database;
using Kartverket.Geosynkronisering.Properties;
using NLog;

namespace Kartverket.Geosynkronisering
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(Namespace = "http://skjema.geonorge.no/standard/geosynkronisering/1.2/produkt")]

    // NLog for logging (nuget package)
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WebFeatureServiceReplication" in code, svc and config file together.
    public class WebFeatureServiceReplication : WebFeatureServiceReplicationPort
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private geosyncEntities db;

        public WebFeatureServiceReplication()
        {
            db = new geosyncEntities();
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public REP_CapabilitiesType GetCapabilities(GetCapabilitiesType1 getcapabilities1)
        {

            CapabilitiesDataBuilder cdb = new CapabilitiesDataBuilder();
            REP_CapabilitiesType cbt = cdb.GetCapabilities();
            return cbt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="describefeaturetype1"></param>
        /// <returns></returns>
        public object DescribeFeatureType(string datasetId, DescribeFeatureTypeType describefeaturetype1)
        {
            try
            {
                int numDatasetId = 0;
                int.TryParse(datasetId, out numDatasetId);
                ChangelogManager mng = new ChangelogManager(db);
                return mng.DescribeFeatureType(numDatasetId, describefeaturetype1);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public ListStoredChangelogsResponse ListStoredChangelogs(
            ListStoredChangelogsRequest request)
        {
            try
            {
                string dataset = request.datasetId;
                int datasetId = 0;
                int.TryParse(dataset, out datasetId);
                ChangelogManager mng = new ChangelogManager(db);
                return mng.ListStoredChangelogs(datasetId); //TODO mangler input datasettid                     
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        



        public GetLastIndexResponse GetLastIndex(GetLastIndexRequest request)
        {
            try
            {
                string dataset = request.datasetId;
                if (dataset == "") throw new ArgumentException("Missing dataset in request");
                int id = 0;
                int.TryParse(dataset, out id);

                string resp;
                string initType;
                var datasets = from d in db.Datasets where d.DatasetId == id select d;
                initType = datasets.First().DatasetProvider;

                //Initiate provider from config/dataset
                Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
                IChangelogProvider changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
                changelogprovider.Intitalize(id);
                resp = changelogprovider.GetLastIndex(id);

                GetLastIndexResponse res = new GetLastIndexResponse();
                res.@return = resp;
                return res;

            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public void AcknowlegeChangelogDownloaded(ChangelogIdentificationType changelogId)
        {
            try
            {
                ChangelogManager mng = new ChangelogManager(db);
                mng.AcknowledgeChangelogDownloaded(changelogId.changelogId);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public void CancelChangelog(ChangelogIdentificationType changelogid)
        {
            try
            {
                ChangelogManager mng = new ChangelogManager(db);
                mng.AcknowledgeChangelogDownloaded(changelogid.changelogId);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public ChangelogType GetChangelog(ChangelogIdentificationType changelogid)
        {
            try
            {
                ChangelogManager mng = new ChangelogManager(db);
                return mng.GetChangelog(changelogid.changelogId);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public ChangelogStatusType GetChangelogStatus(ChangelogIdentificationType changelogid)
        {
            try
            {
                ChangelogManager mng = new ChangelogManager(db);
                return mng.GetChangelogStatus(changelogid.changelogId);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public ChangelogIdentificationType OrderChangelog(ChangelogOrderType order)
        {
            try
            {
                return OrderChangelogAsyncCaller(order);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public ChangelogIdentificationType OrderChangelog2(ChangelogOrderType order, string datasetVersion)
        {
            try
            {
                var providerVersion = GetDataset(order.datasetId).Version;

                if (providerVersion != datasetVersion)
                    throw new ArgumentException(
                        $"Wrong datasetVersion supplied. Provider: {providerVersion.Trim()}, Subscriber: {datasetVersion}");

                return OrderChangelog(order);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public string GetDatasetVersion(string datasetId)
        {
            try
            {
                return GetDataset(datasetId).Version;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        private static int GetId(string datasetId)
        {
            var dataset = datasetId;
            if (dataset == "") throw new ArgumentException("Missing datasetId in request");
            int id;
            int.TryParse(dataset, out id);
            return id;
        }

        private Dataset GetDataset(string datasetId)
        {
            var datasets = from d in db.Datasets where d.DatasetId == GetId(datasetId) select d;
            var dataset = datasets.First();
            return dataset;
        }

        public void SendReport(ReportType report)
        {
            try
            { 
            switch (report.type)
            {
                case ReportTypeEnumType.error:
                    Logger.Error(report.description);
                    break;
                case ReportTypeEnumType.info:
                    Logger.Info(report.description);
                    break;
                default:
                    Logger.Info(report.description);
                    break;
            }

            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public PrecisionType GetPrecision(string datasetId)
        {
            try
            {
                var dataset = GetDataset(datasetId);

                return new PrecisionType
                {
                    decimals = dataset.Decimals,
                    epsgCode = dataset.DefaultCrs,
                    tolerance = dataset.Tolerance
                };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        #region Async Code for OrderChangeLog

        public ChangelogIdentificationType OrderChangeLogAsync(IChangelogProvider changelogprovider,
            int startindex, int count, string to_doFilter, int datasetID)
        {
            try
            {
                var resp = changelogprovider.OrderChangelog(startindex, count, "", datasetID);
                ChangelogIdentificationType res = new ChangelogIdentificationType();
                res.changelogId = resp.changelogId;
                return res;
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.Message, ex);

                return null;
            }
        }


        private ChangelogIdentificationType OrderChangelogAsyncCaller(ChangelogOrderType order)
        {
            int id = -1;
            int.TryParse(order.datasetId, out id);
            IChangelogProvider changelogprovider;
            OrderChangelog resp;
            int startindex = -1;
            int count = -1;
            string initType;
            var datasets = from d in db.Datasets where d.DatasetId == id select d;
            initType = datasets.First().DatasetProvider;
            //Initiate provider from config/dataset
            Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
            changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
            changelogprovider.Intitalize(id);


            int.TryParse(order.startIndex, out startindex);
            int.TryParse(order.count, out count);

            if (startindex == -1)
            {
                throw new Exception("MissingParameterValue : startindex");
            }
            if (count == -1)
            {
                throw new Exception("MissingParameterValue : count");
            }

            resp = changelogprovider.CreateChangelog(startindex, count, "", id);

            // Create the delegate.
            OrderChangelogAsync caller = OrderChangeLogAsync;
            ProcessState state = new ProcessState(resp.changelogId);
            state.Request = caller;

            caller.BeginInvoke(changelogprovider, startindex, count, "", id,
                CallbackProcessStatus, state);

            ChangelogIdentificationType res = new ChangelogIdentificationType();
            res.changelogId = resp.changelogId;
            return res;


        }

        private void CallbackProcessStatus(IAsyncResult ar)
        {
            // Retrieve the delegate.
            ProcessState result = (ProcessState) ar.AsyncState;
            OrderChangelogAsync caller = result.Request;
            ChangelogIdentificationType returnResult = caller.EndInvoke(ar);
            result.Result = returnResult;
            result.Completed = true;
        }

        public class ProcessState
        {
            public ProcessState(string ID)
            {
                this.ID = ID;
            }

            public ChangelogIdentificationType Result { get; set; }

            public string ID { get; set; }

            public OrderChangelogAsync Request { get; set; }

            public bool Completed { get; set; }
        }
    }

    public delegate ChangelogIdentificationType OrderChangelogAsync(
        IChangelogProvider changelogprovider, int startindex, int count, string to_doFilter, int datasetID);

    #endregion

}
