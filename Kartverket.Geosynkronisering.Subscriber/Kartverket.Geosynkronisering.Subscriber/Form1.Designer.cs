namespace Kartverket.Geosynkronisering.Subscriber
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonSave = new System.Windows.Forms.Button();
            this.btnDeleteSelected = new System.Windows.Forms.Button();
            this.btnOfflineSync = new System.Windows.Forms.Button();
            this.btnSimplify = new System.Windows.Forms.Button();
            this.txbLastIndex = new System.Windows.Forms.TextBox();
            this.btnTestSyncronizationComplete = new System.Windows.Forms.Button();
            this.btnResetSubscrLastindex = new System.Windows.Forms.Button();
            this.txbSubscrLastindex = new System.Windows.Forms.TextBox();
            this.txtLimitNumberOfFeatures = new System.Windows.Forms.TextBox();
            this.cboDatasetName = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnTestSyncronizationAll = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelLimitNumberOfFeatures = new System.Windows.Forms.Label();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnGetCapabilities = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGetLastIndex = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.buttonUpdateSelectedCells = new System.Windows.Forms.Button();
            this.buttonNew = new System.Windows.Forms.Button();
            this.dgDataset = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnAddSelected = new System.Windows.Forms.Button();
            this.dgvProviderDataset = new System.Windows.Forms.DataGridView();
            this.txbProviderURL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGetProviderDatasets = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxCellValue = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProviderDataset)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(812, 629);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.toolTip1.SetToolTip(this.buttonSave, "Save changes");
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteSelected.Location = new System.Drawing.Point(892, 629);
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteSelected.TabIndex = 2;
            this.btnDeleteSelected.Text = "Delete";
            this.toolTip1.SetToolTip(this.btnDeleteSelected, "Delete selected dataset");
            this.btnDeleteSelected.UseVisualStyleBackColor = true;
            this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
            // 
            // btnOfflineSync
            // 
            this.btnOfflineSync.Location = new System.Drawing.Point(262, 31);
            this.btnOfflineSync.Name = "btnOfflineSync";
            this.btnOfflineSync.Size = new System.Drawing.Size(75, 23);
            this.btnOfflineSync.TabIndex = 17;
            this.btnOfflineSync.Text = "Offline sync";
            this.toolTip1.SetToolTip(this.btnOfflineSync, "Offline syncronization, input is a changelog zip-file.");
            this.btnOfflineSync.UseVisualStyleBackColor = true;
            this.btnOfflineSync.Click += new System.EventHandler(this.btnOfflineSync_Click);
            // 
            // btnSimplify
            // 
            this.btnSimplify.Location = new System.Drawing.Point(3, 31);
            this.btnSimplify.Name = "btnSimplify";
            this.btnSimplify.Size = new System.Drawing.Size(93, 44);
            this.btnSimplify.TabIndex = 16;
            this.btnSimplify.Text = "Schema Transformation";
            this.toolTip1.SetToolTip(this.btnSimplify, "Mapping from the nested structure of one or more simple features to the simple fe" +
        "atures for GeoServer");
            this.btnSimplify.UseVisualStyleBackColor = true;
            this.btnSimplify.Click += new System.EventHandler(this.btnSimplify_Click);
            // 
            // txbLastIndex
            // 
            this.txbLastIndex.Location = new System.Drawing.Point(85, 3);
            this.txbLastIndex.Name = "txbLastIndex";
            this.txbLastIndex.Size = new System.Drawing.Size(76, 20);
            this.txbLastIndex.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txbLastIndex, "Provider lastIndex");
            // 
            // btnTestSyncronizationComplete
            // 
            this.btnTestSyncronizationComplete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTestSyncronizationComplete.Location = new System.Drawing.Point(4, 628);
            this.btnTestSyncronizationComplete.Name = "btnTestSyncronizationComplete";
            this.btnTestSyncronizationComplete.Size = new System.Drawing.Size(132, 23);
            this.btnTestSyncronizationComplete.TabIndex = 19;
            this.btnTestSyncronizationComplete.Text = "Synchronize dataset";
            this.toolTip1.SetToolTip(this.btnTestSyncronizationComplete, "Test complete syncronization");
            this.btnTestSyncronizationComplete.UseVisualStyleBackColor = true;
            this.btnTestSyncronizationComplete.Click += new System.EventHandler(this.btnSyncronizationComplete_Click);
            // 
            // btnResetSubscrLastindex
            // 
            this.btnResetSubscrLastindex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnResetSubscrLastindex.Location = new System.Drawing.Point(310, 628);
            this.btnResetSubscrLastindex.Name = "btnResetSubscrLastindex";
            this.btnResetSubscrLastindex.Size = new System.Drawing.Size(160, 23);
            this.btnResetSubscrLastindex.TabIndex = 20;
            this.btnResetSubscrLastindex.Text = "Reset subscriber lastIndex";
            this.toolTip1.SetToolTip(this.btnResetSubscrLastindex, "Reset subscriber lastIndex");
            this.btnResetSubscrLastindex.UseVisualStyleBackColor = true;
            this.btnResetSubscrLastindex.Click += new System.EventHandler(this.btnResetSubscrLastindex_Click);
            // 
            // txbSubscrLastindex
            // 
            this.txbSubscrLastindex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txbSubscrLastindex.Location = new System.Drawing.Point(476, 631);
            this.txbSubscrLastindex.Name = "txbSubscrLastindex";
            this.txbSubscrLastindex.ReadOnly = true;
            this.txbSubscrLastindex.Size = new System.Drawing.Size(100, 20);
            this.txbSubscrLastindex.TabIndex = 21;
            this.toolTip1.SetToolTip(this.txbSubscrLastindex, "subscriber lastIndex");
            // 
            // txtLimitNumberOfFeatures
            // 
            this.txtLimitNumberOfFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLimitNumberOfFeatures.Location = new System.Drawing.Point(692, 631);
            this.txtLimitNumberOfFeatures.Name = "txtLimitNumberOfFeatures";
            this.txtLimitNumberOfFeatures.Size = new System.Drawing.Size(52, 20);
            this.txtLimitNumberOfFeatures.TabIndex = 25;
            this.txtLimitNumberOfFeatures.Text = "-1";
            this.toolTip1.SetToolTip(this.txtLimitNumberOfFeatures, "For debugging limit number of objects");
            this.txtLimitNumberOfFeatures.Visible = false;
            // 
            // cboDatasetName
            // 
            this.cboDatasetName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboDatasetName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDatasetName.FormattingEnabled = true;
            this.cboDatasetName.Location = new System.Drawing.Point(142, 631);
            this.cboDatasetName.Name = "cboDatasetName";
            this.cboDatasetName.Size = new System.Drawing.Size(163, 21);
            this.cboDatasetName.Sorted = true;
            this.cboDatasetName.TabIndex = 26;
            this.toolTip1.SetToolTip(this.cboDatasetName, "Current dataset");
            this.cboDatasetName.SelectedIndexChanged += new System.EventHandler(this.cboDatasetName_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 690);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1065, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1059, 681);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.btnTestSyncronizationAll);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.progressBar);
            this.tabPage1.Controls.Add(this.cboDatasetName);
            this.tabPage1.Controls.Add(this.txtLimitNumberOfFeatures);
            this.tabPage1.Controls.Add(this.labelLimitNumberOfFeatures);
            this.tabPage1.Controls.Add(this.listBoxLog);
            this.tabPage1.Controls.Add(this.txbSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnResetSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnTestSyncronizationComplete);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1051, 655);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            // 
            // btnTestSyncronizationAll
            // 
            this.btnTestSyncronizationAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestSyncronizationAll.Location = new System.Drawing.Point(938, 628);
            this.btnTestSyncronizationAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnTestSyncronizationAll.Name = "btnTestSyncronizationAll";
            this.btnTestSyncronizationAll.Size = new System.Drawing.Size(107, 23);
            this.btnTestSyncronizationAll.TabIndex = 29;
            this.btnTestSyncronizationAll.Text = "Synchronize all";
            this.btnTestSyncronizationAll.UseVisualStyleBackColor = true;
            this.btnTestSyncronizationAll.Click += new System.EventHandler(this.btnTestSyncronizationAll_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 848);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Synchronize progress:";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(7, 864);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1041, 23);
            this.progressBar.TabIndex = 27;
            // 
            // labelLimitNumberOfFeatures
            // 
            this.labelLimitNumberOfFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLimitNumberOfFeatures.AutoSize = true;
            this.labelLimitNumberOfFeatures.Location = new System.Drawing.Point(588, 633);
            this.labelLimitNumberOfFeatures.Name = "labelLimitNumberOfFeatures";
            this.labelLimitNumberOfFeatures.Size = new System.Drawing.Size(98, 13);
            this.labelLimitNumberOfFeatures.TabIndex = 24;
            this.labelLimitNumberOfFeatures.Text = "Limit no. of objects:";
            this.labelLimitNumberOfFeatures.Visible = false;
            // 
            // listBoxLog
            // 
            this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.Location = new System.Drawing.Point(0, 0);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(1054, 615);
            this.listBoxLog.TabIndex = 22;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.tableLayoutPanel5);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1051, 655);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Detailed test";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 56.1828F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43.8172F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1045, 649);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.btnGetCapabilities, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnGetLastIndex, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSimplify, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnOfflineSync, 1, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1039, 192);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // btnGetCapabilities
            // 
            this.btnGetCapabilities.Location = new System.Drawing.Point(3, 3);
            this.btnGetCapabilities.Name = "btnGetCapabilities";
            this.btnGetCapabilities.Size = new System.Drawing.Size(90, 22);
            this.btnGetCapabilities.TabIndex = 0;
            this.btnGetCapabilities.Text = "GetCapabilities";
            this.btnGetCapabilities.UseVisualStyleBackColor = true;
            this.btnGetCapabilities.Click += new System.EventHandler(this.btnGetCapabilities_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txbLastIndex, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(780, 31);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(164, 22);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "lastIndex:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnGetLastIndex
            // 
            this.btnGetLastIndex.Location = new System.Drawing.Point(780, 3);
            this.btnGetLastIndex.Name = "btnGetLastIndex";
            this.btnGetLastIndex.Size = new System.Drawing.Size(92, 22);
            this.btnGetLastIndex.TabIndex = 3;
            this.btnGetLastIndex.Text = "GetLastIndex";
            this.btnGetLastIndex.UseVisualStyleBackColor = true;
            this.btnGetLastIndex.Click += new System.EventHandler(this.btnGetLastIndex_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBoxCellValue);
            this.tabPage3.Controls.Add(this.buttonUpdateSelectedCells);
            this.tabPage3.Controls.Add(this.buttonNew);
            this.tabPage3.Controls.Add(this.btnDeleteSelected);
            this.tabPage3.Controls.Add(this.buttonSave);
            this.tabPage3.Controls.Add(this.dgDataset);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1051, 655);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Datasets";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateSelectedCells
            // 
            this.buttonUpdateSelectedCells.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateSelectedCells.Location = new System.Drawing.Point(257, 628);
            this.buttonUpdateSelectedCells.Name = "buttonUpdateSelectedCells";
            this.buttonUpdateSelectedCells.Size = new System.Drawing.Size(89, 20);
            this.buttonUpdateSelectedCells.TabIndex = 4;
            this.buttonUpdateSelectedCells.Text = "Update Cells";
            this.toolTip1.SetToolTip(this.buttonUpdateSelectedCells, "Update multiple columns");
            this.buttonUpdateSelectedCells.UseVisualStyleBackColor = true;
            this.buttonUpdateSelectedCells.Click += new System.EventHandler(this.buttonUpdateSelectedCells_Click);
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNew.Location = new System.Drawing.Point(973, 629);
            this.buttonNew.Margin = new System.Windows.Forms.Padding(2);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 3;
            this.buttonNew.Text = "New";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // dgDataset
            // 
            this.dgDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDataset.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDataset.Location = new System.Drawing.Point(-3, 0);
            this.dgDataset.Name = "dgDataset";
            this.dgDataset.Size = new System.Drawing.Size(1056, 623);
            this.dgDataset.TabIndex = 1;
            this.dgDataset.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgDataset_CellFormatting);
            this.dgDataset.SelectionChanged += new System.EventHandler(this.dgDataset_SelectionChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnAddSelected);
            this.tabPage4.Controls.Add(this.dgvProviderDataset);
            this.tabPage4.Controls.Add(this.txbProviderURL);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Controls.Add(this.btnGetProviderDatasets);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Controls.Add(this.textBoxPassword);
            this.tabPage4.Controls.Add(this.textBoxUserName);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1051, 655);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Get ProviderDataset";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnAddSelected
            // 
            this.btnAddSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSelected.Enabled = false;
            this.btnAddSelected.Location = new System.Drawing.Point(976, 628);
            this.btnAddSelected.Name = "btnAddSelected";
            this.btnAddSelected.Size = new System.Drawing.Size(74, 23);
            this.btnAddSelected.TabIndex = 4;
            this.btnAddSelected.Text = "&Add selected";
            this.btnAddSelected.UseVisualStyleBackColor = true;
            this.btnAddSelected.Click += new System.EventHandler(this.btnAddSelected_Click);
            // 
            // dgvProviderDataset
            // 
            this.dgvProviderDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvProviderDataset.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProviderDataset.Location = new System.Drawing.Point(0, 0);
            this.dgvProviderDataset.Name = "dgvProviderDataset";
            this.dgvProviderDataset.Size = new System.Drawing.Size(1053, 594);
            this.dgvProviderDataset.TabIndex = 0;
            this.dgvProviderDataset.SelectionChanged += new System.EventHandler(this.dgvProviderDataset_SelectionChanged);
            // 
            // txbProviderURL
            // 
            this.txbProviderURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbProviderURL.Location = new System.Drawing.Point(149, 601);
            this.txbProviderURL.Name = "txbProviderURL";
            this.txbProviderURL.Size = new System.Drawing.Size(901, 20);
            this.txbProviderURL.TabIndex = 0;
            this.txbProviderURL.Text = "https://geosynk.nois.no/tilbyder/";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 603);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Data Provider URL:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnGetProviderDatasets
            // 
            this.btnGetProviderDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetProviderDatasets.Location = new System.Drawing.Point(896, 628);
            this.btnGetProviderDatasets.Name = "btnGetProviderDatasets";
            this.btnGetProviderDatasets.Size = new System.Drawing.Size(75, 23);
            this.btnGetProviderDatasets.TabIndex = 3;
            this.btnGetProviderDatasets.Text = "&Get Datasets";
            this.btnGetProviderDatasets.UseVisualStyleBackColor = true;
            this.btnGetProviderDatasets.Click += new System.EventHandler(this.btnGetProviderDatasets_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 633);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Authentication (User/Pass):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPassword.Location = new System.Drawing.Point(229, 631);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(76, 20);
            this.textBoxPassword.TabIndex = 2;
            this.textBoxPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxPassword_KeyPress);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxUserName.Location = new System.Drawing.Point(149, 631);
            this.textBoxUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(76, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 96.51294F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3.487064F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1065, 712);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // textBoxCellValue
            // 
            this.textBoxCellValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxCellValue.Location = new System.Drawing.Point(7, 629);
            this.textBoxCellValue.Name = "textBoxCellValue";
            this.textBoxCellValue.Size = new System.Drawing.Size(244, 20);
            this.textBoxCellValue.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 712);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Geosynkronisering Subscriber";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProviderDataset)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ComboBox cboDatasetName;
        private System.Windows.Forms.TextBox txtLimitNumberOfFeatures;
        private System.Windows.Forms.Label labelLimitNumberOfFeatures;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.TextBox txbSubscrLastindex;
        private System.Windows.Forms.Button btnResetSubscrLastindex;
        private System.Windows.Forms.Button btnTestSyncronizationComplete;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnGetCapabilities;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbLastIndex;
        private System.Windows.Forms.Button btnGetLastIndex;
        private System.Windows.Forms.Button btnSimplify;
        private System.Windows.Forms.Button btnOfflineSync;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button btnDeleteSelected;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgDataset;
        private System.Windows.Forms.Button btnTestSyncronizationAll;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button btnAddSelected;
        private System.Windows.Forms.DataGridView dgvProviderDataset;
        private System.Windows.Forms.TextBox txbProviderURL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGetProviderDatasets;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonUpdateSelectedCells;
        private System.Windows.Forms.TextBox textBoxCellValue;
    }
}

