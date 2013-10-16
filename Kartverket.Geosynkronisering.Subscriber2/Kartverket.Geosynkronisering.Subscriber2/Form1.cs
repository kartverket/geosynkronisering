using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using NLog;
using Ionic.Zip;
using System.Threading;
using SaveOptions = System.Xml.Linq.SaveOptions;
using Kartverket.GeosyncWCF;
using Kartverket.Geosynkronisering.Database;

namespace Kartverket.Geosynkronisering.Subscriber2
{
    public partial class Form1 : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        private int _lastIndexProvider = 0;

        private string _downLoadedChangelogName = "";

        private geosyncDBEntities _localDb;

        public Form1()
        {
            InitializeComponent();

        }

        #region Form events

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
#region GetCapabilities - Hent datasett fra tilbyder.
                txbUser.Cue = "Type username.";
                txbPassword.Cue = "Type password.";
#endregion
               

                // txtDataset is now updated by the cboDatasetName combobox
                txtDataset.Visible = false;
                txtDataset.Text = Properties.Settings.Default.defaultDataset;
                toolStripStatusLabel1.Text = "Ready";

                // TODO: remember to geosyncDB.sdf to the build folder e.g. bin\debug
                // Consider changing geosyncDBEntities(geosyncDB.sdf) database folder. 
                // today the geosyncDB.sdf must be copied to the build folder e.g. bin\debug
                //// 20130528-Leg
                //// Set DataDirectory to match our choice (root folder) to prevent using output directory for build e.g. bin\debug\ folder.
                //// Must be set before the first use of the database.
                //string referencePath = AppDomain.CurrentDomain.GetData("APPBASE").ToString();
                //string relativePath = @"..\..\";
                //string dataDict = System.IO.Path.GetFullPath(System.IO.Path.Combine(referencePath, relativePath));
                //AppDomain.CurrentDomain.SetData("DataDirectory", dataDict);

                geosyncDBEntities localDb = new geosyncDBEntities();

                try
                {
                    dgDataset.DataSource = localDb.Dataset;

                    _localDb = localDb; // We must keep this to be able to update the settings database

                }
                catch (Exception ex)
                {
                    string errMsg = "Form1_Load failed when opening database:" + localDb.Connection.DataSource;

                    logger.ErrorException(errMsg, ex);
                    errMsg += "\r\n" + "Remeber to copy the databse to the the folder:" + AppDomain.CurrentDomain.GetData("APPBASE").ToString();
                    MessageBox.Show(ex.Message + "\r\n" + errMsg);
                    //throw;
                    return;
                }

                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();

