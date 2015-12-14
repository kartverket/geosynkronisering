using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using Kartverket.GeosyncWCF;
using NLog;


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


        private void ReadGetCapabilities(geosyncDBEntities db, Kartverket.GeosyncWCF.REP_CapabilitiesType rootCapabilities)
        {
            //Build Cababilities.XML
            //ServiceIndentification
            Dataset ds;
            m_DatasetBindingList = new BindingList<Dataset>();
            foreach (GeosyncWCF.DatasetType dst in rootCapabilities.datasets)
            {
                ds = db.CreateObject<Dataset>();
                ds.EntityKey = db.CreateEntityKey("Dataset", ds);
                
                ds.ProviderDatasetId = dst.datasetId;
                ds.Name = dst.name;
                GeosyncWCF.DomainType dt = GetConstraint("CountDefault", rootCapabilities.OperationsMetadata.Constraint);       
                if (dt!=null) ds.MaxCount = Convert.ToInt32(dt.DefaultValue.Value);
                ds.TargetNamespace = dst.applicationSchema;
                GeosyncWCF.Operation op = GetOperation("OrderChangelog", rootCapabilities.OperationsMetadata.Operation);
                if (op != null)
                {
                    string PostUrl = GetPostURL(op.DCP);
                    ds.SyncronizationUrl = PostUrl;
                }
                m_DatasetBindingList.Add(ds);                

            }
          
        }

        private string GetPostURL(GeosyncWCF.DCP[] dcps)
        {
            GeosyncWCF.DCP dcp = dcps[0];
            GeosyncWCF.RequestMethodType postReq = null;
            int index = 0;
            foreach ( GeosyncWCF.ItemsChoiceType ict in dcp.Item.ItemsElementName)
            { 
                if (ict == GeosyncWCF.ItemsChoiceType.Post) postReq = dcp.Item.Items[index];
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

        private GeosyncWCF.DomainType GetConstraint(string constraintName, GeosyncWCF.DomainType[] Constraints)
        {
            int Index = 0;
            GeosyncWCF.DomainType dt = Constraints[Index];
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

        private GeosyncWCF.Operation GetOperation(string constraintName, GeosyncWCF.Operation[] Operations)
        {
            int Index = 0;
            GeosyncWCF.Operation Op = Operations[Index];
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

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }

        public IBindingList ProviderDatasets
        {
            get { return m_DatasetBindingList; }
        }

    }

}