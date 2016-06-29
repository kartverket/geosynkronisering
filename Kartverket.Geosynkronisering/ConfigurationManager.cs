using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


namespace Kartverket.Geosynkronisering.Database
{    
    public class ServerConfigData 
    {      

        public static string DownloadUriBase()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.ServerConfigs select sc.FTPUrl;
                if (res.First() != null) return res.First();
                return "";
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
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string Abstract()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Abstract;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string Fees()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Fees;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static IList<string> Keywords()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Keywords;
                IList<string> kw = new List<string>();
                string kws = res.First();
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
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string ProviderName()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ProviderName;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string ProviderSite()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ProviderSite;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string IndividualName()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.IndividualName;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string Phone()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Phone;
                return res.First();
            }
        }
        public static string Fax()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Facsimile;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string Adresse()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Deliverypoint;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string City()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.City;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string PostalCode()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.PostalCode;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string Country()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Country;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string EMail()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.EMail;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string OnlineResourcesUrl()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.OnlineResourcesUrl;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string HoursOfService()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.HoursOfService;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string ContactInstructions()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ContactInstructions;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string Role()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.Role;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string ServiceUrl(bool addSlash)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ServiceURL;
                string url = res.First();
                string slash = "";
                if (!url.EndsWith("/") && addSlash) slash = "/";
                return string.Format("{0}{1}", url, slash);
            }
        }
        public static string ServiceUrlWithQuestionMark()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.ServiceURL;
                string url = res.First();
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
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string SchemaLocation()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from sc in db.Services select sc.SchemaLocation;
                if (res.First() != null) return res.First();
                return "";
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

        public static string Name(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.Name;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string SchemaFileUri(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.SchemaFileUri;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string DatasetProvider(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.DatasetProvider;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string ServerMaxCount(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.ServerMaxCount;
                if (res.First() != null) return res.First().ToString();
                return "";
            }
        }
        public static string DatasetConnection(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.DatasetConnection;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string DbSchema(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.DBSchema;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string TransformationConnection(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.TransformationConnection;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string DefaultCrs(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.DefaultCrs;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string UpperCornerCoords(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.UpperCornerCoords;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        public static string LowerCornerCoords(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.LowerCornerCoords;
                if (res.First() != null) return res.First();
                return "";
            }
        }

        public static string TargetNamespace(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.TargetNamespace;
                if (res.First() != null) return res.First();
                return "";
            }
        }
        //20131016-Leg
        public static string TargetNamespacePrefix(Int32 datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var res = from d in db.Datasets where d.DatasetId == datasetId select d.TargetNamespacePrefix;
                if (res.First() != null) return res.First();
                return "";
            }
        }
    }

    public class CapabilitiesDataBuilder
    {

        public GeosyncWCF.REP_CapabilitiesType GetCapabilities()
        {
            //Build Cababilities.XML
            //ServiceIndentification
            GeosyncWCF.REP_CapabilitiesType rootCapabilities = new GeosyncWCF.REP_CapabilitiesType();
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
            GeosyncWCF.KeywordsType keyWords = new GeosyncWCF.KeywordsType();
            //TODO: Sjekk om dette fungerer!!!
            values = ServiceData.Keywords();
            keyWords.Keyword = CreateNode(values);
            keyWords.Type = new GeosyncWCF.CodeType();
            keyWords.Type.Value = "String";
            List<GeosyncWCF.KeywordsType> lstDesc = new List<GeosyncWCF.KeywordsType>();
            lstDesc.Add(keyWords);
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
            rootCapabilities.ServiceProvider = new GeosyncWCF.ServiceProvider();
            rootCapabilities.ServiceProvider.ProviderName = ServiceData.ProviderName();

            rootCapabilities.ServiceProvider.ProviderSite = new GeosyncWCF.OnlineResourceType();
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
            List<GeosyncWCF.Operation> listLst = new List<GeosyncWCF.Operation>();
            GeosyncWCF.Operation operationNode = CreateOperation("GetCababilities", "Acceptversions", "2.0.0"); //TODO, må inn i databasen
            listLst.Add(operationNode);
            operationNode = CreateOperation("DescribeFeatureType", "", "");
            listLst.Add(operationNode);
            operationNode = CreateOperation("ListStoredChangelog", "", "");
            listLst.Add(operationNode);
            operationNode = CreateOperation("OrderChangelog", "", "");
            listLst.Add(operationNode);
            operationNode = CreateOperation("GetChangelogStatus", "", "");
            listLst.Add(operationNode);
            rootCapabilities.OperationsMetadata.Operation = listLst.ToArray();
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


        private static string[] CreateNode(string value)
        {           
            string[] strValues = new string[1];
            strValues[0] = value;
            return strValues;
          
        }

        private static GeosyncWCF.LanguageStringType[] CreateNode(IList<string> values = null)
        {
            GeosyncWCF.LanguageStringType node = new GeosyncWCF.LanguageStringType();
            node.lang = "no";
            List<GeosyncWCF.LanguageStringType> listLst = new List<GeosyncWCF.LanguageStringType>();
            if (values != null)
            {
                foreach (string value in values)
                {
                    node.Value = value;
                    listLst.Add(node);
                }
            }

           return listLst.ToArray();            
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
                List<GeosyncWCF.WGS84BoundingBoxType> lstWgs84Box = new List<GeosyncWCF.WGS84BoundingBoxType>();
                GeosyncWCF.WGS84BoundingBoxType wgs84Box = new GeosyncWCF.WGS84BoundingBoxType();
                wgs84Box.crs = string.Format("urn:ogc:def:crs:EPSG::{0}",DatasetsData.DefaultCrs(id));
                wgs84Box.LowerCorner = DatasetsData.LowerCornerCoords(id);
                wgs84Box.UpperCorner = DatasetsData.UpperCornerCoords(id);
                lstWgs84Box.Add(wgs84Box);
                featType.WGS84BoundingBox = lstWgs84Box.ToArray();                
                lstFeatTypes.Add(featType);
                dataset.featureTypes = lstFeatTypes.ToArray();
                datasets.Add(dataset);

            }
            return datasets.ToArray();
        }

        private static GeosyncWCF.DomainType CreateParameter(string parameterName, params string[] values)
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
            
            param.name = parameterName;
            
            return param;                
        }


        private static GeosyncWCF.DomainType CreateConstraints(string parameterName, string defaultValue)
        {
            GeosyncWCF.DomainType param = new GeosyncWCF.DomainType();
            List<object> paramValues = new List<object>();
            param.DefaultValue = new GeosyncWCF.ValueType();
            param.DefaultValue.Value = defaultValue;
            param.name = parameterName;            
            param.NoValues = new GeosyncWCF.NoValues();            
            return param;
        }

        private static GeosyncWCF.Operation CreateOperation(string operationName, string parameterName, params string[] parameterValues)
        {
            GeosyncWCF.Operation node = new GeosyncWCF.Operation();           
            
            node.name = operationName;
            List<GeosyncWCF.DCP> dcpList = new List<GeosyncWCF.DCP>();
            GeosyncWCF.DCP dcp = new GeosyncWCF.DCP();
            dcp.Item = new GeosyncWCF.HTTP();
            
            //GET
            GeosyncWCF.ItemsChoiceType ictGet = new GeosyncWCF.ItemsChoiceType();
            ictGet = GeosyncWCF.ItemsChoiceType.Get;           
            List<GeosyncWCF.ItemsChoiceType> listIct = new List<GeosyncWCF.ItemsChoiceType>();
            listIct.Add(ictGet);            
            List <GeosyncWCF.RequestMethodType> reqMethods = new List<GeosyncWCF.RequestMethodType>();
            GeosyncWCF.RequestMethodType reqMethod = new GeosyncWCF.RequestMethodType();
            reqMethod.href = ServiceData.ServiceUrlWithQuestionMark();
            reqMethods.Add(reqMethod);
            

            //POST
            GeosyncWCF.ItemsChoiceType ictPost = new GeosyncWCF.ItemsChoiceType();
            ictPost = new GeosyncWCF.ItemsChoiceType();
            ictPost = GeosyncWCF.ItemsChoiceType.Post;            
            listIct.Add(ictPost);                       
            reqMethod = new GeosyncWCF.RequestMethodType();
            reqMethod.href = ServiceData.ServiceUrl(true);
            reqMethods.Add(reqMethod);
            dcp.Item.Items =  reqMethods.ToArray();
            dcp.Item.ItemsElementName = listIct.ToArray();
            dcpList.Add(dcp);
            node.DCP = dcpList.ToArray();            
            if (parameterName != String.Empty)
            {
                List<GeosyncWCF.DomainType> lstDomains = new List<GeosyncWCF.DomainType>();
                lstDomains.Add(CreateParameter(parameterName, parameterValues));
                node.Parameter = lstDomains.ToArray();
            }
            return node;

        }
    }
}