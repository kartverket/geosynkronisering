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
using Kartverket.Geosynkronisering.Subscriber2;
using Kartverket.GeosyncWCF;
using System.ComponentModel;
using NLog;


namespace Kartverket.Geosynkronisering.Database
{    
   
  
    public class DatasetsData
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static IList<Int32> GetListOfDatasetIDs()
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                IList<Int32> idList = new List<Int32>();
                var res = from d in db.Dataset select d.DatasetId;
                foreach (Int32 id in res)
                {
                    idList.Add(id);
                }
                return idList;
            }
        }
        

        public static string Name(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.Name;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string SyncronizationUrl(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.SyncronizationUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ProviderDatasetId(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.ProviderDatasetId;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string MappingFile(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.MappingFile;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string MaxCount(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.MaxCount;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string LastIndex(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.LastIndex;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string ClientWfsUrl(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.ClientWfsUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        
        public static string TargetNamespace(Int32 DatasetID)
        {
            using (geosyncDBEntities db = new geosyncDBEntities())
            {
                var res = from d in db.Dataset where d.DatasetId == DatasetID select d.TargetNamespace;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static bool AddDatasets(geosyncDBEntities db, IBindingList DatasetBindingList, IList<int> selectedDatasets)
        {
          
            Dataset ds = null;
            foreach (int selected in selectedDatasets)
            {
                ds = (Dataset)DatasetBindingList[selected];
                try
                {                  
                    db.Dataset.AddObject(ds);
                    db.AcceptAllChanges();
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.LogException(LogLevel.Error, "Error saving selected datasets!", ex);
                    return false;
                }
            }                
                  
            return true;
        }
    }


    public class CapabilitiesDataBuilder
    {

        public CapabilitiesDataBuilder(string ProviderURL)
        {
            WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(ProviderURL);

            GetCapabilitiesType1 req = new GetCapabilitiesType1();

            REP_CapabilitiesType rootCapabilities = client.GetCapabilities(req);
            
            ReadGetCapabilities(rootCapabilities);
        }

        
        private IBindingList m_DatasetBindingList;


        private void ReadGetCapabilities(Kartverket.GeosyncWCF.REP_CapabilitiesType rootCapabilities)
        {
            //Build Cababilities.XML
            //ServiceIndentification
            Dataset ds;
            m_DatasetBindingList = new BindingList<Dataset>();
            foreach (GeosyncWCF.DatasetType dst in rootCapabilities.datasets)
            {
                ds = new Dataset();                
                ds.ProviderDatasetId = Convert.ToInt32(dst.datasetId);
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

            if (postReq != null) return postReq.href;
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