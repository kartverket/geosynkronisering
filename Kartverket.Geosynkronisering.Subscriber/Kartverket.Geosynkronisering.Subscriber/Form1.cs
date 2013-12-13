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
        private SynchController _synchController;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        private int _lastIndexProvider;
        private int _currentDatasetId;

        public Form1()
        {
            InitializeComponent();
            InitSynchController();
            InitializeDatasetGrid();
            InitializeCurrentDataset();
        }

        private void InitSynchController()
        {
            _synchController = new SynchController();
            _synchController.NewSynchMilestoneReached += new System.EventHandler(this.Progress_OnMilestoneReached);
            _synchController.UpdateLogList += new System.EventHandler(this.Progress_UpdateLogList);
            _synchController.OrderProcessingStart += new System.EventHandler(this.Progress_OrderProcessingStart);
            _synchController.OrderProcessingChange += new System.EventHandler(this.Progress_OrderProcessingChange);
        }

        private void InitializeDatasetGrid()
        {
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
            }
        }

        /// <summary>
        /// Initialize _currentDatasetId
        /// </summary>
        private void InitializeCurrentDataset()
        {
            _currentDatasetId = Properties.Subscriber.Default.DefaultDatasetId;

            // Get subscribers last index from db
            txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetLastIndex(_currentDatasetId);
        }


        /// <summary>
        /// Updates the toolstrip with new milestone info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OnMilestoneReached(object sender, System.EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                var newMilestoneDescription = prg.MilestoneDescription;

                this.toolStripStatusLabel.Text = newMilestoneDescription;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Updates listBoxLog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_UpdateLogList(object sender, System.EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                var newLogListItem = prg.NewLogListItem;

                Action action = () => listBoxLog.Items.Add(newLogListItem);
                this.Invoke(action);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Initialize progressbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OrderProcessingStart(object sender, System.EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                Action action = () => this.progressBar.Maximum = prg.TotalNumberOfOrders;
                this.Invoke(action);

                action = () => this.progressBar.Value = 0;
                this.Invoke(action);
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// Updates progressbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OrderProcessingChange(object sender, System.EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                Action action = () => this.progressBar.Value = prg.OrdersProcessedCount;
                this.Invoke(action);
                //this.Update();
                //Application.DoEvents();
            }
            catch (Exception ex)
            { }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                #region GetCapabilities - Hent datasett fra tilbyder.
                //txbUser.Cue = "Type username.";
                //txbPassword.Cue = "Type password.";
                #endregion

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

                // TODO: For development comment out these 2 lines
                //TabPage page2 = tabControl1.TabPages[1];
                //tabControl1.TabPages.Remove(page2);

                ////page2.Visible = false;
                ////page2.Hide();
                ////page2.Enabled = false;

                UpdateToolStripStatusLabel("Initializing...");

                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();

                // nlog start
                logger.Info("===== Kartverket.Geosynkronisering.Subscriber Start =====");
                listBoxLog.Items.Clear();

                ShowGeoSyncLogo();

                UpdateToolStripStatusLabel("Ready");
            }
            catch (Exception ex)
            {
                logger.ErrorException("Form1_Load failed:", ex);
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void ShowGeoSyncLogo()
        {
            string path = System.Environment.CurrentDirectory;
            string fileName = path.Substring(0, path.LastIndexOf("bin")) + "Images" + "\\Geosynk.ico";
            webBrowser1.Navigate(fileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("===== Kartverket.Geosynkronisering.Subscriber End =====");
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

                cboDatasetName.SelectedIndex = _currentDatasetId - 1;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboDatasetName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cboDatasetName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentDatasetId = cboDatasetName.SelectedIndex + 1;
            Properties.Subscriber.Default.DefaultDatasetId = _currentDatasetId;
            Properties.Subscriber.Default.Save();
            txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetDataset(_currentDatasetId).LastIndex.ToString();
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
                var subscriberDatasets = (List<DL.SubscriberDataset>)dgDataset.DataSource;
                foreach (var subscriberDataset in subscriberDatasets)
                {
                    DL.SubscriberDatasetManager.UpdateDataset(subscriberDataset);
                }

                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        // Reset subsrcriber lastChangeIndex
        private void btnResetSubscrLastindex_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to reset the subscriber lastIdex?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _synchController.ResetSubscriberLastIndex(_currentDatasetId);

                txbSubscrLastindex.Text = "0";

                InitializeDatasetGrid();
            }
        }


        private void btnGetLastIndex_Click(object sender, EventArgs e)
        {
            try
            {
                _lastIndexProvider = _synchController.GetLastIndexFromProvider(_currentDatasetId);

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

                InitializeDatasetGrid();
                FillComboBoxDatasetName();
            }
        }

        /// <summary>
        /// Delete selected dataset from database
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            IList<int> selectedDataset = new List<int>();
            foreach (DataGridViewRow dgr in dgDataset.SelectedRows)
            {
                selectedDataset.Add(dgr.Index);
            }
            var subscriberDatasets = (List<DL.SubscriberDataset>)dgDataset.DataSource;

            if (!DL.SubscriberDatasetManager.RemoveDatasets(subscriberDatasets, selectedDataset))
            {
                MessageBox.Show(this, "Error removing  selected datasets to internal Database.", "Remove Datasets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(this, "Saved after removing selected datasets to the internal Database.", "Remove Datasets", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgDataset.DataSource = null;

                InitializeDatasetGrid();
                FillComboBoxDatasetName();
            }


        }
        private void dgvProviderDataset_SelectionChanged(object sender, EventArgs e)
        {
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }


        /// <summary>
        /// Handles the Click event of the btnOfflineSync control.
        /// Starts Offline sync.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnOfflineSync_Click(object sender, EventArgs e)
        {
            try
            {
                string path = System.Environment.CurrentDirectory;
                //string fileName = "";
                string zipFile = "";

                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                //openFileDialog1.InitialDirectory = path.Substring(0, path.LastIndexOf("bin")) +
                //                                   @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping"; //System.Environment.CurrentDirectory;
                openFileDialog1.Filter = "zip files (*.zip)|*.zip|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.Title = "Select file for Offline Syncronization";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        zipFile = openFileDialog1.FileName;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not open file. Original error: " + ex.Message);
                    }
                  
                
                }
                else
                {
                    return;
                }
                
                //string zipFile = "";
                //// zipFile = @"C:\Users\leg\AppData\Local\Temp\abonnent\6fa6e29d-e978-4ba5-a660-b7f355b233ef.zip";
                //zipFile = @"C:\Users\b543836\AppData\Local\Temp\abonnent\6fa6e29d-e978-4ba5-a660-b7f355b233ef.zip";

                this.tabControl1.SelectTab(0);
                bool status = _synchController.TestOfflineSyncronizationComplete(zipFile, _currentDatasetId);

                if (status)
                {
                    // Oppdaterer dgDataset
                    dgDataset.DataSource = DL.SubscriberDatasetManager.GetAllDataset();
                    // update subscribers last index from db
                    txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetLastIndex(_currentDatasetId);
                    
                    if (_synchController.TransactionsSummary != null)
                    {

                        var logMessage = "Offline Syncronization Transaction summary:";
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalInserted: " + _synchController.TransactionsSummary.TotalInserted.ToString();
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalUpdated: " + _synchController.TransactionsSummary.TotalUpdated.ToString();
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalDeleted: " + _synchController.TransactionsSummary.TotalDeleted.ToString();
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalReplaced: " + _synchController.TransactionsSummary.TotalReplaced.ToString();
                        listBoxLog.Items.Add(logMessage);

                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "btnOfflneSync_Click");

                //   throw;
            }
        }

        protected void ShowError(string msg)
        {
            Hourglass(false);
            MessageBox.Show(msg);
        }


        /// <summary>
        /// Test full Syncronization without executing the other commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSyncronizationComplete_Click(object sender, EventArgs e)
        {
            ShowGeoSyncLogo();

            SetSynchButtonInactive();

            Hourglass(true);

            listBoxLog.Items.Clear();

            var logMessage = "Syncronization Start";
            listBoxLog.Items.Add(logMessage);
            logger.Info(logMessage);

            SynchronizeAsThread(_currentDatasetId);
        }

        public void SynchronizeAsThread(int datasetId)
        {
            var _backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _backgroundWorker.DoWork += bw_DoSynchronization;
            _backgroundWorker.RunWorkerAsync(datasetId);
            _backgroundWorker.RunWorkerCompleted += bw_SynchronizeComplete;
        }

        /// <summary>
        /// Synchronize a dataset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bw_DoSynchronization(object sender, DoWorkEventArgs e)
        {
            _synchController.InitTransactionsSummary();
            var datasetId = (int)e.Argument;
            _synchController.DoSynchronization(datasetId);
        }

        void bw_SynchronizeComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Update Dataset-datagridview control with changes
            InitializeDatasetGrid();

            // update subscribers last index from db
            txbSubscrLastindex.Text = DL.SubscriberDatasetManager.GetLastIndex(_currentDatasetId);

            // Display in geoserver openlayers
            UpdateToolStripStatusLabel("Display Map");

            var currentDataset = DL.SubscriberDatasetManager.GetDataset(_currentDatasetId);

            if (currentDataset.ClientWfsUrl.Contains("geoserver"))
            {
                // only for GeoServer
                DisplayMap(epsgCode: "EPSG:32633", datasetName: currentDataset.Name); //DisplayMap(epsgCode: "EPSG:32633"); 
            }
            

            UpdateToolStripStatusLabel("Ready");
            SetSynchButtonActive();
            Hourglass(false);
            if (_synchController.TransactionsSummary != null)
            {

                var logMessage = "Syncronization Transaction summary:";
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalInserted: " + _synchController.TransactionsSummary.TotalInserted.ToString();
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalUpdated: " + _synchController.TransactionsSummary.TotalUpdated.ToString();
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalDeleted: " + _synchController.TransactionsSummary.TotalDeleted.ToString();
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalReplaced: " + _synchController.TransactionsSummary.TotalReplaced.ToString();
                listBoxLog.Items.Add(logMessage);

            }

        }

        public void SetSynchButtonActive()
        {
            btnTestSyncronizationComplete.Enabled = true;
        }

        public void SetSynchButtonInactive()
        {
            btnTestSyncronizationComplete.Enabled = false;
        }

        private void UpdateToolStripStatusLabel(string message)
        {
            toolStripStatusLabel.Text = message;
            statusStrip1.Refresh();
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


        #region Schema Transformation (for testing)

        /// <summary>
        /// Test Schema transformation - using hardcoded mapping file and selecttable xml-file
        /// Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
        /// </summary>
        private void TestSchemaTransformSimplifyChangelog()
        {
            try
            {
                string path = System.Environment.CurrentDirectory;
                string fileName = "";

                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = path.Substring(0, path.LastIndexOf("bin")) +
                                                   @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping"; //System.Environment.CurrentDirectory;
                openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.Title = "Select file to transform";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        fileName = openFileDialog1.FileName;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not open file. Original error: " + ex.Message);
                    }
                
                }
                else
                {
                    return;
                }



                
                //string fileName = path.Substring(0, path.LastIndexOf("bin")) + @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping" + @"\MixFeaturetypes.Ar5-Insert.xml"; //@"\_wfsT-test1.xml";

                // Test empty changelog
                // fileName = path.Substring(0, path.LastIndexOf("bin")) + "SchemaMapping" + @"\ar5-tom-07a8e3ef-7315-409f-862d-6417b4275368.xml";

                string mappingFileName = path.Substring(0, path.LastIndexOf("bin")) + @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping" + @"\ar5FeatureType-mapping-file.xml";

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
                    string newFileName = path.Substring(0, path.LastIndexOf("bin")) + @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping" + @"\New_wfsT-test1.xml";
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

            if (!hostWms.Contains("geoserver"))
            {
                return; // return if not GeoServer
            }

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


        protected void Hourglass(bool Show)
        {
            if (Show == true)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            }
            else
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
            return;
        }






    }
}
