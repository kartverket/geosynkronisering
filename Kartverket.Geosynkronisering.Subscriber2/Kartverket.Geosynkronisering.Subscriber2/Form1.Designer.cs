namespace Kartverket.Geosynkronisering.Subscriber2
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtLimitNumberOfFeatures = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDataset = new System.Windows.Forms.TextBox();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.txbSubscrLastindex = new System.Windows.Forms.TextBox();
            this.btnResetSubscrLastindex = new System.Windows.Forms.Button();
            this.btnTestSyncronizationComplete = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSimplify = new System.Windows.Forms.Button();
            this.btnGetCapabilities = new System.Windows.Forms.Button();
            this.btnDescribeFeaturetype = new System.Windows.Forms.Button();
            this.btnListStoredChangelogs = new System.Windows.Forms.Button();
            this.btnGetLastIndex = new System.Windows.Forms.Button();
            this.btnOrderChangelog = new System.Windows.Forms.Button();
            this.btnGetChangelog = new System.Windows.Forms.Button();
            this.btnAcknowledgeChangelogDownloaded = new System.Windows.Forms.Button();
            this.btnCancelChangelog = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.txbLastIndex = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.txbChangelogid = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnGetChangelogStatus = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbGET = new System.Windows.Forms.RadioButton();
            this.rbPOST = new System.Windows.Forms.RadioButton();
            this.btnParseWfsChangelog = new System.Windows.Forms.Button();
            this.txbDownloadedFile = new System.Windows.Forms.TextBox();
            this.btnDoTransaction = new System.Windows.Forms.Button();
            this.btnSoapGLI = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.buttonSave = new System.Windows.Forms.Button();
            this.dgDataset = new System.Windows.Forms.DataGridView();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.cboDatasetName = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.webBrowser1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.50415F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61.49585F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(884, 722);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(878, 271);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.cboDatasetName);
            this.tabPage1.Controls.Add(this.txtLimitNumberOfFeatures);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtDataset);
            this.tabPage1.Controls.Add(this.listBoxLog);
            this.tabPage1.Controls.Add(this.txbSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnResetSubscrLastindex);
            this.tabPage1.Controls.Add(this.btnTestSyncronizationComplete);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(870, 245);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Normal";
            // 
            // txtLimitNumberOfFeatures
            // 
            this.txtLimitNumberOfFeatures.Location = new System.Drawing.Point(722, 19);
            this.txtLimitNumberOfFeatures.Name = "txtLimitNumberOfFeatures";
            this.txtLimitNumberOfFeatures.Size = new System.Drawing.Size(52, 20);
            this.txtLimitNumberOfFeatures.TabIndex = 25;
            this.txtLimitNumberOfFeatures.Text = "-1";
            this.toolTip1.SetToolTip(this.txtLimitNumberOfFeatures, "For debugging limit number of objects");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(618, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "Limit no. of objects:";
            // 
            // txtDataset
            // 
            this.txtDataset.Location = new System.Drawing.Point(144, 18);
            this.txtDataset.Name = "txtDataset";
            this.txtDataset.Size = new System.Drawing.Size(164, 20);
            this.txtDataset.TabIndex = 23;
            this.toolTip1.SetToolTip(this.txtDataset, "Current dataset");
            // 
            // listBoxLog
            // 
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.Location = new System.Drawing.Point(7, 48);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(857, 186);
            this.listBoxLog.TabIndex = 22;
            // 
            // txbSubscrLastindex
            // 
            this.txbSubscrLastindex.Location = new System.Drawing.Point(507, 19);
            this.txbSubscrLastindex.Name = "txbSubscrLastindex";
            this.txbSubscrLastindex.ReadOnly = true;
            this.txbSubscrLastindex.Size = new System.Drawing.Size(100, 20);
            this.txbSubscrLastindex.TabIndex = 21;
            this.toolTip1.SetToolTip(this.txbSubscrLastindex, "subscriber lastIndex");
            // 
            // btnResetSubscrLastindex
            // 
            this.btnResetSubscrLastindex.Location = new System.Drawing.Point(341, 16);
            this.btnResetSubscrLastindex.Name = "btnResetSubscrLastindex";
            this.btnResetSubscrLastindex.Size = new System.Drawing.Size(160, 23);
            this.btnResetSubscrLastindex.TabIndex = 20;
            this.btnResetSubscrLastindex.Text = "Reset subscriber lastIndex";
            this.toolTip1.SetToolTip(this.btnResetSubscrLastindex, "Reset subscriber lastIndex");
            this.btnResetSubscrLastindex.UseVisualStyleBackColor = true;
            this.btnResetSubscrLastindex.Click += new System.EventHandler(this.btnResetSubscrLastindex_Click);
            // 
            // btnTestSyncronizationComplete
            // 
            this.btnTestSyncronizationComplete.Location = new System.Drawing.Point(6, 16);
            this.btnTestSyncronizationComplete.Name = "btnTestSyncronizationComplete";
            this.btnTestSyncronizationComplete.Size = new System.Drawing.Size(132, 23);
            this.btnTestSyncronizationComplete.TabIndex = 19;
            this.btnTestSyncronizationComplete.Text = "Test complete sync";
            this.toolTip1.SetToolTip(this.btnTestSyncronizationComplete, "Test complete syncronization");
            this.btnTestSyncronizationComplete.UseVisualStyleBackColor = true;
            this.btnTestSyncronizationComplete.Click += new System.EventHandler(this.btnTestSyncronizationComplete_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(870, 245);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Detailed test";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.btnSimplify, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.btnGetCapabilities, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDescribeFeaturetype, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnListStoredChangelogs, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnGetLastIndex, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnOrderChangelog, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnGetChangelog, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnAcknowledgeChangelogDownloaded, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnCancelChangelog, 3, 2);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnGetChangelogStatus, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnParseWfsChangelog, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.txbDownloadedFile, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnDoTransaction, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnSoapGLI, 3, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(864, 239);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // btnSimplify
            // 
            this.btnSimplify.Location = new System.Drawing.Point(3, 116);
            this.btnSimplify.Name = "btnSimplify";
            this.btnSimplify.Size = new System.Drawing.Size(93, 44);
            this.btnSimplify.TabIndex = 16;
            this.btnSimplify.Text = "Schema Transformation";
            this.toolTip1.SetToolTip(this.btnSimplify, "Mapping from the nested structure of one or more simple features to the simple fe" +
        "atures for GeoServer");
            this.btnSimplify.UseVisualStyleBackColor = true;
            this.btnSimplify.Click += new System.EventHandler(this.btnSimplify_Click);
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
            // btnDescribeFeaturetype
            // 
            this.btnDescribeFeaturetype.Location = new System.Drawing.Point(219, 3);
            this.btnDescribeFeaturetype.Name = "btnDescribeFeaturetype";
            this.btnDescribeFeaturetype.Size = new System.Drawing.Size(113, 22);
            this.btnDescribeFeaturetype.TabIndex = 1;
            this.btnDescribeFeaturetype.Text = "DescribeFeaturetype";
            this.btnDescribeFeaturetype.UseVisualStyleBackColor = true;
            this.btnDescribeFeaturetype.Click += new System.EventHandler(this.btnDescribeFeaturetype_Click);
            // 
            // btnListStoredChangelogs
            // 
            this.btnListStoredChangelogs.Location = new System.Drawing.Point(435, 3);
            this.btnListStoredChangelogs.Name = "btnListStoredChangelogs";
            this.btnListStoredChangelogs.Size = new System.Drawing.Size(132, 22);
            this.btnListStoredChangelogs.TabIndex = 2;
            this.btnListStoredChangelogs.Text = "ListStoredChangelogs";
            this.btnListStoredChangelogs.UseVisualStyleBackColor = true;
            this.btnListStoredChangelogs.Click += new System.EventHandler(this.btnListStoredChangelogs_Click);
            // 
            // btnGetLastIndex
            // 
            this.btnGetLastIndex.Location = new System.Drawing.Point(651, 3);
            this.btnGetLastIndex.Name = "btnGetLastIndex";
            this.btnGetLastIndex.Size = new System.Drawing.Size(92, 22);
            this.btnGetLastIndex.TabIndex = 3;
            this.btnGetLastIndex.Text = "GetLastIndex";
            this.btnGetLastIndex.UseVisualStyleBackColor = true;
            this.btnGetLastIndex.Click += new System.EventHandler(this.btnGetLastIndex_Click);
            // 
            // btnOrderChangelog
            // 
            this.btnOrderChangelog.Location = new System.Drawing.Point(3, 31);
            this.btnOrderChangelog.Name = "btnOrderChangelog";
            this.btnOrderChangelog.Size = new System.Drawing.Size(99, 22);
            this.btnOrderChangelog.TabIndex = 4;
            this.btnOrderChangelog.Text = "OrderChangelog";
            this.btnOrderChangelog.UseVisualStyleBackColor = true;
            this.btnOrderChangelog.Click += new System.EventHandler(this.btnOrderChangelog_Click);
            // 
            // btnGetChangelog
            // 
            this.btnGetChangelog.Location = new System.Drawing.Point(435, 31);
            this.btnGetChangelog.Name = "btnGetChangelog";
            this.btnGetChangelog.Size = new System.Drawing.Size(93, 22);
            this.btnGetChangelog.TabIndex = 6;
            this.btnGetChangelog.Text = "GetChangelog";
            this.btnGetChangelog.UseVisualStyleBackColor = true;
            this.btnGetChangelog.Click += new System.EventHandler(this.btnGetChangelog_Click);
            // 
            // btnAcknowledgeChangelogDownloaded
            // 
            this.btnAcknowledgeChangelogDownloaded.Location = new System.Drawing.Point(435, 59);
            this.btnAcknowledgeChangelogDownloaded.Name = "btnAcknowledgeChangelogDownloaded";
            this.btnAcknowledgeChangelogDownloaded.Size = new System.Drawing.Size(162, 22);
            this.btnAcknowledgeChangelogDownloaded.TabIndex = 7;
            this.btnAcknowledgeChangelogDownloaded.Text = "AcknowledgeChangelogDownloaded";
            this.btnAcknowledgeChangelogDownloaded.UseVisualStyleBackColor = true;
            this.btnAcknowledgeChangelogDownloaded.Click += new System.EventHandler(this.btnAcknowledgeChangelogDownloaded_Click);
            // 
            // btnCancelChangelog
            // 
            this.btnCancelChangelog.Location = new System.Drawing.Point(651, 59);
            this.btnCancelChangelog.Name = "btnCancelChangelog";
            this.btnCancelChangelog.Size = new System.Drawing.Size(109, 22);
            this.btnCancelChangelog.TabIndex = 8;
            this.btnCancelChangelog.Text = "CancelChangelog";
            this.btnCancelChangelog.UseVisualStyleBackColor = true;
            this.btnCancelChangelog.Click += new System.EventHandler(this.btnCancelChangelog_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txbLastIndex, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(651, 31);
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
            this.label1.Text = "lastIndex";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txbLastIndex
            // 
            this.txbLastIndex.Location = new System.Drawing.Point(85, 3);
            this.txbLastIndex.Name = "txbLastIndex";
            this.txbLastIndex.Size = new System.Drawing.Size(76, 20);
            this.txbLastIndex.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txbLastIndex, "Provider lastIndex");
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.txbChangelogid, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 59);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(162, 22);
            this.tableLayoutPanel4.TabIndex = 10;
            // 
            // txbChangelogid
            // 
            this.txbChangelogid.Location = new System.Drawing.Point(84, 3);
            this.txbChangelogid.Name = "txbChangelogid";
            this.txbChangelogid.Size = new System.Drawing.Size(75, 20);
            this.txbChangelogid.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 22);
            this.label2.TabIndex = 1;
            this.label2.Text = "Changelogid";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnGetChangelogStatus
            // 
            this.btnGetChangelogStatus.Location = new System.Drawing.Point(219, 59);
            this.btnGetChangelogStatus.Name = "btnGetChangelogStatus";
            this.btnGetChangelogStatus.Size = new System.Drawing.Size(113, 22);
            this.btnGetChangelogStatus.TabIndex = 5;
            this.btnGetChangelogStatus.Text = "GetChangelogStatus";
            this.btnGetChangelogStatus.UseVisualStyleBackColor = true;
            this.btnGetChangelogStatus.Click += new System.EventHandler(this.btnGetChangelogStatus_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbGET);
            this.panel1.Controls.Add(this.rbPOST);
            this.panel1.Location = new System.Drawing.Point(219, 31);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(162, 22);
            this.panel1.TabIndex = 11;
            // 
            // rbGET
            // 
            this.rbGET.AutoSize = true;
            this.rbGET.Checked = true;
            this.rbGET.Location = new System.Drawing.Point(76, 4);
            this.rbGET.Name = "rbGET";
            this.rbGET.Size = new System.Drawing.Size(47, 17);
            this.rbGET.TabIndex = 1;
            this.rbGET.TabStop = true;
            this.rbGET.Text = "GET";
            this.rbGET.UseVisualStyleBackColor = true;
            // 
            // rbPOST
            // 
            this.rbPOST.AutoSize = true;
            this.rbPOST.Location = new System.Drawing.Point(3, 4);
            this.rbPOST.Name = "rbPOST";
            this.rbPOST.Size = new System.Drawing.Size(54, 17);
            this.rbPOST.TabIndex = 0;
            this.rbPOST.Text = "POST";
            this.rbPOST.UseVisualStyleBackColor = true;
            // 
            // btnParseWfsChangelog
            // 
            this.btnParseWfsChangelog.Location = new System.Drawing.Point(219, 87);
            this.btnParseWfsChangelog.Name = "btnParseWfsChangelog";
            this.btnParseWfsChangelog.Size = new System.Drawing.Size(79, 22);
            this.btnParseWfsChangelog.TabIndex = 12;
            this.btnParseWfsChangelog.Text = "Parse WFS Changelog";
            this.toolTip1.SetToolTip(this.btnParseWfsChangelog, "Parse WFS Changelog");
            this.btnParseWfsChangelog.UseVisualStyleBackColor = true;
            this.btnParseWfsChangelog.Click += new System.EventHandler(this.btnParseWfsChangelog_Click);
            // 
            // txbDownloadedFile
            // 
            this.txbDownloadedFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbDownloadedFile.Location = new System.Drawing.Point(3, 87);
            this.txbDownloadedFile.Name = "txbDownloadedFile";
            this.txbDownloadedFile.Size = new System.Drawing.Size(210, 20);
            this.txbDownloadedFile.TabIndex = 13;
            // 
            // btnDoTransaction
            // 
            this.btnDoTransaction.Location = new System.Drawing.Point(435, 87);
            this.btnDoTransaction.Name = "btnDoTransaction";
            this.btnDoTransaction.Size = new System.Drawing.Size(93, 22);
            this.btnDoTransaction.TabIndex = 14;
            this.btnDoTransaction.Text = "DoTransaction";
            this.toolTip1.SetToolTip(this.btnDoTransaction, "Do WFS-T transaction");
            this.btnDoTransaction.UseVisualStyleBackColor = true;
            this.btnDoTransaction.Click += new System.EventHandler(this.btnDoTransaction_Click);
            // 
            // btnSoapGLI
            // 
            this.btnSoapGLI.Location = new System.Drawing.Point(651, 87);
            this.btnSoapGLI.Name = "btnSoapGLI";
            this.btnSoapGLI.Size = new System.Drawing.Size(144, 23);
            this.btnSoapGLI.TabIndex = 15;
            this.btnSoapGLI.Text = "SOAP GetLastIndex";
            this.btnSoapGLI.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.buttonSave);
            this.tabPage3.Controls.Add(this.dgDataset);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(870, 245);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Dataset";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(789, 206);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // dgDataset
            // 
            this.dgDataset.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgDataset.Location = new System.Drawing.Point(3, 3);
            this.dgDataset.Name = "dgDataset";
            this.dgDataset.Size = new System.Drawing.Size(864, 239);
            this.dgDataset.TabIndex = 0;
            this.dgDataset.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgDataset_DataBindingComplete);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(3, 280);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(878, 439);
            this.webBrowser1.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 700);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // cboDatasetName
            // 
            this.cboDatasetName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDatasetName.FormattingEnabled = true;
            this.cboDatasetName.Location = new System.Drawing.Point(145, 18);
            this.cboDatasetName.Name = "cboDatasetName";
            this.cboDatasetName.Size = new System.Drawing.Size(163, 21);
            this.cboDatasetName.TabIndex = 26;
            this.toolTip1.SetToolTip(this.cboDatasetName, "Current dataset");
            this.cboDatasetName.SelectedIndexChanged += new System.EventHandler(this.cboDatasetName_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 722);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Geosynkronisering Subscriber";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgDataset)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txbSubscrLastindex;
        private System.Windows.Forms.Button btnResetSubscrLastindex;
        private System.Windows.Forms.Button btnTestSyncronizationComplete;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnGetCapabilities;
        private System.Windows.Forms.Button btnDescribeFeaturetype;
        private System.Windows.Forms.Button btnListStoredChangelogs;
        private System.Windows.Forms.Button btnGetLastIndex;
        private System.Windows.Forms.Button btnOrderChangelog;
        private System.Windows.Forms.Button btnGetChangelog;
        private System.Windows.Forms.Button btnAcknowledgeChangelogDownloaded;
        private System.Windows.Forms.Button btnCancelChangelog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbLastIndex;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TextBox txbChangelogid;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGetChangelogStatus;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbGET;
        private System.Windows.Forms.RadioButton rbPOST;
        private System.Windows.Forms.Button btnParseWfsChangelog;
        private System.Windows.Forms.TextBox txbDownloadedFile;
        private System.Windows.Forms.Button btnDoTransaction;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Button btnSoapGLI;
        private System.Windows.Forms.TextBox txtDataset;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgDataset;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.TextBox txtLimitNumberOfFeatures;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSimplify;
        private System.Windows.Forms.ComboBox cboDatasetName;
    }
}

