using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using Dapper;
using Dapper.Contrib.Extensions;
using Kartverket.GeosyncWCF;


namespace Kartverket.Geosynkronisering.Subscriber.DL
{

    public class CapabilitiesDataBuilder
    {
        public CapabilitiesDataBuilder(string ProviderURL, string UserName, string Password)
        {
            geosyncDBEntities db = new geosyncDBEntities();

            WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
            client.ClientCredentials.UserName.UserName = UserName;
            client.ClientCredentials.UserName.Password = Password;

            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(ProviderURL);

            ReadGetCapabilities(db, client);
        }


        private IBindingList m_DatasetBindingList;


        private void ReadGetCapabilities(geosyncDBEntities db, WebFeatureServiceReplicationPort client)
        {
            var req = new GetCapabilitiesType1();
            var rootCapabilities = client.GetCapabilities(req);

            //Build Cababilities.XML
            //ServiceIndentification
            m_DatasetBindingList = new BindingList<Dataset>();
            foreach (var dst in rootCapabilities.datasets)
            {
                var ds = new Dataset
                {
                    ProviderDatasetId = dst.datasetId.Trim(),
                    Name = dst.name.Trim(),
                    Version = client.GetDatasetVersion(dst.datasetId).Trim()
                };

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
            foreach (ItemsChoiceType1 ict in dcp.Item.ItemsElementName)
            {
                if (ict == ItemsChoiceType1.Post) postReq = dcp.Item.Items[index];
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

    internal class geosyncDBEntities : IDisposable
    {
        public geosyncDBEntities()
        {
            SqlMapperExtensions.TableNameMapper = (type) => {
                //use exact name
                return type.Name;
            };

            // Populate Dataset-List from database
            Connection = new SqlCeConnection
            {
                ConnectionString =
                    "Data Source = C:\\git\\geosynk\\Kartverket.Geosynkronisering.Subscriber\\Kartverket.Geosynkronisering.Subscriber\\bin\\Debug\\geosyncDB.sdf"
            };

            Dataset = ReadAll<Dataset>("Dataset");

            //Dataset = SelectDatasets();

            //Connection.Open();
            //var cmd = new SqlCeCommand("SELECT * FROM Dataset") {Connection = Connection};
            //foreach (var i in cmd.ExecuteResultSet(new ResultSetOptions()))
            //{

            //}
            //cmd.ExecuteNonQuery();
            //Connection.Close();

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public List<Dataset> Dataset { get; set; }
        public static SqlCeConnection Connection { get; set; }

        public void AddObject(Dataset ds)
        {
            Dataset.Add(ds);
        }

        public void SaveChanges()
        {
            foreach (var dataset in Dataset)
            {
                if (DatasetExists(dataset.DatasetId)) UpdateDataset(dataset);
                else InsertDataset(dataset);
            }
        }

        public static List<T> ReadAll<T>(string tableName)
        {
            using (IDbConnection db = new SqlCeConnection(Connection.ConnectionString))
            {
                return db.Query<T>("SELECT * FROM " + tableName).ToList();
            }
        }

        public static bool DatasetExists(int datasetId)
        {
            using (IDbConnection db = new SqlCeConnection(Connection.ConnectionString))
            {
                return db.Query<int>($"SELECT 1 FROM Dataset WHERE DatasetId = {datasetId}").ToList().FirstOrDefault() == 1;
            }
        }

        private void InsertDataset(Dataset dataset)
        {
            Connection.Insert(dataset);
        }

        private void UpdateDataset(Dataset dataset)
        {
            Connection.Update(dataset);
        }

        public void DeleteObject(Dataset dataset)
        {
            Connection.Delete(dataset);
            Dataset.Remove(dataset);
        }
    }
}