                //20121122-Leg: get lastindex from db
                var dataset = (from d in localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                txbSubscrLastindex.Text = dataset.LastIndex.ToString();

                //cboDatasetName.SelectedIndex = cboDatasetName.Items.IndexOf(dataset.Name);
                //txbSubscrLastindex.Text = Properties.Settings.Default.lastChangeIndex.ToString();

                // nlog start
                logger.Info("===== Kartverket.Geosynkronisering.Subscriber2 Start =====");
                listBoxLog.Items.Clear();

                //webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);
                string path = System.Environment.CurrentDirectory;
                string fileName = path.Substring(0, path.LastIndexOf("bin")) + "Images" + "\\Geosynk.ico";
                //webBrowser1.Refresh();
                webBrowser1.Navigate(fileName);
                //webBrowser1.Navigate(new Uri(fileName));

            }
            catch (Exception ex)
            {
                logger.ErrorException("Form1_Load failed:", ex);
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Fills the cboDatasetName comboBox with the dataset names
        /// </summary>
        private void FillComboBoxDatasetName()
        {
            cboDatasetName.Items.Clear();
            var datasetNameList = (from d in _localDb.Dataset select d.Name).ToList();
            if (datasetNameList.Count > 0)
            {
                foreach (string name in datasetNameList)
                {
                    cboDatasetName.Items.Add(name);
                }
                if (txtDataset.Text.Length > 0)
                {
                    if (cboDatasetName.Items.Contains(txtDataset.Text))
                    {
                        cboDatasetName.SelectedIndex = cboDatasetName.Items.IndexOf(txtDataset.Text);
                    }
                }
                else
                {
                    cboDatasetName.SelectedIndex = -1;
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboDatasetName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cboDatasetName_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtDataset.Text = cboDatasetName.SelectedItem.ToString();
            Properties.Settings.Default.defaultDataset = txtDataset.Text;
            Properties.Settings.Default.Save();
            if (_localDb != null)
            {
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                txbSubscrLastindex.Text = dataset.LastIndex.ToString();
            }

        }

        //private void webBrowser1_Navigating(object sender,
        //                                    WebBrowserNavigatingEventArgs e)

        //{
        //    e.Cancel = false;
        //}

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("===== Kartverket.Geosynkronisering.Subscriber2 End =====");
        }

        /// <summary>
        /// Save settings database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {

                // Does not work on SQL Server Compact 3.5, must upgrade to 4.0.
                _localDb.SaveChanges();

                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Occurs after a data-binding operation has finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDataset_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Set the primary ket field to ReadOnly
            dgDataset.Columns["DatasetId"].ReadOnly = true;

            //dgDataset.Columns.Remove("DatasetId");
            //dgDataset.Columns["DatasetId"].Visible = false;            
        }


        // Reset subsrcriber lastChangeIndex
        private void btnResetSubscrLastindex_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to reset the subscriber lastIdex?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                //20121122-Leg: update db with  lastindex
                //geosyncDBEntities localDb = new geosyncDBEntities();
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                dataset.LastIndex = 0;
                _localDb.SaveChanges();
                txbSubscrLastindex.Text = dataset.LastIndex.ToString();

                // Update datagridview control with changes
                dgDataset.DataSource = _localDb.Dataset;

                //Properties.Settings.Default.lastChangeIndex = 0;
                //Properties.Settings.Default.Save();
                //txbSubscrLastindex.Text = Properties.Settings.Default.lastChangeIndex.ToString();
            }
        }

        private void btnDescribeFeaturetype_Click(object sender, EventArgs e)
        {
            try
            {
                var response = DescribeFeaturetype(); // 20130529-Leg: refactored
                VisXML(response);
            }
            catch (WebException webEx)
            {
                MessageBox.Show("Error : " + webEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void btnListStoredChangelogs_Click(object sender, EventArgs e)
        {
            try
            {
                var response = ListStoredChangelogs(); // 20130529-Leg: refactored
                VisXML(response);
            }
            catch (WebException webEx)
            {
                MessageBox.Show("Error : " + webEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void btnGetLastIndex_Click(object sender, EventArgs e)
        {
            try
            {
                string lastIndex;
                var response = GetLastIndexFromProvider(out lastIndex);

                txbLastIndex.Text = lastIndex;
                _lastIndexProvider = Convert.ToInt32(lastIndex);

                VisXML(response);
            }
            catch (WebException webEx)
            {
                MessageBox.Show("Error : " + webEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void btnOrderChangelog_Click(object sender, EventArgs e)
        {
            int lastIndex;
            if (txbLastIndex.Text == "")
            {

                lastIndex = 0;
            }
            else
            {
                lastIndex = Convert.ToInt32(txbLastIndex.Text);
            }
            int startIndex = lastIndex + 1;



            startIndex = 0;
            //startIndex = _lastIndex + 1;

            // 20121019-Leg: calc startIndex
            //20121122-Leg: get lastindex from db
            //geosyncDBEntities localDb = new geosyncDBEntities();
            var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
            int lastChangeIndexSubscriber = (int)dataset.LastIndex;
            // int lastChangeIndexSubscriber = Properties.Settings.Default.lastChangeIndex;
            if (lastChangeIndexSubscriber < _lastIndexProvider)
            {
                // calc startIndex
                startIndex = lastChangeIndexSubscriber + 1;
                System.Diagnostics.Debug.WriteLine("startIndex:" + startIndex.ToString() + " lastChangeIndex:" + lastChangeIndexSubscriber.ToString());

                bool useGet = rbGET.Checked;
                string schangelogid;
                try
                {
                    var response = GetOrderChangelogResponse(useGet, startIndex, out schangelogid);
                    if (response != null)
                    {
                        VisXML(response);
                        txbChangelogid.Text = schangelogid;
                    }
                }

                catch (WebException webEx)
                {
                    MessageBox.Show("Error : " + webEx.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Changelog has already been downloaded and handled: " + " Provider lastIndex:" + _lastIndexProvider.ToString() + " Subscriber lastChangeIndex:" + lastChangeIndexSubscriber.ToString());
            }

        }

        private void btnGetChangelogStatus_Click(object sender, EventArgs e)
        {
            try
            {
                string changelogId = txbChangelogid.Text;

                var response = GetChangelogStatusResponse(changelogId);

                VisXML(response);
            }
            catch (WebException webEx)
            {
                MessageBox.Show("Error : " + webEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void btnGetChangelog_Click(object sender, EventArgs e)
        {
            string response = "";
            try
            {
                string changelogId = txbChangelogid.Text;

                response = GetChangelog(changelogId);

                VisXML(response);

            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.Message, "Error");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
        }

        private void btnAcknowledgeChangelogDownloaded_Click(object sender, EventArgs e)
        {
            try
            {

                var response = AcknowledgeChangelogDownloaded(); // 20130529-Leg: refactored

                VisXML("<Message>AcknowledgeChangelogDownloaded sent to server</Message>");
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    VisXML("<Message>AcknowledgeChangelogDownloaded sent to server</Message>");
                    return;
                }
                MessageBox.Show(webEx.Message, "Error");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
        }

        private void btnCancelChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                var response = CancelChangelog(); // 20130529-Leg: refactored
                VisXML("<Message>CancelChangelog sent to server</Message>");
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    VisXML("<Message>CancelChangelog sent to server</Message>");
                    return;
                }
                MessageBox.Show(webEx.Message, "Error");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
        }

        /// <summary>
        /// Test parsing the Changelog.xml for wfs:Insert using LINQ XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnParseWfsChangelog_Click(object sender, EventArgs e)
        {
            // load an XML document from a file
            XElement changeLog = XElement.Load(txbDownloadedFile.Text);
            GetWfsInsert(changeLog);
            //GetWfsInsert(changeLog, handle: 1);

            string carriagereturn = "\r\n";

            string attr = "";

            GetTransactions(changeLog, out attr);

            string attr2 = "";
            GetTransactionCollection(changeLog, out attr2);

            MessageBox.Show("chlogf:TransactionCollection: " + attr2 + carriagereturn +
                            "chlogf:transactions:" + attr, "ParseWFS result");

            //
            // Build wfs-t transaction from changelog, and do the transaction
            //
            //this.DoWfsTransactions(changeLog);


        }

        /// <summary>
        /// Do Transactions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDoTransaction_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;

            // load an XML document from a file
            XElement changeLog = XElement.Load(txbDownloadedFile.Text);
            GetWfsInsert(changeLog);

            //
            // Build wfs-t transaction from changelog, and do the transaction
            //
            if (this.DoWfsTransactions(changeLog))
            {
                // sucsess - update subscriber lastChangeIndex
                //Properties.Settings.Default.lastChangeIndex = _lastIndexProvider;
                //Properties.Settings.Default.Save();
                //txbSubscrLastindex.Text = Properties.Settings.Default.lastChangeIndex.ToString();

                //20121122-Leg: update db with  lastindex
                //geosyncDBEntities localDb = new geosyncDBEntities();
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                dataset.LastIndex = _lastIndexProvider;
                _localDb.SaveChanges();
                txbSubscrLastindex.Text = dataset.LastIndex.ToString();
                // Update datagridview control with changes
                dgDataset.DataSource = _localDb.Dataset;

            }

            //
            // Display in geoserver openlayers
            //
            DisplayMap();


            this.Cursor = DefaultCursor;


        }

        /// <summary>
        /// Test full Syncronization without executing the other commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestSyncronizationComplete_Click(object sender, EventArgs e)
        {
            bool sucsess = TestSyncronizationComplete();
            if (sucsess)
            {
                //MessageBox.Show("TestSyncronizationComplete OK");
            }
            else
            {
                MessageBox.Show("TestSyncronizationComplete failed!");
            }

        }

        private void btnSoapGLI_Click(object sender, EventArgs e)
        {
            //Kartverket.GeosyncClient.WebFeatureServiceReplicationPortClient client = new Kartverket.GeosyncClient.WebFeatureServiceReplicationPortClient();
            //string lastindex = client.GetLastIndex();
            //txbLastIndex.Text = lastindex;
        }

        /// <summary>
        /// Handles the Click event of the btnSimplify control.
        /// Testing Schema transformation:
        ///    Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSimplify_Click(object sender, EventArgs e)
        {
            // Mapping from the nested structure of one or more simple features to the simple features for GeoServer
            TestSchemaTransformSimplifyChangelog();
        }


        #endregion

        #region helper methods

        /// <summary>
        /// Show XML in WebBrowser control
        /// </summary>
        /// <param name="xmlText"></param>
        private void VisXML(string xmlText)
        {
            //if (string.IsNullOrEmpty(xmlText)) throw new Exception("Response from server is empty!");
            //// Load the xslt used by IE to render the xml
            //XslCompiledTransform xTrans = new XslCompiledTransform();
            //string path = System.Environment.CurrentDirectory;
            //string xls = path.Substring(0, path.LastIndexOf("bin")) + "Files" + "\\defaultss.xlst";

            //xTrans.Load(xls);
            //// Read the xml string data into an XML reader object 
            //System.IO.StringReader sr = new System.IO.StringReader(xmlText);
            //XmlReader xReader = XmlReader.Create(sr);
            //// Apply / transform the XML data
            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //xTrans.Transform(xReader, null, ms);
            //// Reset the position
            //ms.Position = 0;
            //// Set to the document stream
            //webBrowser1.ScriptErrorsSuppressed = true;
            //webBrowser1.DocumentStream = ms;
        }

        private string GetRequest(string uri)
        {
            string response = "";
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                webRequest.Timeout = 1000 * 1000;
                webRequest.ContentType = "text/xml";
                webRequest.Method = "GET";
                webRequest.KeepAlive = true;
                HttpWebResponse res = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(res.GetResponseStream());
                response = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            // Check if response is empty
            if (string.IsNullOrEmpty(response))
            {
                throw new WebException("Empty response", WebExceptionStatus.Success);
            }
            // Check if response is an exception
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(response);
            XmlNode root = xmldoc.DocumentElement;
            if (root.Name == "ExceptionReport")
            {

                throw new WebException(root.InnerText);
            }
            return response;

        }

        /// <summary>
        /// Read XML data from file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns file content in XML string format</returns>
        private string GetTextFromXMLFile(string file)
        {
            StreamReader reader = new StreamReader(file);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        /// <summary>
        /// Create directory if missing
        /// </summary>
        /// <param name="path"></param>
        /// <returns> True if OK, false if exception </returns>
        private bool CreateFolderIfMissing(string path)
        {
            try
            {
                bool folderExists = Directory.Exists((path));
                if (!folderExists)
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Geosync operations

        /// <summary>
        /// Returnerer det som tilbyder støtter av dataset, filter operatorer og objekttyper.
        /// </summary>
        /// <returns></returns>
        private void GetCapabilitiesXml(string url)
        {

            CapabilitiesDataBuilder cdb = new CapabilitiesDataBuilder(_localDb, url);
            dgvProviderDataset.DataSource = cdb.ProviderDatasets;
            IDictionary<string,IList<object>> visibleColumns = new Dictionary<string,IList<object>>();
            IList<object> columnFormat = new List<object>();
            columnFormat.Add("Datasett");
            columnFormat.Add("1");
            columnFormat.Add(DataGridViewAutoSizeColumnMode.AllCells);
            visibleColumns.Add("name", columnFormat);
            columnFormat = new List<object>();
            columnFormat.Add("Applikasjonsskjema");
            columnFormat.Add("2");
            columnFormat.Add(DataGridViewAutoSizeColumnMode.ColumnHeader);
            visibleColumns.Add("appschema", columnFormat);
            columnFormat = new List<object>();
            columnFormat.Add("Navnerom");
            columnFormat.Add("3");
            columnFormat.Add(DataGridViewAutoSizeColumnMode.Fill);
            visibleColumns.Add("targetnamespace", columnFormat);                        
            columnFormat = new List<object>();
            columnFormat.Add("Datasett ID");
            columnFormat.Add("4");
            columnFormat.Add(DataGridViewAutoSizeColumnMode.ColumnHeader);
            visibleColumns.Add("providerdatasetid", columnFormat);
           
            foreach (DataGridViewColumn col in dgvProviderDataset.Columns)
            {
                col.Visible = false;
                if (visibleColumns.ContainsKey(col.Name.ToLower()))
                {
                    col.Visible = true;
                    columnFormat = visibleColumns[col.Name.ToLower()];
                    col.HeaderText = columnFormat[0].ToString();
                    col.DisplayIndex = Convert.ToInt32(columnFormat[1]);
                    col.AutoSizeMode = (DataGridViewAutoSizeColumnMode)columnFormat[2];
                }
            }
            dgvProviderDataset.AutoSize = true;            
            dgvProviderDataset.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;           

        }

        /// <summary>
        /// Returnerer gml xsd skjema for hele tjenesten eller for valgte typer gitt en dataseId
        /// </summary>
        /// <returns></returns>
        private string DescribeFeaturetype()
        {
            try
            {
                //geosyncDBEntities localDb = new geosyncDBEntities();
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);
                //nytt datasett id fra server (tilsvarende det publisert i GetCapabilities)
                var res = client.DescribeFeatureType(dataset.ProviderDatasetId.ToString(), null);

                return res.ToString();
            }
            catch (WebException webEx)
            {
                logger.ErrorException("DescribeFeaturetype WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("DescribeFeaturetype failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Henter siste endringsnr fra tilbyder. Brukes for at klient enkelt kan sjekke om det er noe nytt siden sist synkronisering
        /// </summary>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        private string GetLastIndexFromProvider(out string lastIndex)
        {
            lastIndex = "-1";
            try
            {
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);

                lastIndex = client.GetLastIndex(dataset.ProviderDatasetId.ToString());
                return lastIndex;
            }

            catch (WebException webEx)
            {
                logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("GetLastIndexFromProvider failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Get OrderChangelog Response and changelogid
        /// </summary>
        /// <param name="useGet"></param>
        /// <param name="startIndex"></param>
        /// <param name="schangelogid"></param>
        /// <returns></returns>
        private string GetOrderChangelogResponse(bool useGet, int startIndex, out string schangelogid)
        {
            schangelogid = "-1";

            var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

            WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);
            ChangelogOrderType order = new ChangelogOrderType();
            order.datasetId = dataset.ProviderDatasetId.ToString();
            order.count = dataset.MaxCount.ToString();
            order.startIndex = startIndex.ToString();


            ChangelogIdentificationType resp = client.OrderChangelog(order);

            schangelogid = resp.changelogId;


            return schangelogid;
        }

        /// <summary>
        /// Get ChangelogStatus Response
        /// </summary>
        /// <param name="changelogId"></param>
        /// <returns></returns>
        private string GetChangelogStatusResponse(string changelogId)
        {
            var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

            WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);


            ChangelogIdentificationType id = new ChangelogIdentificationType();
            id.changelogId = changelogId;

            ChangelogStatusType resp = client.GetChangelogStatus(id);

            return resp.ToString();
        }

        /// <summary>
        /// Get and download changelog
        /// </summary>
        /// <param name="changelogId"></param>
        /// <returns></returns>
        private string GetChangelog(string changelogId)
        {
            string response = "";
            try
            {

                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);


                ChangelogIdentificationType id = new ChangelogIdentificationType();
                id.changelogId = changelogId;

                ChangelogType resp = client.GetChangelog(id);


                string changelogid = resp.id.changelogId;
                string downloaduri = resp.downloadUri;
                string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
#if (NOT_FTP)
                string fileName = tempDir + @"\" + changelogid + "_Changelog.xml";
#else
                string ftpPath = "abonnent";
                CreateFolderIfMissing(tempDir + @"\" + ftpPath); // Create the abonnent folder if missing               

                string fileName = tempDir + @"\" + ftpPath + @"\" + Path.GetFileName(downloaduri) + ".zip";
#endif
                _downLoadedChangelogName = fileName;
#if (NOT_FTP)
                DownloadChangelog(downloaduri, fileName);
#else
                // DownloadChangelog now handles ftp asynchron download
                DownloadChangelog2(downloaduri, fileName);
                //DownloadChangelogFTP(downloaduri, fileName);
#endif


            }
            catch (WebException webEx)
            {
                MessageBox.Show(webEx.Message, "Error");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return null;
            }
            return response;
        }

        /// <summary>
        /// ListStoredChangelogs
        /// </summary>
        /// <returns></returns>
        private string ListStoredChangelogs()
        {
            try
            {
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);


                StoredChangelogType[] resp = client.ListStoredChangelogs(dataset.ProviderDatasetId.ToString());
                return "Antall lagrede endringslogger:" + resp.Count();
            }
            catch (WebException webEx)
            {
                logger.ErrorException("ListStoredChangelogs WebException:", webEx);
                throw;

            }
            catch (Exception ex)
            {
                logger.ErrorException("ListStoredChangelogs failed:", ex);
                throw;
            }

        }

        /// <summary>
        /// Bekrefte at endringslogg er lastet ned
        /// </summary>
        /// <returns></returns>
        private string AcknowledgeChangelogDownloaded()
        {
            try
            {

                string changelogId = txbChangelogid.Text;
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);

                ChangelogIdentificationType id = new ChangelogIdentificationType();
                id.changelogId = changelogId;
                client.AcknowlegeChangelogDownloaded(id);

                return "Jepp...helt fin sletting...";
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    return "";
                }

                logger.ErrorException("AcknowledgeChangelogDownloaded WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("AcknowledgeChangelogDownloaded failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Avbryter endringslogg jobb hvis feks noe går galt.
        /// </summary>
        /// <returns></returns>
        private string CancelChangelog()
        {

            try
            {
                //geosyncDBEntities localDb = new geosyncDBEntities();
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();

                WebFeatureServiceReplicationPortClient client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SyncronizationUrl);
                ChangelogIdentificationType id = new ChangelogIdentificationType();
                id.changelogId = txbChangelogid.Text;
                client.CancelChangelog(id);

                return "Jepp...helt fin avbryting...";
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    return "";
                }

                logger.ErrorException("CancelChangelog WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("CancelChangelog failed:", ex);
                throw;
            }
        }

        #endregion

        #region Download


        private bool DownloadDone = false;
        /// <summary>
        /// Download changelog
        /// </summary>
        /// <param name="downloadUri"></param>
        /// <param name="localFileName"> Filename including path </param>
        private bool DownloadChangelog2(string downloadUri, string localFileName)
        {
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');
            string ftpUser = par1[0].Split(':')[0];
            string ftpPasswd = par1[0].Split(':')[1];
            string ftpServer = par1[1].Split('/')[0];
            string ftpFileName = par1[1].Split('/')[1] + ".zip";
            Common.FileTransferHandler ftpHandler = new Common.FileTransferHandler();
            ftpHandler.ProgressChanged += new Common.FileTransferHandler.ProgressHandler(ftpHandler_ProgressChanged);
            ftpHandler.ProcessDone += new Common.FileTransferHandler.ProcessDoneHandler(ftpHandler_ProcessDone);
            if (ftpHandler.DownloadFileFromFtp(localFileName, ftpFileName, ftpServer, ftpUser, ftpPasswd))
            {
                if (Path.GetExtension(localFileName) != ".zip")
                {
                    logger.ErrorException("File " + localFileName + " is not a zip file", null);
                    return false;
                }

                string outPath = Path.GetDirectoryName(_downLoadedChangelogName);
                this.unpackZipFile(_downLoadedChangelogName, outPath);
                string xmlFile = Path.ChangeExtension(_downLoadedChangelogName, ".xml");
                _downLoadedChangelogName = xmlFile;


                txbDownloadedFile.Text = _downLoadedChangelogName;
                //MessageBox.Show("File downloaded: " + downLoadedChangelogName);
                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted: File downloaded");

                toolStripStatusLabel1.Text = "File downloaded: " + _downLoadedChangelogName;
                //listBoxLog.Items.Add("GetChangelog OK");
                statusStrip1.Refresh();
                return true;
            }
            else
            { return false; }
        }

        void ftpHandler_ProcessDone(object sender, Common.FileTransferHandler.ProgressEventArgs e)
        {
            if (!e.error)
            {
                DownloadDone = e.status == Common.FileTransferHandler.ftpStatus.done;
                toolStripProgressBar1.Maximum = e.totalFiles;
                toolStripProgressBar1.Value = e.currentFile;
                toolStripStatusLabel1.Text = "Downloaded file: " + e.fileName;
            }
        }

        void ftpHandler_ProgressChanged(object sender, Common.FileTransferHandler.ProgressEventArgs e)
        {
            if (!e.error)
            {
                toolStripProgressBar1.Maximum = e.totalFiles;
                toolStripProgressBar1.Value = e.currentFile;
                toolStripStatusLabel1.Text = "Downloading file: " + e.fileName;
            }

        }


        /// <summary>
        /// Download changelog
        /// </summary>
        /// <param name="downloadUri"></param>
        /// <param name="localFileName"> Filename including path </param>
        private void DownloadChangelog(string downloadUri, string localFileName)
        {

            // Create an instance of WebClient
            WebClient client = new WebClient();

#if !(NOT_FTP)
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');
            string ftpUser = par1[0].Split(':')[0];
            string ftpPasswd = par1[0].Split(':')[1];
            string ftpServer = par1[1].Split('/')[0];
            string ftpFileName = par1[1].Split('/')[1];
            client.Credentials = new NetworkCredential(ftpUser, ftpPasswd);

            string url = "ftp://" + ftpServer + "/" + ftpFileName + ".zip";
#endif


            // Hookup DownloadFileCompleted Event
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            // Start the download and copy the file to localFileName
#if (NOT_FTP)
            client.DownloadFileAsync(new Uri(downloadUri), localFileName);
#else
            client.DownloadFileAsync(new Uri(url), localFileName);
#endif

            // checking IsBusy to see if the background task is
            // still running.
            while (client.IsBusy)
            {
                toolStripProgressBar1.Increment(10);

                Application.DoEvents();
            }
        }

        /// <summary>
        /// This event is raised each time an asynchronous file download operation completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            // Set progress bar to 100% in case it's not already there.
            toolStripProgressBar1.Value = 100;


            if (e.Error == null)
            {
                if (Path.GetExtension(_downLoadedChangelogName) != ".zip")
                {
                    logger.ErrorException("File " + _downLoadedChangelogName + " is not a zip file", null);
                    return;
                }

                WebClient client = (WebClient)sender;

#if !(NOT_FTP)
                string outPath = Path.GetDirectoryName(_downLoadedChangelogName);

                FileInfo fileInfo = new FileInfo(_downLoadedChangelogName);
                int retry_counter = 0;

                while ((!fileInfo.Exists || fileInfo.Length == 0) && retry_counter < 5)
                {
                    logger.ErrorException("File " + _downLoadedChangelogName + " is empty, counter = " + retry_counter, null);
                    Thread.Sleep(2000);
                    fileInfo.Refresh();
                    retry_counter++;
                }

                if (retry_counter == 4)
                {
                    logger.ErrorException("File " + _downLoadedChangelogName + " is empty", null);
                    MessageBox.Show(
                        "Failed to download file",
                        "File " + _downLoadedChangelogName + " is empty",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted failed");

                }

                this.unpackZipFile(_downLoadedChangelogName, outPath);

                string localFileName = Path.ChangeExtension(_downLoadedChangelogName, ".xml");
                _downLoadedChangelogName = localFileName;
#endif

                txbDownloadedFile.Text = _downLoadedChangelogName;
                //MessageBox.Show("File downloaded: " + downLoadedChangelogName);
                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted: File downloaded");

                toolStripStatusLabel1.Text = "File downloaded: " + _downLoadedChangelogName;
                //listBoxLog.Items.Add("GetChangelog OK");
                statusStrip1.Refresh();

            }
            else
            {
                logger.ErrorException(e.Error.ToString(), null);
                MessageBox.Show(
                    "Failed to download file",
                    "Download failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted failed");
            }


            toolStripProgressBar1.Value = 0;
        }

        private bool unpackZipFile(string zipfile, string utpath)
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(zipfile))
                {
                    foreach (ZipEntry fil in zip)
                    {
                        fil.Extract(utpath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }

            catch (Exception ex)
            {
                logger.ErrorException("unpackZipFile failed for file :" + zipfile, ex);
                return false;
            }


            return true;

        }
        #endregion

        #region WFS transactions




        /// <summary>
        /// get all wfs:Insert, one is shown
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="handle"> Handle > = 0 shows the specific handle, else the last one is shown </param>
        /// <returns></returns>
        private int GetWfsInsert(XElement changeLog, int handle = 0)
        {
            try
            {
                int count = 0;
                //
                // get all wfs:Insert
                //

                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);

                foreach (var tran in transactions)
                {
                    // write out each insert
                    //Console.WriteLine(tran);
                    VisXML(tran.ToString(SaveOptions.DisableFormatting));
                    //webBrowser1.DocumentText = tran.ToString(SaveOptions.DisableFormatting);


                    // Write out each handle
                    Console.WriteLine(tran.Attribute("handle"));
                    ++count;
                }
                return count;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + ex.StackTrace);
                return -1;
                //throw;
            }

        }

        /// <summary>
        /// get all wfs transactions (Insert, Update,  Delete, Replace) from changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private IEnumerable<XElement> GetWfsTransactions(XElement changeLog)
        {
            try
            {
                // Namespace must be set: xmlns:wfs="http://www.opengis.net/wfs/2.0"
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";

                IEnumerable<XElement> transactions =
                    from item in changeLog.Descendants()
                    where item.Name == nsWfs + "Insert" || item.Name == nsWfs + "Delete" || item.Name == nsWfs + "Update" || item.Name == nsWfs + "Replace"
                    select item;

                return transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                return null;
                //throw;
            }
        }


        /// <summary>
        /// get chlogf:transactions
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private int GetTransactions(XElement changeLog, out string attributes)
        {
            try
            {
                int count = 0;
                //
                // get chlogf:transactions
                //
                XNamespace nschlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
                IEnumerable<XElement> chlogfTransactions =
                    from item in changeLog.Descendants(nschlogf + "transactions")
                    select item;
                Console.WriteLine("chlogf:transactions:");

                StringBuilder sb = new StringBuilder();
                foreach (var chlog in chlogfTransactions)
                {
                    Console.WriteLine(chlog.Attribute("service"));
                    //sb.Append("service:");
                    sb.Append(chlog.Attribute("service"));
                    ++count;
                }
                attributes = sb.ToString();

                return count;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                attributes = "";
                return -1;
                //throw;
            }

        }

        /// <summary>
        /// get chlogf:TransactionCollection - this is the root
        /// </summary>
        /// <param name="changeLog"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private int GetTransactionCollection(XElement changeLog, out string attributes)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                //
                // get chlogf:TransactionCollection - this is the root
                //
                //sb.Append("chlogf:TransactionCollection:");
                sb.Append(changeLog.Attribute("numberMatched"));
                sb.Append(changeLog.Attribute("numberReturned"));
                sb.Append(changeLog.Attribute("startIndex"));
                sb.Append(changeLog.Attribute("endIndex"));


                // Get the xsi:schemaLocation: http://www.falconwebtech.com/post/2010/06/03/Adding-schemaLocation-attribute-to-XElement-in-LINQ-to-XML.aspx
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                sb.AppendLine();
                sb.Append(changeLog.Attribute(xsi + "schemaLocation"));
                sb.AppendLine();

                //
                // Get the namespaces:
                //

                // Distinct() doesn't work with namespaces decleared as var!!
                IEnumerable<XNamespace> namespaces = (from x in changeLog.DescendantsAndSelf()
                                                      select x.Name.Namespace).Distinct();

                sb.AppendLine("namespaces:");
                foreach (var ns in namespaces)
                {
                    sb.AppendLine(ns.ToString());
                }

                attributes = sb.ToString();
                return 0;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + ex.StackTrace);
                attributes = "";
                return -1;

                //throw;
            }

        }

        /// <summary>
        /// Build wfs-t transaction from changelog, and do the transaction
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private bool DoWfsTransactions(XElement changeLog)
        {
            bool sucsess = false;

            try
            {

                var xDoc = BuildWfsTransaction(changeLog);
                if (xDoc == null)
                {
                    return false;
                }


                //
                // Post to GeoServer
                //
                try
                {

                    //20121122-Leg::  Get subscriber GeoServer url from db
                    //geosyncDBEntities localDb = new geosyncDBEntities();
                    var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                    String url = dataset.ClientWfsUrl;
                    //String url = Properties.Settings.Default.urlGeoserverSubscriber; // "http://localhost:8081/geoserver/app/ows?service=WFS";

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "text/xml"; //"application/x-www-form-urlencoded";
                    StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream());
                    xDoc.Save(writer);
                    writer.Close();

                    // get response from request
                    HttpWebResponse httpWebResponse = null;
                    Stream responseStream = null;
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    var resultString = new StringBuilder("");
                    using (var resultStream = httpWebResponse.GetResponseStream())
                    {
                        if (resultStream != null)
                        {
                            int count;
                            do
                            {
                                var buffer = new byte[8192];
                                count = resultStream.Read(buffer, 0, buffer.Length);
                                if (count == 0) continue;
                                resultString.Append(Encoding.UTF8.GetString(buffer, 0, count));
                            } while (count > 0);
                        }
                    }
                    httpWebResponse.Close();

                    if (httpWebResponse.StatusCode == HttpStatusCode.OK && resultString.ToString().Contains("ExceptionReport")==false)
                    {
                        //TODO en får alltid status 200 OK fra geoserver
                        //En må sjekke om en har fått ExceptionReport
                        //<?xml version="1.0" encoding="UTF-8"?>
                        //<ows:ExceptionReport version="2.0.0"
                        //  xsi:schemaLocation="http://www.opengis.net/ows/1.1 http://localhost:8081/geoserver/schemas/ows/1.1.0/owsAll.xsd"
                        //  xmlns:ows="http://www.opengis.net/ows/1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                        //  <ows:Exception exceptionCode="0">
                        //    <ows:ExceptionText>Error performing insert: Error inserting features
                        //Error inserting features
                        //ERROR: insert or update on table &amp;quot;navneenhet&amp;quot; violates foreign key constraint &amp;quot;navneenhet_navnetype_fkey&amp;quot;
                        //  Detail: Key (navnetype)=() is not present in table &amp;quot;navnetype&amp;quot;.</ows:ExceptionText>
                        //  </ows:Exception>
                        //</ows:ExceptionReport>



                        sucsess = true;
                        XElement transactionResponseElement = XElement.Parse(resultString.ToString());
                       

                        // TODO: It's not necesary to save the file here, but nice for debugging
                        string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
                        string fileName = tempDir + @"\" + "TransactionResponse" + txbChangelogid.Text + ".xml";
                        //transactionResponseElement.Save(fileName);

                        XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";
                        IEnumerable<XElement> transactionSummaries =
                            from item in transactionResponseElement.Descendants(nsWfs + "TransactionSummary")
                            select item;

                        if (transactionSummaries.Count() > 0)
                        {
                            string message = "Geoserver WFS-T Transaction: ";
                            string transactionSummary = transactionSummaries.ElementAt(0).ToString(SaveOptions.DisableFormatting);
                            //MessageBox.Show(message + "\r\n" + transactionSummary, "Transaction Status: " + httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                            logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + transactionSummary, httpWebResponse.StatusCode);
                            ////VisXML(tran.ToString(SaveOptions.DisableFormatting));
                            // For more debugging:
                            //logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode);

                            listBoxLog.Items.Add("TransactionSummary:");
                            IEnumerable<XElement> transactions =
                                from item in transactionResponseElement.Descendants()
                                where item.Name == nsWfs + "totalInserted" || item.Name == nsWfs + "totalUpdated" || item.Name == nsWfs + "totalReplaced" || item.Name == nsWfs + "totalDeleted"
                                select item;
                            foreach (var tran in transactions)
                            {
                                string tranResult = "unknown";
                                if (tran.Name == nsWfs + "totalInserted")
                                {
                                    tranResult = "totalInserted";
                                }
                                else if (tran.Name == nsWfs + "totalUpdated")
                                {
                                    tranResult = "totalUpdated";
                                }
                                else if (tran.Name == nsWfs + "totalDeleted")
                                {
                                    tranResult = "totalDeleted";
                                }
                                else if (tran.Name == nsWfs + "totalReplaced")
                                {
                                    tranResult = "totalReplaced";
                                }
                                listBoxLog.Items.Add(tranResult + ":" + tran.Value);
                            }
                            //listBoxLog.Items.Add("transactionSummary:" + transactionSummary);
                        }
                        else
                        {
                            string message = "Geoserver WFS-T Transaction: ";
                            MessageBox.Show(message + "\r\n" + "No transactions ", "Transaction Status: " + httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                            logger.Info("DoWfsTransactions:" + message + " transactionSummary" + " Transaction Status:{0}" + "\r\n" + "No transactions ", httpWebResponse.StatusCode);
                        }

                    }
                    else
                    {
                        string message = "Geoserver WFS-T Transaction feilet: ";
                        MessageBox.Show(message + "\r\n" + resultString.ToString(), "Transaction Status: " + httpWebResponse.StatusCode + " " + httpWebResponse.StatusDescription);
                        logger.Info("DoWfsTransactions: " + message + " Transaction Status:{0}" + "\r\n" + resultString.ToString(), httpWebResponse.StatusCode);

                    }

                }
                catch (WebException webEx)
                {
                    logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                    MessageBox.Show("WebException error : " + webEx.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    //Response.Write(exception.Message);
                    //Response.Write(exception.ToString());
                    logger.ErrorException("GetLastIndexFromProvider WebException:", ex);
                    return false;
                }

                return sucsess;

            }
            catch (Exception)
            {

                throw;
                return false;
            }
        }

        /// <summary>
        /// Buld WFS Transaction XDocument from Changelog
        /// </summary>
        /// <param name="changeLog"></param>
        /// <returns></returns>
        private XDocument BuildWfsTransaction(XElement changeLog)
        {
            try
            {
                // NameSpace manipulation: http://msdn.microsoft.com/en-us/library/system.xml.linq.xnamespace.xmlns(v=vs.100).aspx
                XNamespace nsWfs = "http://www.opengis.net/wfs/2.0"; // "wfs";


                // Get the xsi:schemaLocation: http://www.falconwebtech.com/post/2010/06/03/Adding-schemaLocation-attribute-to-XElement-in-LINQ-to-XML.aspx
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XAttribute xAttributeXsi = changeLog.Attribute(xsi + "schemaLocation");



                //
                // Generate the XDocument
                //
                XDocument xDoc = new XDocument(
                    new XElement(nsWfs + "Transaction", new XAttribute("version", "2.0.0"), new XAttribute("service", "WFS"),
                                 new XAttribute(XNamespace.Xmlns + "wfs", "http://www.opengis.net/wfs/2.0"), xAttributeXsi)
                    );

                //
                // Get the WFS-T transactions and add them to the root
                //
                IEnumerable<XElement> transactions = GetWfsTransactions(changeLog);
                XElement xRootElement = xDoc.Root;

                foreach (var tran in transactions)
                {
                    xRootElement.Add(tran);
                }

                //
                // Estimate number of transactions for each feature type
                //


                var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                                   group item.Name.LocalName                 //operation
                              by item.Elements().ElementAt(0).Name.LocalName //(Key) typeName-for Insert it follows in the next Element 
                                       into g
                                       select g;
                // If wfs:member comes before typeName for Insert:
                //var insertGroups = from item in changeLog.Descendants(nsWfs + "Insert")
                //                   group item.Name.LocalName                 //operation
                //              by item.Elements().Descendants().ElementAt(0).Name.LocalName //(Key) typeName-for Insert it follows in the next Element 
                //                       into g
                //                       select g;

                //var inserts = (from x in changeLog.Descendants(nsWfs + "Insert")
                //                                      select x.Name.LocalName);
                //System.Diagnostics.Debug.WriteLine("  features of {0}", inserts.Count());

                var updateGroups = from item in changeLog.Descendants(nsWfs + "Update")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Update it follows in the typeName attribute
                                       into g
                                       select g;

                var deleteGroups = from item in changeLog.Descendants(nsWfs + "Delete")
                                   group item.Name.LocalName                 //operation
                              by item.Attribute("typeName").Value //(Key) typeName-for Delete it follows in the typeName attribute
                                       into g
                                       select g;
                listBoxLog.Items.Add("Estimated transactions:");
                if (insertGroups.Any())
                {
                    foreach (var group in insertGroups)
                    {
                        // Insert: count of number of Insert transactions:
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                        listBoxLog.Items.Add(group.First().ToString() + " operation:" + group.Count().ToString() + " features of " + group.Key);
                    }
                }

                if (updateGroups.Any())
                {
                    foreach (var group in updateGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                        listBoxLog.Items.Add(group.First().ToString() + ":" + group.Count().ToString() + " features of " + group.Key);
                    }
                }

                if (deleteGroups.Any())
                {
                    foreach (var group in deleteGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{1}: {0} features of {2}", group.Count(), group.First(), group.Key);
                        listBoxLog.Items.Add(group.First().ToString() + ":" + group.Count().ToString() + " features of " + group.Key);
                    }
                }

                //
                // Get the namespaces, and update the XDocument:
                // See: http://www.hanselman.com/blog/GetNamespacesFromAnXMLDocumentWithXPathDocumentAndLINQToXML.aspx
                // 
                var result = changeLog.Attributes().
                    Where(a => a.IsNamespaceDeclaration).
                    GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                            a => XNamespace.Get(a.Value)).
                    ToDictionary(g => g.Key,
                                 g => g.First());

                XElement xEl = xDoc.Root;
                foreach (var xns in result)
                {
                    try
                    {
                        xEl.Add(new XAttribute(XNamespace.Xmlns + xns.Key, xns.Value));
                    }
                    catch (Exception)
                    {
                        //TODO: FIX
                    }
                }

                // System.Diagnostics.Debug.WriteLine(xDoc);

                // TODO: It's not necesary to save the file here, but nice for debugging
                if (transactions.Count() <= 50)
                {
                    //string fileName = @"c:\temp\" + "_wfsT-test1.xml";
                    string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
                    string fileName = tempDir + @"\" + "_wfsT-test1.xml";
                    //System.Environment.GetFolderPath(Environment.SpecialFolder.)
                    //System.IO.Path.GetTempPath();


                    xDoc.Save(fileName);
                }

                return xDoc;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + ex.StackTrace, "BuildWfsTransaction");
                //throw;
                return null;
            }
        }
        #endregion

        #region Schema Transformation

        /// <summary>
        /// Test Schema transformation - using hardcoded files
        /// Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
        /// </summary>
        private void TestSchemaTransformSimplifyChangelog()
        {
            try
            {

                string path = System.Environment.CurrentDirectory;
                string fileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" + @"\_wfsT-test1.xml";

                // Test empty changelog
                // fileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" + @"\ar5-tom-07a8e3ef-7315-409f-862d-6417b4275368.xml";

                string mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" + @"\ar5FeatureType-mapping-file.xml";
           

                // load the changelog XML document from file
                // XElement changeLog = XElement.Load(fileName);

                // Set up GeoServer mapping
                GeoserverMapping geoserverMap = new GeoserverMapping();
                geoserverMap.NamespaceUri = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                geoserverMap.SetXmlMappingFile(mappingFileName);

                // Simplify
                XElement newChangeLog = geoserverMap.Simplify(fileName);
                if (newChangeLog != null)
                {
                    string newFileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" + @"\New_wfsT-test1.xml";
                    newChangeLog.Save(newFileName);

                    string msg = "Source: " + fileName;
                    msg += "\r\n" + "Target: " + newFileName;
                    msg += "\r\n" + "Mappingfile: " + mappingFileName;
                    msg += "\r\n" + "Schema: " + geoserverMap.NamespaceUri;
                    logger.Info("TestSimplifyChangelog Schema transformation OK {0}", msg);
                    MessageBox.Show("Sucsessfull schema transformation." + "\r\n" + msg, "TestSimplifyChangelog");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "TestSimplifyChangelog");

            }
        }

        /// <summary>
        /// Transform the changelog file to a simplified form based on GeoServer mappiing file.
        /// </summary>
        /// <param name="fileName">Name of the file to transform.</param>
        /// <returns> Name of transformed changelog</returns>
        private string SchemaTransformSimplify(string fileName)
        {
            string newFileName = "";
            try
            {

                // Create new stopwatch
                Stopwatch stopWatch = new Stopwatch();


                //var stopwatch = System.Diagnostics.Stopwatch.StartNew();


                // 
                string path = System.Environment.CurrentDirectory;
                string mappingFileName;
                //mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" +
                //                         @"\ar5FeatureType-mapping-file.xml";

                // Get Mappingfile and TargetNamespace from database
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                string namespaceUri = dataset.TargetNamespace;
                mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + dataset.MappingFile; //"SchemaMapping" + @"\" + dataset.MappingFile;


                // Set up GeoServer mapping
                // TODO: GetCapabilities should deliver NamespaceUri? Once, or get every time?
                GeoserverMapping geoserverMap = new GeoserverMapping();
                //geoserverMap.NamespaceUri = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                geoserverMap.NamespaceUri = namespaceUri;
                geoserverMap.SetXmlMappingFile(mappingFileName);

                // Simplify
                XElement newChangeLog = geoserverMap.Simplify(fileName);

                if (newChangeLog != null)
                {
                    string outPath = Path.GetDirectoryName(fileName);
                    newFileName = outPath + @"\" + "New_" + Path.GetFileName(fileName);
                    newChangeLog.Save(newFileName);
                    string msg = "Source: " + fileName;
                    msg += "\r\n" + "Target: " + newFileName;
                    msg += "\r\n" + "Mappingfile: " + mappingFileName;
                    msg += "\r\n" + "Schema: " + geoserverMap.NamespaceUri;
                    logger.Info("SchemaTransform Schema transformation OK {0}", msg);
                    //MessageBox.Show("Sucsessfull schema transformation." + "\r\n" + msg, "TestSimplifyChangelog");

                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                logger.Info("SchemaTransformSimplify RunTime: {0}", elapsedTime);


            }
            catch (Exception ex)
            {
                logger.ErrorException("SetXmlMappingFile:", ex);
                throw;
            }

            return newFileName;
        }

        #endregion

        #region Test SyncronizationComplete

        /// <summary>
        /// Test full Syncronization without any user input,
        /// </summary>
        /// <returns></returns>
        private bool TestSyncronizationComplete()
        {
            //            GetLastIndex (ikke endre verdi)
            //            OrderChangelog
            //            GetChangelog
            //            DoTransaction

            try
            {
                listBoxLog.Items.Clear();
                //listBoxLog.Refresh();

                // Create new stopwatch
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                logger.Info("TestSyncronizationComplete start");
                //logger.Info("GetLastIndexFromProvider lastIndex :{0}{1}", "\t", lastIndex);

                //
                // GetLastIndex from Provider
                //
                toolStripStatusLabel1.Text = "GetLastIndexFromProvider";
                statusStrip1.Refresh();

                string lastIndex;
                var response = GetLastIndexFromProvider(out lastIndex);
                //txbLastIndex.Text = lastIndex;
                _lastIndexProvider = Convert.ToInt32(lastIndex);
                //VisXML(response);
                toolStripStatusLabel1.Text = "GetLastIndexFromProvider OK";
                statusStrip1.Refresh();
                listBoxLog.BeginUpdate();
                listBoxLog.Items.Add("GetLastIndexFromProvider:" + lastIndex.ToString());
                listBoxLog.EndUpdate();

                //
                // OrderChangelog
                //
                Cursor.Current = Cursors.WaitCursor;

                toolStripStatusLabel1.Text = "Order Changelog. Wait...";
                statusStrip1.Refresh();

                //20121122-Leg: get lastindex from db
                //geosyncDBEntities localDb = new geosyncDBEntities();
                var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
                int lastChangeIndexSubscriber = (int)dataset.LastIndex;
                //int lastChangeIndexSubscriber = Properties.Settings.Default.lastChangeIndex;


                if (lastChangeIndexSubscriber >= _lastIndexProvider)
                {
                    Cursor.Current = Cursors.Default;
                    string message = "Changelog has already been downloaded and handled:";
                    logger.Info(message + " Provider lastIndex:{0} Subscriber lastChangeIndex:{1}", _lastIndexProvider, lastChangeIndexSubscriber);
                    toolStripStatusLabel1.Text = message;
                    statusStrip1.Refresh();
                    MessageBox.Show(message + " Provider lastIndex:" + _lastIndexProvider.ToString() + " Subscriber lastChangeIndex:" + lastChangeIndexSubscriber.ToString());
                    return false;
                }

                // 20121107-Leg            
                int maxCount = (int)dataset.MaxCount;

                // TODO: For testing, remove?
                // lastChangeIndexSubscriber = _lastIndexProvider - 10;
                int limitNumberOfFeature = 0;
                if (int.TryParse(txtLimitNumberOfFeatures.Text, out limitNumberOfFeature))
                {
                    if (limitNumberOfFeature > 0)
                    {
                        lastChangeIndexSubscriber = _lastIndexProvider - limitNumberOfFeature;
                    }
                }


                int numberOfFeatures = _lastIndexProvider - lastChangeIndexSubscriber;


                int numberOfOrders = (numberOfFeatures / maxCount);
                if (numberOfFeatures % maxCount > 0)
                    ++numberOfOrders;

                logger.Info("TestSyncronizationComplete: numberOfFeatures:{0} numberOfOrders:{1} maxCount:{2}", numberOfFeatures, numberOfOrders, maxCount);
                listBoxLog.Items.Add("maxCount:" + maxCount.ToString());
                if (lastChangeIndexSubscriber < _lastIndexProvider)
                {
                    listBoxLog.Items.Add("Subscriber lastChangeIndex:" + lastChangeIndexSubscriber.ToString());
                    for (int i = 0; i < numberOfOrders; i++)
                    {

                        // 20130822-Leg: Fix for initial/first syncronization: Provider startIndex (GetLastIndex) starts at 1
                        int startIndex = lastChangeIndexSubscriber + 1;

                        if (i > 0)
                        {
                            //startIndex = (i * maxCount) + lastChangeIndexSubscriber;
                            startIndex = (i * maxCount) + lastChangeIndexSubscriber + 1;
                        }

                        System.Diagnostics.Debug.WriteLine("startIndex:" + startIndex.ToString() + " lastChangeIndex:" +
                                                           lastChangeIndexSubscriber.ToString());
                        //listBoxLog.Items.Add("Subscriber lastChangeIndex:" + lastChangeIndexSubscriber.ToString());
                        listBoxLog.Items.Add("Subscriber startIndex:" + startIndex.ToString());

                        bool useGet = true;
                        string schangelogid;

                        response = GetOrderChangelogResponse(useGet, startIndex, out schangelogid);
                        if (response == null)
                        {
                            System.Diagnostics.Debug.WriteLine("GetOrderChangelogResponse failed");
                            return false;
                        }
                        //VisXML(response);
                        txbChangelogid.Text = schangelogid;
                        toolStripStatusLabel1.Text = "OrderChangelog OK";
                        statusStrip1.Refresh();
                        listBoxLog.Items.Add("ChangelogId:" + schangelogid);


                        Cursor.Current = DefaultCursor;

                        //
                        //            GetChangelog
                        //
                        Cursor.Current = Cursors.WaitCursor;
                        toolStripStatusLabel1.Text = "GetChangelog. Wait...";
                        statusStrip1.Refresh();
                        string changelogId = txbChangelogid.Text;
                        response = "";
                        response = GetChangelog(changelogId);
                        if (response == null)
                        {
                            System.Diagnostics.Debug.WriteLine("GetOrderChangelogResponse failed");
                            logger.Info("TestSyncronizationComplete: GetOrderChangelogResponse failed");
                            Cursor.Current = Cursors.Default;
                            return false;
                        }
                        //VisXML(response);
                        toolStripStatusLabel1.Text = "GetChangelog OK";
                        listBoxLog.Items.Add("GetChangelog OK");
                        statusStrip1.Refresh();
                        Cursor.Current = Cursors.Default;

                        //
                        //            DoTransaction
                        //
                        toolStripStatusLabel1.Text = "DoTransaction. Wait...";
                        statusStrip1.Refresh();
                        Cursor.Current = Cursors.WaitCursor;



                        //
                        // Schema transformation
                        // Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
                        //
                        string fileName = txbDownloadedFile.Text;
                        var newFileName = SchemaTransformSimplify(fileName);
                        if (!string.IsNullOrEmpty(newFileName))
                        {
                            fileName = newFileName;
                        }

                        // load an XML document from a file
                        XElement changeLog = XElement.Load(fileName);
                        //XElement changeLog = XElement.Load(txbDownloadedFile.Text);



#if (false)              
                        if (GetWfsInsert(changeLog) < 0)
                        {
                            return false;
                        }                   
#endif

                        // Build wfs-t transaction from changelog, and do the transaction          
                        if (this.DoWfsTransactions(changeLog))
                        {
                            // sucsess - update subscriber lastChangeIndex
                            int lastIndexSubscriber = _lastIndexProvider;
                            if (numberOfOrders > 1 && i < (numberOfOrders - 1))
                            {
                                lastIndexSubscriber = (i * maxCount) + lastChangeIndexSubscriber;
                                toolStripStatusLabel1.Text = "DoWfsTransactions OK, pass " + (i + 1).ToString();
                                statusStrip1.Refresh();
                                logger.Info("DoWfsTransactions OK, pass {0}", (i + 1));
                                listBoxLog.Items.Add("DoWfsTransactions OK, pass " + (i + 1).ToString());
                            }
                            else
                            {
                                toolStripStatusLabel1.Text = "TestSyncronizationComplete OK";
                                statusStrip1.Refresh();
                                logger.Info("TestSyncronizationComplete sucsess");
                                listBoxLog.Items.Add("TestSyncronizationComplete sucsess");
                            }

                            //20121122-Leg: update db with  lastindex
                            dataset.LastIndex = lastIndexSubscriber;
                            _localDb.SaveChanges();
                            txbSubscrLastindex.Text = lastIndexSubscriber.ToString();

                            // Update datagridview control with changes
                            dgDataset.DataSource = _localDb.Dataset;
                            //dgDataset.Refresh();

                            //Properties.Settings.Default.lastChangeIndex = lastIndexSubscriber; //_lastIndex;
                            //Properties.Settings.Default.Save();
                            //txbSubscrLastindex.Text = Properties.Settings.Default.lastChangeIndex.ToString();

                            //TODO: Call AcknowlegeChangelogDownloaded here?
                            // Bekrefte at endringslogg er lastet ned
                            AcknowledgeChangelogDownloaded();
                        }


                    }


                }

                // Stop timing
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                listBoxLog.Items.Add("TestSyncronizationComplete RunTime: " + elapsedTime);
                logger.Info("TestSyncronizationComplete RunTime: {0}", elapsedTime);

                // Display in geoserver openlayers
                toolStripStatusLabel1.Text = "DisplayMap";
                statusStrip1.Refresh();
                DisplayMap(epsgCode: "EPSG:32633", datasetName: txtDataset.Text); //DisplayMap(epsgCode: "EPSG:32633");
                toolStripStatusLabel1.Text = "Ready";
                statusStrip1.Refresh();

                Cursor.Current = Cursors.WaitCursor;

                return true;
            }
            catch (FaultException e)
            {
                MessageBox.Show("Error : " + e.Message);
                return false;
            }
            catch (WebException webEx)
            {
                MessageBox.Show("Error : " + webEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Map Display

        /// <summary>
        /// Display map in geoserver openlayers
        /// </summary>
        private void DisplayMap(string epsgCode = "EPSG:4258", string datasetName = "ar5")
        {
            string bBox;
            switch (epsgCode)
            {

                case "EPSG:32633":
                    bBox = "-76910,6448455,1000982,7939899";
                    break;
                default: //EPSG:4258
                    bBox = "4.0,57.0,20.0,70.0";
                    break;
            }
            // app:Skjær will not display due to "æ"
            string layers = "app:Flytebrygge,app:Flytebryggekant,app:Kystkontur,app:HavElvSperre,app:KystkonturTekniskeAnlegg";
            string hostWms = "http://localhost:8081/geoserver/app/wms?service=WMS&version=1.1.0&request=GetMap";

            if (datasetName.ToLower() == "ar5")
            {
                layers = "ar5:ArealressursFlate,ar5:KantUtsnitt,ar5:ArealressursGrense,ar5:ArealressursGrenseFiktiv";
                hostWms = "http://localhost:8081/geoserver/ar5/wms?service=WMS&version=1.1.0&request=GetMap";
                bBox = "10.0,59.6,10.2,59.8";
                epsgCode = "EPSG:4258";
            }


            StringBuilder sb = new StringBuilder();
            sb.Append(hostWms);
            //sb.Append("http://localhost:8081/geoserver/app/wms?service=WMS&version=1.1.0&request=GetMap");
            sb.Append("&layers=" + layers);
            sb.Append("&styles=");
            sb.Append("&bbox=" + bBox);
            sb.Append("&width=512&height=416");
            sb.Append("&srs=" + epsgCode);
            sb.Append("&format=application/openlayers");
            //sb.Append("&CQL_FILTER=lokal_id = 1");
            string geoserverUrl = sb.ToString();
            //geoserverUrl = "http://localhost:8081/geoserver/ar5/wms?service=WMS&version=1.1.0&request=GetMap&layers=ar5:ArealressursFlate&styles=&bbox=10.0,59.6,10.2,59.8&width=304&height=512&srs=EPSG:4258&format=application/openlayers";

            // http://localhost:8081/geoserver/ar5/wms?service=WMS&version=1.1.0&request=GetMap&layers=ar5:ArealressursFlate&styles=&bbox=10.0,59.6,10.2,59.8&width=304&height=512&srs=EPSG:4258&format=application/openlayers
            // http://localhost:8081/geoserver/ar5/wms?service=WMS&version=1.1.0&request=GetMap&layers=ar5:ArealressursFlate&styles=&bbox=10.0956697463989,59.7662086486816,10.1084928512573,59.7877578735352&width=304&height=512&srs=EPSG:4258&format=application/openlayers
            // http://localhost:8081/geoserver/ar5/wms?service=WMS&version=1.1.0&request=GetMap&layers=ar5:KantUtsnitt&styles=&bbox=9.98602014038926,59.6960364835843,10.1071185944591,59.7432999411019&width=845&height=330&srs=EPSG:4258&format=application/openlayers
            //string geoserverUrl =
            //    "http://localhost:8081/geoserver/app/wms?service=WMS&version=1.1.0&request=GetMap&layers=app:Flytebrygge,app:Flytebryggekant,app:Kystkontur&styles=&bbox=4.0,57.0,20.0,70.0&width=512&height=416&srs=EPSG:4258&format=application/openlayers";

            //VisXML(geoserverUrl);
            webBrowser1.Navigate(geoserverUrl);

            // UTM 33: EPSG:32633
            // http://localhost:8081/geoserver/app/wms?service=WMS&version=1.1.0&request=GetMap&layers=app:Flytebrygge,app:Flytebryggekant,app:Kystkontur&styles=&bbox=-76910,6448455,1000982,7939899&width=512&height=416&srs=EPSG:32633&format=application/openlayers

            // Vi kan samle opp alle lokal_id
            // Filter: CQL_FILTER=lokal_id = 1
            // lokal_id IN (1,2,3,4,10)
            // eksempel:
            // http://localhost:8081/geoserver/app/wms?service=WMS&version=1.1.0&request=GetMap&layers=app:Flytebrygge&styles=&bbox=4.0,57.0,20.0,70.0&width=512&height=416&srs=EPSG:4258&format=application/openlayers&CQL_FILTER=lokal_id = 1
        }

        #endregion


        #region Get Capabilities - Hente datasett fra tilbyder

        private void btnGetCapabilities_Click(object sender, EventArgs e)
        {
            //var dataset = (from d in _localDb.Dataset where d.Name == txtDataset.Text select d).FirstOrDefault();
            string Url = txbProviderURL.Text;
            GetCapabilitiesXml(Url);
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;

        }
        private void btnGetProviderDatasets_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                GetCapabilitiesXml(txbProviderURL.Text);
            }
            catch (Exception ex)
            {
                this.Cursor = this.DefaultCursor;
                MessageBox.Show("Error : " + ex.Message);
            }
            finally
            {
                this.Cursor = this.DefaultCursor;
            }

        }

        private void btnAddSelected_Click(object sender, EventArgs e)
        {
            IList<int> selectedDataset = new List<int>();
            foreach (DataGridViewRow dgr in dgvProviderDataset.SelectedRows)
            {
                selectedDataset.Add(dgr.Index);
            }
            IBindingList blDataset = (IBindingList)dgvProviderDataset.DataSource;
            if (!Database.DatasetsData.AddDatasets(_localDb, blDataset, selectedDataset))
            {
                MessageBox.Show(this, "Error saving selected datasets to internal Database.", "Get Provider Datasets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(this, "Saved selected datasets to the internal Database.", "Get Provider Datasets", MessageBoxButtons.OK, MessageBoxIcon.Information);               
                dgDataset.DataSource = null;
                _localDb.Connection.Close();
                _localDb.Connection.Dispose();

                _localDb = new geosyncDBEntities();
                dgDataset.DataSource = _localDb.Dataset;
                dgDataset.Refresh();
            }
        }

        
        private void dgvProviderDataset_SelectionChanged(object sender, EventArgs e)
        {
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }
        #endregion

    }




    #region Not_in_Use
#if (NOT_IN_USE)

         private void DownloadChangelogFTP(string downloadUri, string localFileName)
        {

            changeLogHandlerSubscriber chgLogHandler = new changeLogHandlerSubscriber(logger);
            //chgLogHandler.FtpDownloadComplete +=chgLogHandler_FtpDownloadComplete;
            chgLogHandler.FtpDownloadComplete += new changeLogHandlerSubscriber.FtpDownloadCompleteHandler(chgLogHandler_FtpDownloadComplete);
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');
            string ftpUser = par1[0].Split(':')[0];
            string ftpPasswd = par1[0].Split(':')[1];
            string ftpServer = par1[1].Split('/')[0];
            string ftpFileName = par1[1].Split('/')[1];
            //chgLogHandler.DownloadFileFromFtp(ftpFileName + ".zip", localFileName, ftpServer, ftpUser, ftpPasswd);

            //TODO: Testing Synchron FTP: Det er feil i den asynkrone FTP-koden i changeLogHandlerSubscriber
            chgLogHandler.DownloadFileFromFtpSynchron(ftpFileName + ".zip", localFileName, ftpServer, ftpUser, ftpPasswd);
            string localFileNameDir = Path.GetDirectoryName(localFileName);
            chgLogHandler.unpackZipFile(localFileName, localFileNameDir);
            downLoadedChangelogName = Path.ChangeExtension(localFileName, "xml");
            txbDownloadedFile.Text = downLoadedChangelogName; //20130125-Leg
        }

        void chgLogHandler_FtpDownloadComplete(object sender, ftpDownloadEventArgs e)
        {
            changeLogHandlerSubscriber chgLogHandler = new changeLogHandlerSubscriber(logger);

            string localFileName = e.fileName;
            string localFileNameDir = Path.GetDirectoryName(localFileName);
            chgLogHandler.unpackZipFile(localFileName, localFileNameDir);
            downLoadedChangelogName = Path.ChangeExtension(localFileName, "xml");

            //string localFileName = Path.GetFileNameWithoutExtension(e.fileName) + ".xml";
            //chgLogHandler.unpackZipFile(e.fileName, localFileName);
            //downLoadedChangelogName = localFileName;


            txbDownloadedFile.Text = downLoadedChangelogName; //20130125-Leg
        }
    #region changeLogHandlerSubscriber_FTP
    public class changeLogHandlerSubscriber
    {


        string m_zipFile;
        string m_workingDirectory;
        string m_changeLog;

        Logger m_logger;
        public delegate void FtpDownloadCompleteHandler(object sender, ftpDownloadEventArgs e);

        public event FtpDownloadCompleteHandler FtpDownloadComplete;

        protected virtual void FtpOnFileDownloadComplete(string targetFileName)
        {

            if (FtpDownloadComplete != null)
            {
                FtpDownloadComplete(this, new ftpDownloadEventArgs(targetFileName, ""));
            }
        }

        public changeLogHandlerSubscriber(Logger logger)
        {

            m_logger = logger;
            m_workingDirectory = System.IO.Path.GetTempPath();

        }

        protected class FtpState
        {
            private ManualResetEvent wait;
            private FtpWebRequest request;
            private string fileName;
            private Exception operationException = null;
            string status;

            public FtpState()
            {
                wait = new ManualResetEvent(false);
            }

            public ManualResetEvent OperationComplete
            {
                get { return wait; }
            }

            public FtpWebRequest Request
            {
                get { return request; }
                set { request = value; }
            }

            public string FileName
            {
                get { return fileName; }
                set { fileName = value; }
            }
            public Exception OperationException
            {
                get { return operationException; }
                set { operationException = value; }
            }
            public string StatusDescription
            {
                get { return status; }
                set { status = value; }
            }
        }

        protected class AsynchronousFtpUpLoader
        {

            private string m_ftpServer;
            private string m_user;
            private string m_pwd;

            private Logger m_logger;

            public delegate void DownloadCompleteHandler(object sender, ftpDownloadEventArgs e);

            public event DownloadCompleteHandler DownloadComplete;

            protected virtual void OnFileDownloadComplete(string targetFilename)
            {

                if (DownloadComplete != null)
                {

                    DownloadComplete(this, new ftpDownloadEventArgs(targetFilename, ""));
                }
            }

            public AsynchronousFtpUpLoader(string ftpserver, string user, string password, Logger logger)
            {
                m_ftpServer = ftpserver;
                m_user = user;
                m_pwd = password;
                m_logger = logger;
            }
            // Command line arguments are two strings:
            // 1. The url that is the name of the file being uploaded to the server.
            // 2. The name of the file on the local machine.
            //
            public void UploadFileToFtpServer(string fileName, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                bool passv = true;
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;


                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = fileName;

                // Get the event to wait on.
                waitObject = state.OperationComplete;

                // Asynchronously get the stream for the file contents.
                request.BeginGetRequestStream(
                    new AsyncCallback(EndGetStreamCallback),
                    state
                );

                // Block the current thread until all operations are complete.
                // waitObject.WaitOne();

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("The operation completed - {0}", state.StatusDescription)));

                    m_logger.Info(string.Format("Upload of file {0} started", fileName));

                }
            }

            public void downloadFileFromFTPServer(string SourceFileName, string TargetFilename, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (SourceFileName.Contains(@"\")) file = SourceFileName.Substring(SourceFileName.LastIndexOf(@"\") + 1); else file = SourceFileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                bool passv = true;// Core.Config.GetSettingString(Core.Config.configSetting.SKSDftppassive) == "1";
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = TargetFilename;

                // Get the event to wait on.
                waitObject = state.OperationComplete;

                // Asynchronously get the stream for the file contents.
                request.BeginGetResponse(
                    new AsyncCallback(EndGetStreamCallbackDownload),
                    state
                );

                // Block the current thread until all operations are complete.
                //waitObject.WaitOne();

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    m_logger.Info(string.Format("Download of file {0} started", target));

                }
            }

            private void EndGetStreamCallbackDownload(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                FtpWebResponse fileFtpRepsonse;
                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    fileFtpRepsonse = (FtpWebResponse)state.Request.EndGetResponse(ar);

                    requestStream = fileFtpRepsonse.GetResponseStream();
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 2048;
                    stream = File.Create(state.FileName);
                    do
                    {
                        readBytes = requestStream.Read(buffer, 0, bufferLength);

                        stream.Write(buffer, 0, readBytes);
                        count += readBytes;
                    }
                    while (readBytes != 0);

                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                    m_logger.Info(string.Format("Download of file {0} finished", state.FileName));
                    OnFileDownloadComplete(state.FileName);
                }
                // Return exceptions to the main application thread.

                catch (Exception e)
                {
                    // Throw exception
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }


            }
            private void EndGetStreamCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    requestStream = state.Request.EndGetRequestStream(ar);
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 0;
                    stream = File.OpenRead(state.FileName);
                    do
                    {
                        readBytes = stream.Read(buffer, 0, bufferLength);
                        requestStream.Write(buffer, 0, readBytes);
                        count += readBytes;
                    }
                    while (readBytes != 0);
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("Writing {0} bytes to the stream.", count.ToString())));
                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                    m_logger.Info(string.Format("Upload of file {0} finished", state.FileName));

                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("Could not get the request stream. \n{0}", e.Message.ToString()), true));
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }


            }

            // The EndGetResponseCallback method  
            // completes a call to BeginGetResponse.
            private void EndGetResponseCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;
                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                    response.Close();
                    state.StatusDescription = response.StatusDescription;
                    // Signal the main application thread that 
                    // the operation is complete.
                    state.OperationComplete.Set();
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    Console.WriteLine("Error getting response.");
                    state.OperationException = e;
                    state.OperationComplete.Set();
                }
            }




            public bool DeleteFileOnServer(string fileName, string user = null, string password = null)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}{1}", "Starts deleting file:", file)));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.               
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Method = WebRequestMethods.Ftp.DeleteFile;



                bool passv = true;// Core.Config.GetSettingString(Core.Config.configSetting.SKSDftppassive) == "1";
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }


                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}", response.StatusDescription)));
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {

                        }

                    }

                }
                return true;
            }



            public static bool GetFilListFromFTP(string ftpServer, string user, string pwd)
            {
                List<string> strFileList = new List<string>();
                FtpWebRequest fwr;
                try
                {
                    fwr = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServer + "//"));
                    fwr.Credentials = new NetworkCredential(user, pwd);
                    fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                StreamReader sr = null;
                try
                {

                    sr = new StreamReader(fwr.GetResponse().GetResponseStream());

                    string str = sr.ReadLine();
                    if (str == null) return false;
                    while (str != null)
                    {
                        strFileList.Add(str);
                        str = sr.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (sr != null) sr.Close();
                    sr = null;
                    fwr = null;
                }


                return true;
            }



            public bool FileExistsOnServer(string fileName)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Check if file exists: {0}{1}", "Looking for file:", file)));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Credentials = new NetworkCredential(m_user, m_pwd);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Check file status: {0}", response.StatusDescription)));
                }
                catch (WebException ex)
                {
                    FtpWebResponse responseRes = (FtpWebResponse)ex.Response;
                    if (responseRes.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    if (response != null) response.Close();
                }
                return true;
            }


        }






        private bool cleanUpTemp()
        {
            string zipFile = m_zipFile;

            try
            {

                if (File.Exists(zipFile)) File.Delete(zipFile);
                if (Directory.Exists(m_workingDirectory)) Directory.Delete(m_workingDirectory, true);
            }
            catch (Exception exp)
            {
                m_logger.ErrorException("cleanUpTemp function failed:", exp);
                throw new System.Exception("cleanUpTemp function failed", exp);

            }
            return true;
        }




        public bool UploadFileToFtp(string FileName, string ftpserver = null, string user = null, string password = null)
        {
            // Todo: get ftpserver from setting
            string server = "";
            if (ftpserver != null) server = ftpserver;

            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(server, user, password, m_logger);
            try
            {
                //ftp.FileSendingProgressChanged += new AsynchronousFtpUpLoader.FileSendingProgressHandler(FileSendingProgressChanged);
                ftp.UploadFileToFtpServer(Path.Combine(m_workingDirectory, FileName), user, password);

            }
            catch (Exception exp)
            {
                m_logger.ErrorException("UploadFileToFtp function failed:", exp);
                throw new System.Exception("UploadFileToFtp function failed", exp);

            }
            finally
            {
                ftp = null;
            }



            return true;
        }



        // TODO: Newer got this to work
        public bool DownloadFileFromFtp(string SourceFileName, string TargetFileName, string ftpserver = null, string user = null, string password = null)
        {
            // Todo: get ftpserver from setting
            string server = "";
            if (ftpserver != null) server = ftpserver;

            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(server, user, password, m_logger);
            try
            {

                ftp.downloadFileFromFTPServer(SourceFileName, TargetFileName, user, password);

            }
            catch (Exception exp)
            {
                m_logger.ErrorException("DownloadFileFromFtp function failed:", exp);
                throw new System.Exception("DownloadFileFromFtp function failed", exp);

            }
            finally
            {
                ftp = null;
            }



            return true;
        }


        /// <summary>
        /// Synchron FTP download - Gunstein Vatnar
        /// </summary>
        /// <param name="SourceFileName"></param>
        /// <param name="TargetFileName"></param>
        /// <param name="ftpserver"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool DownloadFileFromFtpSynchron(string SourceFileName, string TargetFileName, string ftpserver = null, string user = null, string password = null)
        {
            WebClient request = new WebClient();

            string url = "ftp://" + ftpserver + "/" + SourceFileName;

            try
            {
                request.Credentials = new NetworkCredential(user, password);
                byte[] newFileData = request.DownloadData(url);
                FileStream file = File.Create(TargetFileName);
                file.Write(newFileData, 0, newFileData.Length);
                file.Close();
            }
            catch (WebException e)
            {
                m_logger.ErrorException("DownloadFileFromFtpSynchron function failed:", e);
                throw new System.Exception("DownloadFileFromFtpSynchron function failed", e);
            }


            return true;
        }

        public bool CleanUpFTP(string packageName, string ftpserver, string user, string pwd, bool formatZipFile = true)
        {
            string zipFile;
            if (formatZipFile)
            {
                if (packageName.Contains(@"\")) zipFile = packageName.Substring(packageName.LastIndexOf(@"\") + 1); else zipFile = packageName;
                if (!zipFile.ToLower().Contains(".ftp")) zipFile = String.Concat(zipFile, ".zip");
            }
            else zipFile = packageName;
            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(ftpserver, user, pwd, m_logger);
            try
            {

                ftp.DeleteFileOnServer(zipFile);
            }
            catch (Exception exp)
            {
                m_logger.ErrorException("CleanUpFTP function failed:", exp);
                throw new System.Exception("CleanUpFTP function failed", exp);

            }
            finally
            {
                ftp = null;
            }
            return true;
        }





        public bool unpackZipFile(string zipfile, string utpath)
        {
            using (ZipFile zip = ZipFile.Read(zipfile))
                foreach (ZipEntry fil in zip)
                {
                    fil.Extract(utpath, ExtractExistingFileAction.OverwriteSilently);
                }

            return true;
        }
        public bool createZipFile(string infile, string zipFile)
        {
            using (ZipFile zip = new ZipFile())
            {
                m_zipFile = Path.Combine(m_workingDirectory, zipFile);
                zip.AddFile(infile, @"\");

                zip.Comment = "Changelog created " + DateTime.Now.ToString("G");
                zip.Save(m_zipFile);
                zip.Dispose();
            }
            return true;
        }

        private string createWorkingDirectory(string name)
        {
            string tempPath = string.Concat(System.IO.Path.GetTempPath(), name);
            try
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            catch (Exception exp)
            {
                m_logger.ErrorException("createWorkingDirectory function failed:", exp);
                throw new System.Exception("createWorkingDirectory function failed", exp);

            }
            return tempPath;
        }

        private bool CopyFileToDirectory(string fileName, string destination = null)
        {
            string destFileName = null;
            if (destination == null) destFileName = string.Concat(m_workingDirectory, fileName.Substring(fileName.LastIndexOf(@"\"))); else destFileName = destination;
            try
            {

                System.IO.File.Copy(fileName, destFileName);
                return true;
            }
            catch (Exception exp)
            {

                m_logger.ErrorException("CopyFileToDirectory function failed:", exp);
                throw new System.Exception("CopyFileToDirectory function failed", exp);
            }
        }


    }
    public class ftpDownloadEventArgs : EventArgs
    {
        private string m_fileName = "";
        public string fileName
        {
            get
            {
                return m_fileName;
            }
        }



        private string m_errorMessage;
        public string errorMessage
        { get { return m_errorMessage; } }

        private bool m_error;
        public bool error
        {
            get
            {
                return m_error;
            }
        }

        public ftpDownloadEventArgs(string FileName, string errorMsg, bool Error = false)
        {
            m_fileName = FileName;

            m_errorMessage = errorMsg;

            m_error = Error;
        }



    }
    #endregion
#endif
    #endregion
}
