using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.BL.SchemaMapping;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber
{
    public partial class Form1 : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        private int _currentDatasetId;
        private long _lastIndexProvider;
        private SynchController _synchController;


        private bool comboFill;

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
            _synchController.NewSynchMilestoneReached += Progress_OnMilestoneReached;
            _synchController.UpdateLogList += Progress_UpdateLogList;
            _synchController.OrderProcessingStart += Progress_OrderProcessingStart;
            _synchController.OrderProcessingChange += Progress_OrderProcessingChange;
        }

        private void InitializeDatasetGrid()
        {
            try
            {
                List<string> invisibleColumns = new List<string>()
                {
                    //"DatasetId",
                    "AbortedEndIndex",
                    "AbortedTransaction",
                    "AbortedChangelogPath",
                    "AbortedChangelogId",
                    //"UserName",
                    //"Password"
                };
#if DEBUG
                dgDataset.DataSource = SubscriberDatasetManager.GetAllDataset();
#else
                dgDataset.DataSource = SubscriberDatasetManager.GetAllDataset();
                foreach (var invisibleColumn in invisibleColumns)
                {
                    dgDataset.Columns[invisibleColumn].Visible = false;
                };
#endif


            }
            catch (Exception ex)
            {
                var errMsg = "Form1_Load failed when opening database:" + SubscriberDatasetManager.GetDatasource();

                logger.ErrorException(errMsg, ex);
                errMsg += "\r\n" + "Remeber to copy the databse to the the folder:" +
                          AppDomain.CurrentDomain.GetData("APPBASE");
                MessageBox.Show(ex.Message + "\r\n" + errMsg);
            }
        }

        /// <summary>
        ///     Initialize _currentDatasetId
        /// </summary>
        private void InitializeCurrentDataset()
        {
            _currentDatasetId = Properties.Subscriber.Default.DefaultDatasetId;

            // Get subscribers last index from db
            txbSubscrLastindex.Text = SubscriberDatasetManager.GetLastIndex(_currentDatasetId);
        }


        /// <summary>
        ///     Updates the toolstrip with new milestone info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OnMilestoneReached(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress) sender;

                var newMilestoneDescription = prg.MilestoneDescription;

                toolStripStatusLabel.Text = newMilestoneDescription;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///     Updates listBoxLog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_UpdateLogList(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress) sender;

                var newLogListItem = prg.NewLogListItem;

                Action action = () => listBoxLog.Items.Add(newLogListItem);
                Invoke(action);
                // Scroll down automatically
                action = () => listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                Invoke(action);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///     Initialize progressbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OrderProcessingStart(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress) sender;

                Action action = () => progressBar.Maximum = (int) prg.TotalNumberOfOrders;
                Invoke(action);

                action = () => progressBar.Value = 0;
                Invoke(action);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        ///     Updates progressbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Progress_OrderProcessingChange(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress) sender;

                // For some reason, the progressbar value could seldom be larger that the Maximum
                var value = Math.Min(prg.OrdersProcessedCount, progressBar.Maximum);
                Action action = () => progressBar.Value = value; //prg.OrdersProcessedCount;
                Invoke(action);


                //this.Update();
                //Application.DoEvents();
            }
            catch (Exception ex)
            {
            }
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

#if DEBUG
#else
                TabPage page2 = tabControl1.TabPages[1];
                tabControl1.TabPages.Remove(page2);
