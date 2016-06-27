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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvProviderDataset = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAddSelected = new System.Windows.Forms.Button();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.btnGetProviderDatasets = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txbProviderURL = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.buttonSave = new System.Windows.Forms.Button();
            this.btnDeleteSelected = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOfflineSync = new System.Windows.Forms.Button();
            this.btnSimplify = new System.Windows.Forms.Button();
            this.btnGetLastIndex = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.txbLastIndex = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGetCapabilities = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnTestSyncronizationComplete = new System.Windows.Forms.Button();
            this.btnResetSubscrLastindex = new System.Windows.Forms.Button();
            this.txbSubscrLastindex = new System.Windows.Forms.TextBox();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.labelLimitNumberOfFeatures = new System.Windows.Forms.Label();
            this.txtLimitNumberOfFeatures = new System.Windows.Forms.TextBox();
            this.cboDatasetName = new System.Windows.Forms.ComboBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgDataset = new System.Windows.Forms.DataGridView();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProviderDataset)).BeginInit();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 867);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1501, 22);
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
            this.tabControl1.Location = new System.Drawing.Point(4, 4);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1493, 474);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.tableLayoutPanel6);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage4.Size = new System.Drawing.Size(1485, 445);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Get ProviderDataset";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel8, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.dgvProviderDataset, 0, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.34561F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 48.72521F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.64589F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1477, 437);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // dgvProviderDataset
            // 
            this.dgvProviderDataset.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProviderDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProviderDataset.Location = new System.Drawing.Point(4, 119);
            this.dgvProviderDataset.Margin = new System.Windows.Forms.Padding(4);
            this.dgvProviderDataset.Name = "dgvProviderDataset";
            this.dgvProviderDataset.Size = new System.Drawing.Size(1469, 205);
            this.dgvProviderDataset.TabIndex = 18;
            this.dgvProviderDataset.SelectionChanged += new System.EventHandler(this.dgvProviderDataset_SelectionChanged);
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Controls.Add(this.btnAddSelected, 0, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(4, 332);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(1469, 101);
            this.tableLayoutPanel8.TabIndex = 19;
            // 
            // btnAddSelected
            // 
            this.btnAddSelected.Enabled = false;
            this.btnAddSelected.Location = new System.Drawing.Point(4, 4);
            this.btnAddSelected.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddSelected.Name = "btnAddSelected";
            this.btnAddSelected.Size = new System.Drawing.Size(115, 28);
            this.btnAddSelected.TabIndex = 0;
            this.btnAddSelected.Text = "&Add selected";
            this.btnAddSelected.UseVisualStyleBackColor = true;
            this.btnAddSelected.Click += new System.EventHandler(this.btnAddSelected_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.86247F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.13753F));
            this.tableLayoutPanel7.Controls.Add(this.txbProviderURL, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.btnGetProviderDatasets, 1, 2);
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel9, 1, 1);
            this.tableLayoutPanel7.Controls.Add(this.label5, 0, 1);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 3;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(1469, 107);
            this.tableLayoutPanel7.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(4, 25);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(298, 42);
            this.label5.TabIndex = 4;
            this.label5.Text = "Authentication (User/Pass):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 3;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.46457F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.53543F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 519F));
            this.tableLayoutPanel9.Controls.Add(this.textBoxUserName, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.textBoxPassword, 1, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(310, 29);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 1;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(1155, 34);
            this.tableLayoutPanel9.TabIndex = 3;
            // 
            // btnGetProviderDatasets
            // 
            this.btnGetProviderDatasets.Location = new System.Drawing.Point(310, 71);
            this.btnGetProviderDatasets.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetProviderDatasets.Name = "btnGetProviderDatasets";
            this.btnGetProviderDatasets.Size = new System.Drawing.Size(121, 28);
            this.btnGetProviderDatasets.TabIndex = 2;
            this.btnGetProviderDatasets.Text = "&Get Datasets";
            this.btnGetProviderDatasets.UseVisualStyleBackColor = true;
            this.btnGetProviderDatasets.Click += new System.EventHandler(this.btnGetProviderDatasets_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(4, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(298, 25);
            this.label4.TabIndex = 1;
            this.label4.Text = "Type in the Data Provider\'s  URL:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txbProviderURL
            // 
            this.txbProviderURL.Dock = System.Windows.Forms.DockStyle.Top;
            this.txbProviderURL.Location = new System.Drawing.Point(310, 4);
            this.txbProviderURL.Margin = new System.Windows.Forms.Padding(4);
            this.txbProviderURL.Name = "txbProviderURL";
            this.txbProviderURL.Size = new System.Drawing.Size(1155, 22);
            this.txbProviderURL.TabIndex = 0;
            this.txbProviderURL.Text = "http://localhost:43397/WebFeatureServiceReplication.svc";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.splitContainer2);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage3.Size = new System.Drawing.Size(1485, 445);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Dataset";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(3, 40);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(100, 28);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.toolTip1.SetToolTip(this.buttonSave, "Save changes");
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteSelected.Location = new System.Drawing.Point(3, 4);
            this.btnDeleteSelected.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.Size = new System.Drawing.Size(100, 28);
            this.btnDeleteSelected.TabIndex = 2;
            this.btnDeleteSelected.Text = "Delete";
            this.toolTip1.SetToolTip(this.btnDeleteSelected, "Delete selected dataset");
            this.btnDeleteSelected.UseVisualStyleBackColor = true;
            this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.tableLayoutPanel5);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(1485, 445);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Detailed test";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 56.1828F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43.8172F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1477, 437);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1469, 236);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // btnOfflineSync
            // 
            this.btnOfflineSync.Location = new System.Drawing.Point(371, 39);
            this.btnOfflineSync.Margin = new System.Windows.Forms.Padding(4);
            this.btnOfflineSync.Name = "btnOfflineSync";
            this.btnOfflineSync.Size = new System.Drawing.Size(100, 28);
            this.btnOfflineSync.TabIndex = 17;
            this.btnOfflineSync.Text = "Offline sync";
            this.toolTip1.SetToolTip(this.btnOfflineSync, "Offline syncronization, input is a changelog zip-file.");
            this.btnOfflineSync.UseVisualStyleBackColor = true;
            this.btnOfflineSync.Click += new System.EventHandler(this.btnOfflineSync_Click);
            // 
            // btnSimplify
            // 
            this.btnSimplify.Location = new System.Drawing.Point(4, 39);
            this.btnSimplify.Margin = new System.Windows.Forms.Padding(4);
            this.btnSimplify.Name = "btnSimplify";
            this.btnSimplify.Size = new System.Drawing.Size(124, 54);
            this.btnSimplify.TabIndex = 16;
            this.btnSimplify.Text = "Schema Transformation";
            this.toolTip1.SetToolTip(this.btnSimplify, "Mapping from the nested structure of one or more simple features to the simple fe" +
        "atures for GeoServer");
            this.btnSimplify.UseVisualStyleBackColor = true;
            this.btnSimplify.Click += new System.EventHandler(this.btnSimplify_Click);
            // 
            // btnGetLastIndex
            // 
            this.btnGetLastIndex.Location = new System.Drawing.Point(1105, 4);
            this.btnGetLastIndex.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetLastIndex.Name = "btnGetLastIndex";
            this.btnGetLastIndex.Size = new System.Drawing.Size(123, 27);
            this.btnGetLastIndex.TabIndex = 3;
            this.btnGetLastIndex.Text = "GetLastIndex";
            this.btnGetLastIndex.UseVisualStyleBackColor = true;
            this.btnGetLastIndex.Click += new System.EventHandler(this.btnGetLastIndex_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txbLastIndex, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(1105, 39);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(219, 27);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // txbLastIndex
            // 
            this.txbLastIndex.Location = new System.Drawing.Point(113, 4);
            this.txbLastIndex.Margin = new System.Windows.Forms.Padding(4);
            this.txbLastIndex.Name = "txbLastIndex";
            this.txbLastIndex.Size = new System.Drawing.Size(100, 22);
            this.txbLastIndex.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txbLastIndex, "Provider lastIndex");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "lastIndex:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnGetCapabilities
            // 
            this.btnGetCapabilities.Location = new System.Drawing.Point(4, 4);
            this.btnGetCapabilities.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetCapabilities.Name = "btnGetCapabilities";
            this.btnGetCapabilities.Size = new System.Drawing.Size(120, 27);
            this.btnGetCapabilities.TabIndex = 0;
            this.btnGetCapabilities.Text = "GetCapabilities";
            this.btnGetCapabilities.UseVisualStyleBackColor = true;
            this.btnGetCapabilities.Click += new System.EventHandler(this.btnGetCapabilities_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.progressBar);
            this.tabPage1.Controls.Add(this.cboDatasetName);
            this.tabPage1.Controls.Add(this.txtLimitNumberOfFeatures);
            this.tabPage1.Controls.Add(this.labelLimitNumberOfFeatures);
            this.tabPage1.Controls.Add(this.listBoxLog);
            this.tabPage1.Controls.Add(this.txbSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnResetSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnTestSyncronizationComplete);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1485, 445);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Normal";
            // 
            // btnTestSyncronizationComplete
            // 
            this.btnTestSyncronizationComplete.Location = new System.Drawing.Point(8, 20);
            this.btnTestSyncronizationComplete.Margin = new System.Windows.Forms.Padding(4);
            this.btnTestSyncronizationComplete.Name = "btnTestSyncronizationComplete";
            this.btnTestSyncronizationComplete.Size = new System.Drawing.Size(176, 28);
            this.btnTestSyncronizationComplete.TabIndex = 19;
            this.btnTestSyncronizationComplete.Text = "Synchronize dataset";
            this.toolTip1.SetToolTip(this.btnTestSyncronizationComplete, "Test complete syncronization");
            this.btnTestSyncronizationComplete.UseVisualStyleBackColor = true;
            this.btnTestSyncronizationComplete.Click += new System.EventHandler(this.btnSyncronizationComplete_Click);
            // 
            // btnResetSubscrLastindex
            // 
            this.btnResetSubscrLastindex.Location = new System.Drawing.Point(901, 20);
            this.btnResetSubscrLastindex.Margin = new System.Windows.Forms.Padding(4);
            this.btnResetSubscrLastindex.Name = "btnResetSubscrLastindex";
            this.btnResetSubscrLastindex.Size = new System.Drawing.Size(213, 28);
            this.btnResetSubscrLastindex.TabIndex = 20;
            this.btnResetSubscrLastindex.Text = "Reset subscriber lastIndex";
            this.toolTip1.SetToolTip(this.btnResetSubscrLastindex, "Reset subscriber lastIndex");
            this.btnResetSubscrLastindex.UseVisualStyleBackColor = true;
            this.btnResetSubscrLastindex.Click += new System.EventHandler(this.btnResetSubscrLastindex_Click);
            // 
            // txbSubscrLastindex
            // 
            this.txbSubscrLastindex.Location = new System.Drawing.Point(1122, 23);
            this.txbSubscrLastindex.Margin = new System.Windows.Forms.Padding(4);
            this.txbSubscrLastindex.Name = "txbSubscrLastindex";
            this.txbSubscrLastindex.ReadOnly = true;
            this.txbSubscrLastindex.Size = new System.Drawing.Size(132, 22);
            this.txbSubscrLastindex.TabIndex = 21;
            this.toolTip1.SetToolTip(this.txbSubscrLastindex, "subscriber lastIndex");
            // 
            // listBoxLog
            // 
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.ItemHeight = 16;
            this.listBoxLog.Location = new System.Drawing.Point(9, 59);
            this.listBoxLog.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(1471, 228);
            this.listBoxLog.TabIndex = 22;
            // 
            // labelLimitNumberOfFeatures
            // 
            this.labelLimitNumberOfFeatures.AutoSize = true;
            this.labelLimitNumberOfFeatures.Location = new System.Drawing.Point(1270, 27);
            this.labelLimitNumberOfFeatures.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLimitNumberOfFeatures.Name = "labelLimitNumberOfFeatures";
            this.labelLimitNumberOfFeatures.Size = new System.Drawing.Size(130, 17);
            this.labelLimitNumberOfFeatures.TabIndex = 24;
            this.labelLimitNumberOfFeatures.Text = "Limit no. of objects:";
            this.labelLimitNumberOfFeatures.Visible = false;
            // 
            // txtLimitNumberOfFeatures
            // 
            this.txtLimitNumberOfFeatures.Location = new System.Drawing.Point(1409, 23);
            this.txtLimitNumberOfFeatures.Margin = new System.Windows.Forms.Padding(4);
            this.txtLimitNumberOfFeatures.Name = "txtLimitNumberOfFeatures";
            this.txtLimitNumberOfFeatures.Size = new System.Drawing.Size(68, 22);
            this.txtLimitNumberOfFeatures.TabIndex = 25;
            this.txtLimitNumberOfFeatures.Text = "-1";
            this.toolTip1.SetToolTip(this.txtLimitNumberOfFeatures, "For debugging limit number of objects");
            this.txtLimitNumberOfFeatures.Visible = false;
            // 
            // cboDatasetName
            // 
            this.cboDatasetName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDatasetName.FormattingEnabled = true;
            this.cboDatasetName.Location = new System.Drawing.Point(193, 22);
            this.cboDatasetName.Margin = new System.Windows.Forms.Padding(4);
            this.cboDatasetName.Name = "cboDatasetName";
            this.cboDatasetName.Size = new System.Drawing.Size(216, 24);
            this.cboDatasetName.TabIndex = 26;
            this.toolTip1.SetToolTip(this.cboDatasetName, "Current dataset");
            this.cboDatasetName.SelectedIndexChanged += new System.EventHandler(this.cboDatasetName_SelectedIndexChanged);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(11, 321);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1469, 28);
            this.progressBar.TabIndex = 27;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 302);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 17);
            this.label2.TabIndex = 28;
            this.label2.Text = "Synchronize progress:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 54.29363F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.70637F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1501, 889);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 485);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1495, 401);
            this.splitContainer1.SplitterDistance = 1198;
            this.splitContainer1.TabIndex = 3;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1198, 401);
            this.webBrowser1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(4, 4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgDataset);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.buttonSave);
            this.splitContainer2.Panel2.Controls.Add(this.btnDeleteSelected);
            this.splitContainer2.Size = new System.Drawing.Size(1477, 437);
            this.splitContainer2.SplitterDistance = 1366;
            this.splitContainer2.TabIndex = 0;
            // 
            // dgDataset
            // 
            this.dgDataset.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgDataset.Location = new System.Drawing.Point(0, 0);
            this.dgDataset.Margin = new System.Windows.Forms.Padding(4);
            this.dgDataset.Name = "dgDataset";
            this.dgDataset.Size = new System.Drawing.Size(1366, 437);
            this.dgDataset.TabIndex = 1;
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(3, 3);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(100, 22);
            this.textBoxUserName.TabIndex = 0;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(152, 3);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(100, 22);
            this.textBoxPassword.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1501, 889);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Geosynkronisering Subscriber";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProviderDataset)).EndInit();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TextBox txbProviderURL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGetProviderDatasets;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Button btnAddSelected;
        private System.Windows.Forms.DataGridView dgvProviderDataset;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button btnDeleteSelected;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgDataset;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.TextBox textBoxPassword;        
    }
}

