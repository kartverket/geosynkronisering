using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.BL.SchemaMapping;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber
{
    public partial class Form1 : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        private int _lastIndexProvider = 0;
        private int datasetId;

        private SynchController _synchController;

        public Form1()
        {
            InitializeComponent();
            _synchController = new SynchController();
        }

        #region Form events

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                #region GetCapabilities - Hent datasett fra tilbyder.
                // todo
                //txbUser.Cue = "Type username.";
                //txbPassword.Cue = "Type password.";
                #endregion
               
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

                try
                {
                    dgDataset.DataSource = DL.SubscriberDatasetManager.GetAllDataset();
                }
                catch (Exception ex)
                {
                    string errMsg = "Form1_Load failed when opening database:" + DL.SubscriberDatasetManager.GetDatasource();

                    logger.ErrorException(errMsg, ex);
                    errMsg += "\r\n" + "Remeber to copy the databse to the the folder:" + AppDomain.CurrentDomain.GetData("APPBASE").ToString();
                    MessageBox.Show(ex.Message + "\r\n" + errMsg);
                    return;
                }

                // Initialize datasetId
                datasetId = 1;
                
                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();

                // Get subscribers last index from db
                txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetLastIndex(datasetId);
               
                // nlog start
                logger.Info("===== Kartverket.Geosynkronisering.Subscriber Start =====");
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
            var datasetNameList = DL.SubscriberDatasetManager.GetDatasetNames();
            if (datasetNameList.Count > 0)
            {                
                foreach (string name in datasetNameList)
                {
                    cboDatasetName.Items.Add(name);
                }

                cboDatasetName.SelectedIndex = datasetId - 1;
                //if (txtDataset.Text.Length > 0)
                //{
                //    if (cboDatasetName.Items.Contains(txtDataset.Text))
                //    {
                //        cboDatasetName.SelectedIndex = cboDatasetName.Items.IndexOf(txtDataset.Text);
                //    }
                //}
                //else
                //{
                //    cboDatasetName.SelectedIndex = -1;
                //}
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboDatasetName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cboDatasetName_SelectedIndexChanged(object sender, EventArgs e)
        {
            datasetId = cboDatasetName.SelectedIndex + 1;
            // todo: endre defaultdataset til å lagre datasetId.
            //Properties.Settings.Default.defaultDataset = txtDataset.Text;
            //Properties.Settings.Default.Save();
            txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetDataset(datasetId).LastIndex.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("===== Kartverket.Geosynkronisering.Subscriber End =====");
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
                // _localDb.SaveChanges();

                // todo: Hvordan håndteres dette??

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
        }


        // Reset subsrcriber lastChangeIndex
        private void btnResetSubscrLastindex_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to reset the subscriber lastIdex?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _synchController.ResetSubscriberLastIndex(datasetId);

                txbSubscrLastindex.Text = "0";

                dgDataset.DataSource = DL.SubscriberDatasetManager.GetDataset(datasetId);
            }
        }


        private void btnGetLastIndex_Click(object sender, EventArgs e)
        {
            try
            {
                _lastIndexProvider = _synchController.GetLastIndexFromProvider(datasetId);

                txbLastIndex.Text = _lastIndexProvider.ToString();
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


        /// <summary>
        /// Test full Syncronization without executing the other commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestSyncronizationComplete_Click(object sender, EventArgs e)
        {
            // Create new stopwatch
            var stopwatch = Stopwatch.StartNew();

            bool sucsess = _synchController.DoSynchronization(datasetId);

            // Update datagridview control with changes
            dgDataset.DataSource = DL.SubscriberDatasetManager.GetDataset(datasetId);
            dgDataset.Refresh();

            // Stop timing
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            listBoxLog.Items.Add("TestSyncronizationComplete RunTime: " + elapsedTime);
            logger.Info("TestSyncronizationComplete RunTime: {0}", elapsedTime);

            // Display in geoserver openlayers
            toolStripStatusLabel1.Text = "DisplayMap";
            statusStrip1.Refresh();
            var currentDataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
            DisplayMap(epsgCode: "EPSG:32633", datasetName: currentDataset.Name); //DisplayMap(epsgCode: "EPSG:32633");
            toolStripStatusLabel1.Text = "Ready";
            statusStrip1.Refresh();

            Cursor.Current = Cursors.WaitCursor;

            if (!sucsess)
            {
                MessageBox.Show("TestSyncronizationComplete failed!");
            }
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

        #region Schema Transformation (for testing)

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
        #endregion
     
        /// <summary>
        /// Returnerer det som tilbyder støtter av dataset, filter operatorer og objekttyper.
        /// </summary>
        /// <returns></returns>
        private void GetCapabilitiesXml(string url)
        {
            dgvProviderDataset.DataSource = _synchController.GetCapabilitiesProviderDataset(url);
            IDictionary<string, IList<object>> visibleColumns = new Dictionary<string, IList<object>>();
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



        private void btnGetCapabilities_Click(object sender, EventArgs e)
        {
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
            var blDataset = (IBindingList)dgvProviderDataset.DataSource;

            if (!DL.SubscriberDatasetManager.AddDatasets(blDataset, selectedDataset))
            {
                MessageBox.Show(this, "Error saving selected datasets to internal Database.", "Get Provider Datasets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(this, "Saved selected datasets to the internal Database.", "Get Provider Datasets", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgDataset.DataSource = null;

                dgDataset.DataSource = DL.SubscriberDatasetManager.GetDataset(datasetId);
                dgDataset.Refresh();
            }
        }


        private void dgvProviderDataset_SelectionChanged(object sender, EventArgs e)
        {
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }
    }
}