#endif
                ////page2.Visible = false;
                ////page2.Hide();
                ////page2.Enabled = false;


                UpdateToolStripStatusLabel("Initializing...");

                // Fill the cboDatasetName comboBox with the dataset names
                FillComboBoxDatasetName();

                // nlog start
                logger.Info("===== Kartverket.Geosynkronisering.Subscriber Start =====");
                listBoxLog.Items.Clear();
                UpdateToolStripStatusLabel("Ready");
            }
            catch (Exception ex)
            {
                logger.ErrorException("Form1_Load failed:", ex);
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("===== Kartverket.Geosynkronisering.Subscriber End =====");
        }


        /// <summary>
        ///     Occurs after a data-binding operation has finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDataset_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Set the primary ket field to ReadOnly
            dgDataset.Columns["DatasetId"].ReadOnly = true;
        }

        /// <summary>
        ///     Fills the cboDatasetName comboBox with the dataset names
        /// </summary>
        private void FillComboBoxDatasetName()
        {
            //cboDatasetName.Items.Clear();  
            comboFill = true;
            var datasetNameList = SubscriberDatasetManager.GetDatasetNamesAsDictionary();
            cboDatasetName.DataSource = new BindingSource(datasetNameList, null);
            cboDatasetName.DisplayMember = "Value";
            cboDatasetName.ValueMember = "Key";
            cboDatasetName.SelectedValue = _currentDatasetId;
            //if (datasetNameList.Count > 0)
            //{
            //    foreach (string name in datasetNameList)
            //    {
            //        cboDatasetName.Items.Add(name);
            //    }


            //cboDatasetName.SelectedIndex = _currentDatasetId - 1;
            //}
            comboFill = false;
        }

        /// <summary>
        ///     Handles the SelectedIndexChanged event of the cboDatasetName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void cboDatasetName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboFill)
                _currentDatasetId = ((KeyValuePair<int, string>) cboDatasetName.SelectedItem).Key;
                    //cboDatasetName.SelectedIndex + 1;

            Properties.Subscriber.Default.DefaultDatasetId = _currentDatasetId;
            Properties.Subscriber.Default.Save();
            try
            {
                txbSubscrLastindex.Text = SubscriberDatasetManager.GetDataset(_currentDatasetId).LastIndex.ToString();
            }
            catch (Exception ex)
            {
                cboDatasetName.SelectedIndex = 0;
                _currentDatasetId = ((KeyValuePair<int, string>) cboDatasetName.SelectedItem).Key;
                Properties.Subscriber.Default.DefaultDatasetId = _currentDatasetId;
                Properties.Subscriber.Default.Save();
            }
        }

        /// <summary>
        ///     Save settings database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                var subscriberDatasets = (List<SubscriberDataset>) dgDataset.DataSource;
                foreach (var subscriberDataset in subscriberDatasets)
                {
                    SubscriberDatasetManager.UpdateDataset(subscriberDataset);
                }

                // Fill the cboDatasetName comboBox with the dataset names
                cboDatasetName.SelectedIndex = 0;
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
            var result = MessageBox.Show("Do you really want to reset the subscriber lastIdex?", "Warning",
                MessageBoxButtons.YesNo);
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
            var Url = txbProviderURL.Text;
            GetCapabilitiesXml(Url,textBoxUserName.Text, textBoxPassword.Text);
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }

        private void btnGetProviderDatasets_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                GetCapabilitiesXml(txbProviderURL.Text,textBoxUserName.Text, textBoxPassword.Text);
            }
            catch (Exception ex)
            {
                Cursor = DefaultCursor;
                MessageBox.Show("Error : " + ex.Message);
            }
            finally
            {
                Cursor = DefaultCursor;
            }
        }

        private void btnAddSelected_Click(object sender, EventArgs e)
        {
            IList<int> selectedDataset = new List<int>();
            foreach (DataGridViewRow dgr in dgvProviderDataset.SelectedRows)
            {
                selectedDataset.Add(dgr.Index);
            }
            var blDataset = (IBindingList) dgvProviderDataset.DataSource;

            if (!SubscriberDatasetManager.AddDatasets(blDataset, selectedDataset, txbProviderURL.Text, textBoxUserName.Text, textBoxPassword.Text))
            {
                MessageBox.Show(this, "Error saving selected datasets to internal Database.", "Get Provider Datasets",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(this, "Saved selected datasets to the internal Database.", "Get Provider Datasets",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgDataset.DataSource = null;

                InitializeDatasetGrid();
                FillComboBoxDatasetName();
            }
        }

        /// <summary>
        ///     Delete selected dataset from database
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            IList<int> selectedDataset = new List<int>();
            foreach (DataGridViewRow dgr in dgDataset.SelectedRows)
            {
                selectedDataset.Add(dgr.Index);
            }
            var subscriberDatasets = (List<SubscriberDataset>) dgDataset.DataSource;

            if (!SubscriberDatasetManager.RemoveDatasets(subscriberDatasets, selectedDataset))
            {
                MessageBox.Show(this, "Error removing selected datasets from internal Database.", "Remove Datasets",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(this, "Saved after removing selected datasets from internal Database.",
                    "Remove Datasets", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgDataset.DataSource = null;

                InitializeDatasetGrid();
                cboDatasetName.SelectedIndex = 0;
                FillComboBoxDatasetName();
            }
        }

        private void dgvProviderDataset_SelectionChanged(object sender, EventArgs e)
        {
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }


        /// <summary>
        ///     Handles the Click event of the btnOfflineSync control.
        ///     Starts Offline sync.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnOfflineSync_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Environment.CurrentDirectory;
                //string fileName = "";
                var zipFile = "";

                var openFileDialog1 = new OpenFileDialog();
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

                var dataset = SubscriberDatasetManager.GetDataset(_currentDatasetId);
                var lastChangeIndexSubscriber = (int) dataset.LastIndex;
                if (lastChangeIndexSubscriber > 0)
                {
                    var buttons = MessageBoxButtons.YesNo;
                    var result =
                        MessageBox.Show(
                            "TestOfflineSyncronizationComplete could fail if lastChangeIndexSubscriber > 0. Do you want to continue?",
                            "", buttons);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                tabControl1.SelectTab(0);
                var status = _synchController.DoSyncronizationOffline(zipFile, _currentDatasetId, 0);

                if (status)
                {
                    // Oppdaterer dgDataset
                    dgDataset.DataSource = SubscriberDatasetManager.GetAllDataset();
                    // update subscribers last index from db
                    txbSubscrLastindex.Text = SubscriberDatasetManager.GetLastIndex(_currentDatasetId);

                    if (_synchController.TransactionsSummary != null)
                    {
                        var logMessage = "Offline Syncronization Transaction summary:";
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalInserted: " + _synchController.TransactionsSummary.TotalInserted;
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalUpdated: " + _synchController.TransactionsSummary.TotalUpdated;
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalDeleted: " + _synchController.TransactionsSummary.TotalDeleted;
                        listBoxLog.Items.Add(logMessage);
                        logMessage = "TotalReplaced: " + _synchController.TransactionsSummary.TotalReplaced;
                        listBoxLog.Items.Add(logMessage);
                        // Scroll down automatically
                        listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
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
        ///     Test full Syncronization without executing the other commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSyncronizationComplete_Click(object sender, EventArgs e)
        {
            SetSynchButtonInactive();

            Hourglass(true);

            listBoxLog.Items.Clear();

            var logMessage = "Syncronization Start";
            listBoxLog.Items.Add(logMessage);
            logger.Info(logMessage);

            SynchronizeAsThread(_currentDatasetId);
        }

        public void SynchronizeAsThread()
        {
            foreach (var datasetId in SubscriberDatasetManager.GetListOfDatasetIDs())
            {
                var _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += bw_DoSynchronization;
                _backgroundWorker.RunWorkerAsync(datasetId);
                _backgroundWorker.RunWorkerCompleted += bw_SynchronizeComplete;
            }
        }

        public void SynchronizeAsThread(int datasetId)
        {
            var _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _backgroundWorker.DoWork += bw_DoSynchronization;
            _backgroundWorker.RunWorkerAsync(datasetId);
            _backgroundWorker.RunWorkerCompleted += bw_SynchronizeComplete;
        }

        /// <summary>
        ///     Synchronize a dataset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bw_DoSynchronization(object sender, DoWorkEventArgs e)
        {
            _synchController.InitTransactionsSummary();
            var datasetId = (int) e.Argument;
            _synchController.DoSynchronization(datasetId);
        }

        private void bw_SynchronizeComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Update Dataset-datagridview control with changes
            InitializeDatasetGrid();

            // update subscribers last index from db
            txbSubscrLastindex.Text = SubscriberDatasetManager.GetLastIndex(_currentDatasetId);

            // Display in geoserver openlayers
            UpdateToolStripStatusLabel("Display Map");

            UpdateToolStripStatusLabel("Ready");
            SetSynchButtonActive();
            Hourglass(false);
            if (_synchController.TransactionsSummary != null)
            {
                var logMessage = "Syncronization Transaction summary:";
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalInserted: " + _synchController.TransactionsSummary.TotalInserted;
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalUpdated: " + _synchController.TransactionsSummary.TotalUpdated;
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalDeleted: " + _synchController.TransactionsSummary.TotalDeleted;
                listBoxLog.Items.Add(logMessage);
                logMessage = "TotalReplaced: " + _synchController.TransactionsSummary.TotalReplaced;
                listBoxLog.Items.Add(logMessage);
                // Scroll down automatically
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
        }

        public void SetSynchButtonActive()
        {
            btnTestSyncronizationComplete.Enabled = true;
            btnTestSyncronizationAll.Enabled = true;
        }

        public void SetSynchButtonInactive()
        {
            btnTestSyncronizationComplete.Enabled = false;
            btnTestSyncronizationAll.Enabled = false;
        }

        private void UpdateToolStripStatusLabel(string message)
        {
            toolStripStatusLabel.Text = message;
            statusStrip1.Refresh();
        }

        /// <summary>
        ///     Handles the Click event of the btnSimplify control.
        ///     Testing Schema transformation:
        ///     Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void btnSimplify_Click(object sender, EventArgs e)
        {
            // Mapping from the nested structure of one or more simple features to the simple features for GeoServer
            TestSchemaTransformSimplifyChangelog();
        }

        #region Schema Transformation (for testing)

        /// <summary>
        ///     Test Schema transformation - using hardcoded mapping file and selecttable xml-file
        ///     Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
        /// </summary>
        private void TestSchemaTransformSimplifyChangelog()
        {
            try
            {
                var path = Environment.CurrentDirectory;
                var fileName = "";

                var openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = path.Substring(0, path.LastIndexOf("bin")) +
                                                   @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping";
                    //System.Environment.CurrentDirectory;
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

                var mappingFileName = path.Substring(0, path.LastIndexOf("bin")) +
                                      @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping" +
                                      @"\ar5FeatureType-mapping-file.xml";

                // load the changelog XML document from file
                // XElement changeLog = XElement.Load(fileName);

                // Set up GeoServer mapping
                var geoserverMap = new GeoserverMapping();
                geoserverMap.NamespaceUri = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Arealressurs/4.5";
                geoserverMap.SetXmlMappingFile(mappingFileName);

                // Simplify
                var newChangeLog = geoserverMap.Simplify(fileName);
                if (newChangeLog != null)
                {
                    var newFileName = path.Substring(0, path.LastIndexOf("bin")) +
                                      @"..\Kartverket.Geosynkronisering.Subscriber.BL\SchemaMapping" +
                                      @"\New_wfsT-test1.xml";
                    newChangeLog.Save(newFileName);

                    var msg = "Source: " + fileName;
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
        ///     Returnerer det som tilbyder støtter av dataset, filter operatorer og objekttyper.
        /// </summary>
        /// <returns></returns>
        private void GetCapabilitiesXml(string url, string UserName, string Password)
        {
            dgvProviderDataset.DataSource = _synchController.GetCapabilitiesProviderDataset(url, UserName, Password);
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
                    col.AutoSizeMode = (DataGridViewAutoSizeColumnMode) columnFormat[2];
                }
            }
            dgvProviderDataset.AutoSize = true;
            dgvProviderDataset.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        protected void Hourglass(bool Show)
        {
            if (Show)
            {
                Cursor.Current = Cursors.WaitCursor;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnTestSyncronizationAll_Click(object sender, EventArgs e)
        {
            SetSynchButtonInactive();

            Hourglass(true);

            listBoxLog.Items.Clear();

            var logMessage = "Syncronization Start";
            listBoxLog.Items.Add(logMessage);
            logger.Info(logMessage);

            SynchronizeAsThread();
        }

        private void dgDataset_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 15 && e.Value != null)
            {
                e.Value = new String('*', 6);
            }
        }
    }
}