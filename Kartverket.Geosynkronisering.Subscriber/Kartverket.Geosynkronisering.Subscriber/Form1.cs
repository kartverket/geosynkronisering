using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
//using System.Reflection;
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
            txbProviderURL.Text = Properties.Subscriber.Default.DefaultServerURL;
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
                // dgDataset.DataSource = SubscriberDatasetManager.GetAllDataset();
                var list1 = SubscriberDatasetManager.GetAllDataset();


                // To enable sorting in a DataGridView bound to a List
                // See https://www.codeproject.com/Articles/31418/Implementing-a-Sortable-BindingList-Very-Very-Quic
                MySortableBindingList<Dataset> sortableBindingList = new MySortableBindingList<Dataset>(list1); //new SortableList<SubscriberDataset>();

                dgDataset.DataSource = sortableBindingList;
                var col = dgDataset.Columns["DatasetId"];
                dgDataset.Sort(col,ListSortDirection.Ascending);
                
                foreach (DataGridViewColumn column in dgDataset.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
                }

                dgDataset.AutoSize = true;
                //dgDataset.AllowUserToOrderColumns = true; // 20171113-Leg
                // 20171113-Leg:  AutoSizeColumnsMode must be set to None in order to AllowUserToResizeColumns and AllowUserToResizeRows to work.
                //dgDataset.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            }
            catch (Exception ex)
            {
                var errMsg = "Form1_Load failed when opening database:" + SubscriberDatasetManager.GetDatasource();

                logger.Error(ex, errMsg);
                errMsg += "\r\n" + "Remember to copy the database to the the folder:" +
                          AppDomain.CurrentDomain.GetData("APPBASE");
                MessageBox.Show(ex.Message + "\r\n" + errMsg);
            }
        }

        #region SortableBindingList
        /// <summary>
        /// Subclassing BindingList<T> that provides sorting for every property of type T.
        /// See https://www.codeproject.com/Articles/31418/Implementing-a-Sortable-BindingList-Very-Very-Quic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class MySortableBindingList<T> : BindingList<T>
        {

            // reference to the list provided at the time of instantiation
            List<T> originalList;
            ListSortDirection sortDirection;
            PropertyDescriptor sortProperty;

            // function that refereshes the contents
            // of the base classes collection of elements
            Action<MySortableBindingList<T>, List<T>>
                populateBaseList = (a, b) => a.ResetItems(b);

            // a cache of functions that perform the sorting
            // for a given type, property, and sort direction
            static Dictionary<string, Func<List<T>, IEnumerable<T>>>
                cachedOrderByExpressions = new Dictionary<string, Func<List<T>,
                    IEnumerable<T>>>();

            public MySortableBindingList()
            {
                originalList = new List<T>();
            }

            public MySortableBindingList(IEnumerable<T> enumerable)
            {
                originalList = enumerable.ToList();
                populateBaseList(this, originalList);
            }

            public MySortableBindingList(List<T> list)
            {
                originalList = list;
                populateBaseList(this, originalList);
            }

            protected override void ApplySortCore(PropertyDescriptor prop,
                ListSortDirection direction)
            {
                //
                // Look for an appropriate sort method in the cache if not found .
                // Call CreateOrderByMethod to create one. 
                // Apply it to the original list.
                // Notify any bound controls that the sort has been applied.
                //

                sortProperty = prop;

                var orderByMethodName = sortDirection ==
                                        ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
                var cacheKey = typeof(T).GUID + prop.Name + orderByMethodName;

                if (!cachedOrderByExpressions.ContainsKey(cacheKey))
                {
                    CreateOrderByMethod(prop, orderByMethodName, cacheKey);
                }

                ResetItems(cachedOrderByExpressions[cacheKey](originalList).ToList());
                ResetBindings();
                sortDirection = sortDirection == ListSortDirection.Ascending ?
                    ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            private void CreateOrderByMethod(PropertyDescriptor prop,
                string orderByMethodName, string cacheKey)
            {
                //Create a generic method implementation for IEnumerable<T>.
                //Cache it.
                var sourceParameter = Expression.Parameter(typeof(List<T>), "source");
                var lambdaParameter = Expression.Parameter(typeof(T), "lambdaParameter");
                var accesedMember = typeof(T).GetProperty(prop.Name);
                var propertySelectorLambda =
                    Expression.Lambda(Expression.MakeMemberAccess(lambdaParameter,
                        accesedMember), lambdaParameter);
                var orderByMethod = typeof(Enumerable).GetMethods()
                    .Where(a => a.Name == orderByMethodName &&
                                a.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(typeof(T), prop.PropertyType);

                var orderByExpression = Expression.Lambda<Func<List<T>, IEnumerable<T>>>(
                    Expression.Call(orderByMethod,
                        new Expression[] { sourceParameter,
                            propertySelectorLambda }),
                    sourceParameter);

                cachedOrderByExpressions.Add(cacheKey, orderByExpression.Compile());
            }

            protected override void RemoveSortCore()
            {
                ResetItems(originalList);
            }

            private void ResetItems(List<T> items)
            {
                base.ClearItems();
                for (int i = 0; i < items.Count; i++)
                {
                    base.InsertItem(i, items[i]);
                }
            }

            protected override bool SupportsSortingCore
            {
                get
                {
                    // indeed we do
                    return true;
                }
            }

            protected override ListSortDirection SortDirectionCore
            {
                get
                {
                    return sortDirection;
                }
            }

            protected override PropertyDescriptor SortPropertyCore
            {
                get
                {
                    return sortProperty;
                }
            }

            protected override void OnListChanged(ListChangedEventArgs e)
            {
                originalList = base.Items.ToList();
            }
        }
        #endregion
                
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
                var prg = (FeedbackController.Progress)sender;

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
                var prg = (FeedbackController.Progress)sender;

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
                var prg = (FeedbackController.Progress)sender;

                Action action = () => toolStripProgressBar1.Maximum = (int)prg.TotalNumberOfOrders * 100;
                Invoke(action);

                action = () => toolStripProgressBar1.Value = 0;
                Invoke(action);
                statusStrip1.Refresh();

                var logMessage = "Number of orders:" + prg.TotalNumberOfOrders;
                listBoxLog.Items.Add(logMessage);
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
                var prg = (FeedbackController.Progress)sender;

                // For some reason, the progressbar value could seldom be larger that the Maximum
                var value = Math.Min(prg.OrdersProcessedCount, toolStripProgressBar1.Maximum);
                Action action = () => toolStripProgressBar1.Value = value; //prg.OrdersProcessedCount;
                Invoke(action);

                statusStrip1.Refresh();
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

                String version = Application.ProductVersion;
                this.Text += " v." + version; 


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
                logger.Error(ex, "Form1_Load failed:");
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
            try
            {
                cboDatasetName.DisplayMember = "Value";
                cboDatasetName.ValueMember = "Key";
                cboDatasetName.SelectedValue = _currentDatasetId;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message);
            }

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
                _currentDatasetId = ((KeyValuePair<int, string>)cboDatasetName.SelectedItem).Key;
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
                _currentDatasetId = ((KeyValuePair<int, string>)cboDatasetName.SelectedItem).Key;
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
                var subscriberDatasets = (MySortableBindingList<Dataset>)dgDataset.DataSource;
                //var subscriberDatasets = (List<SubscriberDataset>)dgDataset.DataSource;
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
            GetCapabilitiesXml(Url, textBoxUserName.Text, textBoxPassword.Text);
            btnAddSelected.Enabled = dgvProviderDataset.SelectedRows.Count > 0;
        }

        private void btnGetProviderDatasets_Click(object sender, EventArgs e)
        {
            Properties.Subscriber.Default.DefaultServerURL = txbProviderURL.Text;
            Properties.Subscriber.Default.Save();
            try
            {
                Cursor = Cursors.WaitCursor;
                GetCapabilitiesXml(txbProviderURL.Text, textBoxUserName.Text, textBoxPassword.Text);
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
            var blDataset = (IBindingList)dgvProviderDataset.DataSource;

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
            var subscriberDatasets = (MySortableBindingList<Dataset>)dgDataset.DataSource;
            //var subscriberDatasets = (List<SubscriberDataset>)dgDataset.DataSource;


            if (!SubscriberDatasetManager.RemoveDatasets(subscriberDatasets.ToList(), selectedDataset))
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
                var lastChangeIndexSubscriber = (int)dataset.LastIndex;
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
            var datasetId = (int)e.Argument;
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
            List<string> visibleColumns = new List<string>()
            {
                "Name",
                "ProviderDatasetId",
                "TargetNamespace"
            };

            foreach (DataGridViewColumn col in dgvProviderDataset.Columns)
                if (!visibleColumns.Contains(col.Name))
                    col.Visible = false;

            dgvProviderDataset.AutoSize = true;
            dgvProviderDataset.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
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

        private void buttonNew_Click(object sender, EventArgs e)
        {
            SubscriberDatasetManager.AddEmptyDataset();
            InitializeDatasetGrid();
            FillComboBoxDatasetName();
        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnGetProviderDatasets_Click(sender, e);
            }
        }


    }
}