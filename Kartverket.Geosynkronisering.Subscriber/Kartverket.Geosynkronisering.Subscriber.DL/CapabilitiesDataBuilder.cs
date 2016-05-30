using System;
using System.Linq;
using System.ComponentModel;
using Kartverket.GeosyncWCF;


namespace Kartverket.Geosynkronisering.Subscriber.DL
{

    public class CapabilitiesDataBuilder
    {
        public CapabilitiesDataBuilder(string ProviderURL)
        {
            geosyncDBEntities db = new geosyncDBEntities();

            WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(ProviderURL);

            GetCapabilitiesType1 req = new GetCapabilitiesType1();

            REP_CapabilitiesType rootCapabilities = client.GetCapabilities(req);

            ReadGetCapabilities(db, rootCapabilities);
        }


        private IBindingList m_DatasetBindingList;


        private void ReadGetCapabilities(geosyncDBEntities db, REP_CapabilitiesType rootCapabilities)
        {
            //Build Cababilities.XML
            //ServiceIndentification
            Dataset ds;
            m_DatasetBindingList = new BindingList<Dataset>();
            foreach (DatasetType dst in rootCapabilities.datasets)
            {
                ds = db.CreateObject<Dataset>();
                ds.EntityKey = db.CreateEntityKey("Dataset", ds);

                ds.ProviderDatasetId = dst.datasetId;
                ds.Name = dst.name;
                DomainType dt = GetConstraint("CountDefault", rootCapabilities.OperationsMetadata.Constraint);
                if (dt != null) ds.MaxCount = Convert.ToInt32(dt.DefaultValue.Value);
                ds.TargetNamespace = dst.applicationSchema;
                Operation op = GetOperation("OrderChangelog", rootCapabilities.OperationsMetadata.Operation);
                if (op != null)
                {
                    string PostUrl = GetPostURL(op.DCP);
                    ds.SyncronizationUrl = PostUrl;
                }
                m_DatasetBindingList.Add(ds);

            }

        }

        private string GetPostURL(DCP[] dcps)
        {
            DCP dcp = dcps[0];
            RequestMethodType postReq = null;
            int index = 0;
            foreach (ItemsChoiceType ict in dcp.Item.ItemsElementName)
            {
                if (ict == ItemsChoiceType.Post) postReq = dcp.Item.Items[index];
                index++;
            }

            if (postReq != null)
            {
                string href = postReq.href;
                if (postReq.href.EndsWith("/")) href = postReq.href.Remove(postReq.href.LastIndexOf("/"));
                return href;
            }
            return "";
        }

        private DomainType GetConstraint(string constraintName, DomainType[] Constraints)
        {
            int Index = 0;
            DomainType dt = Constraints[Index];
            while (dt.name.ToLower() != constraintName.ToLower() && Index < Constraints.Count() - 1)
            {
                Index++;
                dt = Constraints[Index];
            }
            if (dt.name.ToLower() == constraintName.ToLower())
            {
                return dt;
            }
            return null;
        }

        private Operation GetOperation(string constraintName, Operation[] Operations)
        {
            int Index = 0;
            Operation Op = Operations[Index];
            while (Op.name.ToLower() != constraintName.ToLower() && Index < Operations.Count() - 1)
            {
                Index++;
                Op = Operations[Index];
            }
            if (Op.name.ToLower() == constraintName.ToLower())
            {
                return Op;
            }
            return null;
        }

        public IBindingList ProviderDatasets
        {
            get { return m_DatasetBindingList; }
        }
    }
}