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


namespace Kartverket.Geosynkronisering.Database
{    
    public class ServerConfigData 
    {      

        public static string downloadUriBase()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.ServerConfigs select sc.FTPUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }        
        }

    }

    public class ServiceData
    {
        
        public static string Title()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Title;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string Abstract()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Abstract;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string Fees()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Fees;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static IList<string> Keywords()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Keywords;
                IList<string> kw = new List<string>();
                string kws = res.First().ToString();
                string[] kwords = kws.Split(',');
                foreach (string s in kwords) { kw.Add(s); }
                return kw;
            }
        }
        public static string AccessConstraints()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.AccessConstraints;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string ProviderName()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ProviderName;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string ProviderSite()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ProviderSite;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string IndividualName()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.IndividualName;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string Phone()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Phone;
                return res.First().ToString();
            }
        }
        public static string Fax()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Facsimile;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string Adresse()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Deliverypoint;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string City()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.City;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string PostalCode()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.PostalCode;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string Country()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Country;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string EMail()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.EMail;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string OnlineResourcesUrl()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.OnlineResourcesUrl;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string HoursOfService()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.HoursOfService;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string ContactInstructions()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ContactInstructions;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string Role()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Role;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string ServiceURL(bool addSlash)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ServiceURL;
                string url = res.First().ToString();
                string slash = "";
                if (!url.EndsWith("/") && addSlash) slash = "/";
                return string.Format("{0}{1}", url, slash);
            }
        }
        public static string ServiceURLWithQuestionMark()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ServiceURL;
                string url = res.First().ToString();
                string slash = "";
                if (!url.EndsWith("/")) slash = "/";
                return string.Format("{0}{1}?", url, slash);
            }
        }

        public static string Namespace()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Namespace;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string SchemaLocation()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.SchemaLocation;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
    }

    public class DatasetsData
    {
        
        public static IList<Int32> GetListOfDatasetIDs()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                IList<Int32> idList = new List<Int32>();
                var res = from d in db.Datasets select d.DatasetId;
                foreach (Int32 id in res)
                {
                    idList.Add(id);
                }
                return idList;
            }
        }

        public static string Name(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.Name;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string SchemaFileUri(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.SchemaFileUri;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string DatasetProvider(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.DatasetProvider;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string ServerMaxCount(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.ServerMaxCount;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string DatasetConnection(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.DatasetConnection;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string DBSchema(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.DBSchema;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string TransformationConnection(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.TransformationConnection;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string DefaultCrs(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.DefaultCrs;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string UpperCornerCoords(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.UpperCornerCoords;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string LowerCornerCoords(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.LowerCornerCoords;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string TargetNamespace(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.TargetNamespace;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        //20131016-Leg
        public static string TargetNamespacePrefix(Int32 DatasetID)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == DatasetID select d.TargetNamespacePrefix;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
    }

    public class StoredChangeLogIDData
    {

       
        public static string Name(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.Name;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string OrderUri(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.OrderUri;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string StartIndex(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.StartIndex;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string DownloadUri(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.DownloadUri;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string EndIndex(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.EndIndex;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }
        public static string Status(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.Status;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        }

        public static string Stored(string ChangelogID)
        {
            int nchangelogid = Int32.Parse(ChangelogID);
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from cl in db.StoredChangelogs where cl.ChangelogId == nchangelogid select cl.Stored;
                if (res.First() != null) return res.First().ToString(); else return "";
            }
        } 

    }

    public class CapabilitiesDataBuilder
    {

        public Kartverket.GeosyncWCF.REP_CapabilitiesType GetCapabilities()
        {
            //Build Cababilities.XML
            //ServiceIndentification
            Kartverket.GeosyncWCF.REP_CapabilitiesType rootCapabilities = new Kartverket.GeosyncWCF.REP_CapabilitiesType();
            rootCapabilities.version = "2.0.0";

            rootCapabilities.ServiceIdentification = new GeosyncWCF.ServiceIdentification();
            //Title                        
            IList<string> values = new List<string>();
            values.Add(ServiceData.Title());         
            rootCapabilities.ServiceIdentification.Title = CreateNode(values);

            values = new List<string>();
            values.Add(ServiceData.Abstract());
            //Abstract            
            rootCapabilities.ServiceIdentification.Abstract = CreateNode(values);
            
            //Keywords
            Kartverket.GeosyncWCF.KeywordsType KeyWords = new GeosyncWCF.KeywordsType();
            //TODO: Sjekk om dette fungerer!!!
            values = ServiceData.Keywords();
            KeyWords.Keyword = CreateNode(values);
            KeyWords.Type = new GeosyncWCF.CodeType();
            KeyWords.Type.Value = "String";
            List<GeosyncWCF.KeywordsType> lstDesc = new List<GeosyncWCF.KeywordsType>();
            lstDesc.Add(KeyWords);
            rootCapabilities.ServiceIdentification.Keywords = lstDesc.ToArray();


       

            //<ows:ServiceType>WFS</ows:ServiceType>
            
            //TODO: Legg til felt i databasen            
            rootCapabilities.ServiceIdentification.ServiceType = new GeosyncWCF.CodeType();
            rootCapabilities.ServiceIdentification.ServiceType.Value = "WFS";
            
            //<ows:ServiceTypeVersion>2.0.0</ows:ServiceTypeVersion><ows:ServiceTypeVersion>1.1.0</ows:ServiceTypeVersion><ows:ServiceTypeVersion>1.0.0</ows:ServiceTypeVersion>
            //TODO Legg til felt i basen.
            rootCapabilities.ServiceIdentification.ServiceTypeVersion = new string[3]; 
            rootCapabilities.ServiceIdentification.ServiceTypeVersion[0] = "2.0.0";
            rootCapabilities.ServiceIdentification.ServiceTypeVersion[1] = "1.1.0";
            rootCapabilities.ServiceIdentification.ServiceTypeVersion[2] = "1.0.0";

           
            rootCapabilities.ServiceIdentification.Fees = ServiceData.Fees();
                        rootCapabilities.ServiceIdentification.AccessConstraints = CreateNode(ServiceData.AccessConstraints());

            ////ServiceProvider
            rootCapabilities.ServiceProvider = new Kartverket.GeosyncWCF.ServiceProvider();
            rootCapabilities.ServiceProvider.ProviderName = ServiceData.ProviderName();

            rootCapabilities.ServiceProvider.ProviderSite = new Kartverket.GeosyncWCF.OnlineResourceType();
            rootCapabilities.ServiceProvider.ProviderSite.href = ServiceData.ProviderSite();

            rootCapabilities.ServiceProvider.ServiceContact = new GeosyncWCF.ResponsiblePartySubsetType();
            rootCapabilities.ServiceProvider.ServiceContact.IndividualName = ServiceData.IndividualName();
            rootCapabilities.ServiceProvider.ServiceContact.PositionName = ""; //TODO: Lgg i basen. Innstilling for dette

            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo = new GeosyncWCF.ContactType();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Phone = new GeosyncWCF.TelephoneType();
           
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Phone.Voice = CreateNode(ServiceData.Phone());           
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Phone.Facsimile = CreateNode(ServiceData.Fax());
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address = new GeosyncWCF.AddressType();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address.DeliveryPoint = CreateNode(ServiceData.Adresse());
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address.City = ServiceData.City();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address.AdministrativeArea = ServiceData.City();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address.Country = ServiceData.Country();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.Address.ElectronicMailAddress = CreateNode(ServiceData.EMail());
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.OnlineResource = new GeosyncWCF.OnlineResourceType();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.OnlineResource.href = ServiceData.OnlineResourcesUrl();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.HoursOfService = ServiceData.HoursOfService();
            rootCapabilities.ServiceProvider.ServiceContact.ContactInfo.ContactInstructions = ServiceData.ContactInstructions();
            rootCapabilities.ServiceProvider.ServiceContact.Role = new GeosyncWCF.CodeType();
            rootCapabilities.ServiceProvider.ServiceContact.Role.Value = ServiceData.Role();

            rootCapabilities.OperationsMetadata = new GeosyncWCF.OperationsMetadata();
            List<Kartverket.GeosyncWCF.Operation> listLST = new List<Kartverket.GeosyncWCF.Operation>();
            Kartverket.GeosyncWCF.Operation operationNode = CreateOperation("GetCababilities", "Acceptversions", "2.0.0"); //TODO, må inn i databasen
            listLST.Add(operationNode);
            operationNode = CreateOperation("DescribeFeatureType", "", "");
            listLST.Add(operationNode);
            operationNode = CreateOperation("ListStoredChangelog", "", "");
            listLST.Add(operationNode);
            operationNode = CreateOperation("OrderChangelog", "", "");
            listLST.Add(operationNode);
            operationNode = CreateOperation("GetChangelogStatus", "", "");
            listLST.Add(operationNode);
            rootCapabilities.OperationsMetadata.Operation = listLST.ToArray();
            List<GeosyncWCF.DomainType> lstDomain = new List<GeosyncWCF.DomainType>();
            lstDomain.Add(CreateParameter("version", "2.0.0"));
            rootCapabilities.OperationsMetadata.Parameter = lstDomain.ToArray();
            List<GeosyncWCF.DomainType> lstConstraints = new List<GeosyncWCF.DomainType>();
            lstConstraints.Add(CreateConstraints("ImplementsReplicationWFS", "TRUE"));
            lstConstraints.Add(CreateConstraints("ImplementsTransactionalWFS", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsLockingWFS", "FALSE"));
            lstConstraints.Add(CreateConstraints("KVPEncoding", "TRUE"));
            lstConstraints.Add(CreateConstraints("XMLEncoding", "FALSE"));
            lstConstraints.Add(CreateConstraints("SOAPEncoding", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsInheritance", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsRemoteResolve", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsResultPaging", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsStandardJoins", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsSpatialJoins", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsTemporalJoins", "FALSE"));
            lstConstraints.Add(CreateConstraints("ImplementsFeatureVersioning", "FALSE"));
            lstConstraints.Add(CreateConstraints("ManageStoredQueries", "FALSE"));
            lstConstraints.Add(CreateConstraints("CountDefault", "1000")); //TODO: Legge denne verdie for servicen.
            lstConstraints.Add(CreateParameter("QueryExpressions", "wfs:StoredQuery"));
            rootCapabilities.OperationsMetadata.Constraint = lstConstraints.ToArray();


            rootCapabilities.datasets = CreateDatasets();
            
            return rootCapabilities;
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

        private static string[] CreateNode(string value)
        {           
            string[] strValues = new string[1];
            strValues[0] = value;
            return strValues;
          
        }

        private static Kartverket.GeosyncWCF.LanguageStringType[] CreateNode(IList<string> values = null)
        {
            Kartverket.GeosyncWCF.LanguageStringType node = new Kartverket.GeosyncWCF.LanguageStringType();
            node.lang = "no";
            List<Kartverket.GeosyncWCF.LanguageStringType> listLST = new List<Kartverket.GeosyncWCF.LanguageStringType>();
            if (values != null)
            {
                foreach (string value in values)
                {
                    node.Value = value;
                    listLST.Add(node);
                }
            }

           return listLST.ToArray();            
        }

        private static GeosyncWCF.DatasetType[] CreateDatasets()
        {
            List<GeosyncWCF.DatasetType> datasets = new List<GeosyncWCF.DatasetType>();
            GeosyncWCF.DatasetType dataset = new GeosyncWCF.DatasetType();
            foreach (int id in DatasetsData.GetListOfDatasetIDs())
            {
                dataset = new GeosyncWCF.DatasetType();
                dataset.applicationSchema = DatasetsData.TargetNamespace(id);
                dataset.datasetId = id.ToString();
                dataset.name = DatasetsData.Name(id);
                List<GeosyncWCF.FeatureTypeType> lstFeatTypes = new List<GeosyncWCF.FeatureTypeType>();
                GeosyncWCF.FeatureTypeType featType = new GeosyncWCF.FeatureTypeType();
                featType.Name = new XmlQualifiedName(DatasetsData.Name(id), DatasetsData.TargetNamespace(id));
                List<GeosyncWCF.MetadataURLType> lstMetaDataUrl = new List<GeosyncWCF.MetadataURLType>();
                GeosyncWCF.MetadataURLType metadataUrl = new GeosyncWCF.MetadataURLType();
                metadataUrl.href = DatasetsData.TargetNamespace(id);
                lstMetaDataUrl.Add(metadataUrl);
                featType.MetadataURL = lstMetaDataUrl.ToArray();
                List<GeosyncWCF.Title> lstTitles = new List<GeosyncWCF.Title>();
                GeosyncWCF.Title title = new GeosyncWCF.Title();
                title.lang="no";
                title.Value=DatasetsData.Name(id);                
                lstTitles.Add(title);
                featType.Title = lstTitles.ToArray();                
                featType.OutputFormats = new GeosyncWCF.OutputFormatListType();
                List<string> formats = new List<string>();
                formats.Add("text/xml; subtype=gml/3.2.1"); //DB?
                featType.OutputFormats.Format = formats.ToArray();
                List<GeosyncWCF.WGS84BoundingBoxType> lstWGS84Box = new List<GeosyncWCF.WGS84BoundingBoxType>();
                GeosyncWCF.WGS84BoundingBoxType wgs84Box = new GeosyncWCF.WGS84BoundingBoxType();
                wgs84Box.crs = string.Format("urn:ogc:def:crs:EPSG::{0}",DatasetsData.DefaultCrs(id));
                wgs84Box.LowerCorner = DatasetsData.LowerCornerCoords(id);
                wgs84Box.UpperCorner = DatasetsData.UpperCornerCoords(id);
                lstWGS84Box.Add(wgs84Box);
                featType.WGS84BoundingBox = lstWGS84Box.ToArray();                
                lstFeatTypes.Add(featType);
                dataset.featureTypes = lstFeatTypes.ToArray();
                datasets.Add(dataset);

            }
            return datasets.ToArray();
        }

        private static GeosyncWCF.DomainType CreateParameter(string ParameterName, params string[] values)
        {
            GeosyncWCF.DomainType param = new GeosyncWCF.DomainType();
            List<GeosyncWCF.ValueType> valuelist = new List<GeosyncWCF.ValueType>();
                        
            foreach (string s in values)
            {
                GeosyncWCF.ValueType val = new GeosyncWCF.ValueType();
                val.Value = s;                
                valuelist.Add(val);
            }
            param.AllowedValues = valuelist.ToArray();
            
            param.name = ParameterName;
            
            return param;                
        }


        private static GeosyncWCF.DomainType CreateConstraints(string ParameterName, string DefaultValue)
        {
            GeosyncWCF.DomainType param = new GeosyncWCF.DomainType();
            List<object> paramValues = new List<object>();
            param.DefaultValue = new GeosyncWCF.ValueType();
            param.DefaultValue.Value = DefaultValue;
            param.name = ParameterName;            
            param.NoValues = new GeosyncWCF.NoValues();            
            return param;
        }

        private static Kartverket.GeosyncWCF.Operation CreateOperation(string OperationName, string ParameterName, params string[] parameterValues)
        {
            Kartverket.GeosyncWCF.Operation node = new Kartverket.GeosyncWCF.Operation();           
            
            node.name = OperationName;
            List<GeosyncWCF.DCP> dcpList = new List<GeosyncWCF.DCP>();
            GeosyncWCF.DCP dcp = new GeosyncWCF.DCP();
            dcp.Item = new GeosyncWCF.HTTP();
            
            //GET
            GeosyncWCF.ItemsChoiceType ictGet = new GeosyncWCF.ItemsChoiceType();
            ictGet = GeosyncWCF.ItemsChoiceType.Get;           
            List<GeosyncWCF.ItemsChoiceType> listICT = new List<GeosyncWCF.ItemsChoiceType>();
            listICT.Add(ictGet);            
            List <GeosyncWCF.RequestMethodType> reqMethods = new List<GeosyncWCF.RequestMethodType>();
            GeosyncWCF.RequestMethodType reqMethod = new GeosyncWCF.RequestMethodType();
            reqMethod.href = ServiceData.ServiceURLWithQuestionMark();
            reqMethods.Add(reqMethod);
            

            //POST
            GeosyncWCF.ItemsChoiceType ictPost = new GeosyncWCF.ItemsChoiceType();
            ictPost = new GeosyncWCF.ItemsChoiceType();
            ictPost = GeosyncWCF.ItemsChoiceType.Post;            
            listICT.Add(ictPost);                       
            reqMethod = new GeosyncWCF.RequestMethodType();
            reqMethod.href = ServiceData.ServiceURL(true);
            reqMethods.Add(reqMethod);
            dcp.Item.Items =  reqMethods.ToArray();
            dcp.Item.ItemsElementName = listICT.ToArray();
            dcpList.Add(dcp);
            node.DCP = dcpList.ToArray();            
            if (ParameterName != String.Empty)
            {
                List<GeosyncWCF.DomainType> lstDomains = new List<GeosyncWCF.DomainType>();
                lstDomains.Add(CreateParameter(ParameterName, parameterValues));
                node.Parameter = lstDomains.ToArray();
            }
            return node;

        }

       
        private static System.Xml.XmlDocument OpenGetCapabilities()
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            string filePath;
            filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\GetCapabilitiesTemplate.xml", Utils.App_DataPath);
            xmlDoc.Load(filePath);
            return xmlDoc;
        }

        private static bool SaveGetCapabilities(XmlDocument xmlDoc)
        {
            string filePath;
            filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\GetCapabilities.xml", Utils.App_DataPath);
            xmlDoc.Save(filePath);
            return true;
        }


        public static bool UpdateGetCapabilitiesXML()
        {
            try
            {
                XmlDocument xmlDoc = OpenGetCapabilities();
                AddDataset(xmlDoc);
                SaveGetCapabilities(xmlDoc);
            }
            catch (Exception ex)
            {
                
                return false;
            }

            return true;
        }

        
        private static bool AddDataset(XmlDocument xmlDoc)
        {
            deleteNodes(xmlDoc, "FeatureTypeList/FeatureType");
            IList<Int32> datasetIds = Database.DatasetsData.GetListOfDatasetIDs();
            
            foreach(Int32 datasetID in datasetIds)
            {
                //Create Node
                XmlNode Dataset = xmlDoc.CreateElement("FeatureType");
                                
                //Create Child Name
                // <Name xmlns:app="http://schemas.geonorge.no/prodspek/FellesKystkontur/1.0">app:Flytebrygge</Name> 
                XmlNode Name = addChild(xmlDoc,"Name", string.Format("app:{0}", Database.DatasetsData.Name(datasetID)), "xmlns:app",Database.DatasetsData.TargetNamespace(datasetID));                   
                Dataset.AppendChild(Name);

                //Create Child Title
                // <Title>app:Flytebrygge</Title> 
                XmlNode Title = addChild(xmlDoc,"Title", string.Format("app:{0}", Database.DatasetsData.Name(datasetID)));                 
                Dataset.AppendChild(Title);

                //Create Child DefaultCRS
                //<DefaultCRS>urn:ogc:def:crs:EPSG::4258</DefaultCRS>
                XmlNode DefaultCRS = addChild(xmlDoc,"DefaultCRS", string.Format("urn:ogc:def:crs:EPSG::{0}", Database.DatasetsData.DefaultCrs(datasetID)));                 
                Dataset.AppendChild(DefaultCRS);

               //<OtherCRS>urn:ogc:def:crs:EPSG::3044</OtherCRS>
                 XmlNode OtherCRS = addChild(xmlDoc,"OtherCRS", string.Format("urn:ogc:def:crs:EPSG::{0}", "3044"));                 
                Dataset.AppendChild(OtherCRS);

                //<OutputFormats>
                // <Format>text/xml; subtype=gml/3.2.1</Format> 
                // </OutputFormats>
                XmlNode OutputFormats = addChild(xmlDoc,"OutputFormats");
                XmlNode Format = addChild(xmlDoc,"Format", "text/xml; subtype=gml/3.2.1");
                OutputFormats.AppendChild(Format);
                Dataset.AppendChild(OutputFormats);

              //  <ows:WGS84BoundingBox>
             // <ows:LowerCorner>-0.462485 57.798876</ows:LowerCorner> 
             // <ows:UpperCorner>31.445907 71.459520</ows:UpperCorner> 
             // </ows:WGS84BoundingBox>
                XmlNode WGS84BoundingBox = addChild(xmlDoc, "ows:WGS84BoundingBox");
                XmlNode LowerCorner = addChild(xmlDoc, "ows:LowerCorner", Database.DatasetsData.LowerCornerCoords(datasetID));
                WGS84BoundingBox.AppendChild(LowerCorner);
                 XmlNode UpperCorner = addChild(xmlDoc,"ows:UpperCorner", Database.DatasetsData.UpperCornerCoords(datasetID));
                WGS84BoundingBox.AppendChild(UpperCorner);
                Dataset.AppendChild(WGS84BoundingBox);


                xmlDoc.DocumentElement.AppendChild(Dataset);
            }
            return true;


        }

        private static bool UpdateServiceIdentification(XmlDocument xmlDoc)
        {
            
          //<ows:ServiceIdentification>
            string ServiceIDPath = "ows:ServiceIdentification";
          //   <ows:Title>Kartverket WFS</ows:Title>
            
            UpdateNode(xmlDoc, string.Format("{0}/ows:Title", ServiceIDPath), Database.ServiceData.Title());
         //   <ows:Abstract>Replication Web Feature Service maintained by Kartverket</ows:Abstract>
            UpdateNode(xmlDoc, string.Format("{0}/ows:Abstract", ServiceIDPath), Database.ServiceData.Abstract());
            deleteNodes(xmlDoc, "Keywords/Keyword");
   //   <ows:Keywords>
   //      <ows:Keyword>Geosynkronisering</ows:Keyword>
   //      <ows:Keyword>Kystkontur</ows:Keyword>
   //      <ows:Keyword>Kartverket</ows:Keyword>
   //      <ows:Type>String</ows:Type>
   //   </ows:Keywords>
   //   <ows:ServiceType>WFS</ows:ServiceType>
   //   <ows:ServiceTypeVersion>2.0.0</ows:ServiceTypeVersion>
   //   <ows:ServiceTypeVersion>1.1.0</ows:ServiceTypeVersion>
   //   <ows:ServiceTypeVersion>1.0.0</ows:ServiceTypeVersion>
   //   <ows:Fees>NONE</ows:Fees>
   //   <ows:AccessConstraints>NONE</ows:AccessConstraints>
   //</ows:ServiceIdentification>
            
            return true;
        }

        private static XmlNode UpdateNode(XmlDocument xmlDoc, string nodePathName, string value = null, string attributeName = null, string attributeValue = "")
        {
            XmlNode node = xmlDoc.SelectSingleNode(nodePathName);
            if (attributeName != null)
            {
               XmlAttribute Attribute = node.Attributes[attributeName];

               // check if that attribute even exists...
               if (Attribute != null)
               {

                   Attribute.Value = attributeValue;
               }
            }
            if (value != null) node.InnerText = value;
            return node;
        }
      
        private static XmlNode addChild(XmlDocument xmlDoc, string elementName, string value = null, string attributeName = null, string attributeValue = "")
        {
            XmlNode node = xmlDoc.CreateElement(elementName);
            if (attributeName != null)
            {
                XmlAttribute attribute = xmlDoc.CreateAttribute(attributeName);
                attribute.Value = attributeValue;
                node.Attributes.Append(attribute);
            }
            if (value != null) node.InnerText = value;
            return node;
        }
      

        private static void deleteNodes(XmlDocument xmlDoc, string NodePath)
        {
            foreach (XmlNode node in xmlDoc.SelectNodes(NodePath)) {node.ParentNode.RemoveChild(node);}           
        }
    }
}