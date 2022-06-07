using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Kartverket.Geosynkronisering.Database;
using System.Reflection;
using System.Text;
using ChangelogManager;
using Kartverket.GeosyncWCF;
//using NLog;
//using NLog.Targets;
using Serilog;
using Serilog.Events;


namespace Kartverket.Geosynkronisering
{
    public partial class GeosynkroniseringAdmin : Page
    {
        //protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void ChangePage(object sender, CommandEventArgs e)
        {
            try
            {

                switch (e.CommandName)
                {
                    case "First":
                        if (e.CommandArgument == "DL")
                        {
                            vDataset.PageIndex = 0;
                            vDataset.DataBind(); // Must bind when using Dapper with List
                        }
                        else
                        {
                            gwStoredChangeLogs.PageIndex = 0;
                            gwStoredChangeLogs.DataBind();
                        }
                        break;

                    case "Prev":
                        if (e.CommandArgument == "DL")
                        {
                            if (vDataset.PageIndex > 0)
                            {
                                vDataset.PageIndex = vDataset.PageIndex - 1;
                                vDataset.DataBind();
                            }
                        }
                        else
                        {
                            if (gwStoredChangeLogs.PageIndex > 0)
                            {
                                gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex - 1;
                                gwStoredChangeLogs.DataBind();
                            }
                        }
                        break;

                    case "Next":
                        if (e.CommandArgument == "DL")
                        {
                            if (vDataset.PageIndex < vDataset.PageCount - 1)
                            {
                                vDataset.PageIndex = vDataset.PageIndex + 1;
                                vDataset.DataBind();
                            }
                        }
                        else
                        {
                            if (gwStoredChangeLogs.PageIndex < gwStoredChangeLogs.PageCount - 1)
                            {
                                gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex + 1;
                                gwStoredChangeLogs.DataBind();
                            }
                        }

                        break;

                    case "Last":
                        if (e.CommandArgument == "DL")
                        {
                            vDataset.PageIndex = vDataset.PageCount - 1;
                            vDataset.DataBind();
                        }
                        else
                        {
                            gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageCount - 1;
                            gwStoredChangeLogs.DataBind();
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }

        protected void lbtn_Click(object sender, EventArgs e)
        {
            mvwViews.ActiveViewIndex = Convert.ToInt32((sender as LinkButton).CommandName);
            lbtnChangeLog.CssClass = "LinkButton";
            lbtnConfig.CssClass = "LinkButton";
            lbtnDataset.CssClass = "LinkButton";
            (sender as LinkButton).CssClass = "LinkButtonSelected";
        }

        protected void Panel2_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {
                InitializeAndReadAllData();
            }
        }

        private void InitializeAndReadAllData()
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var listServerConfigs = db.ServerConfigs;
                dvServerConfig.DataSource = listServerConfigs;
                dvServerConfig.DataBind();


                var listServices = db.Services;
                dvService.DataSource = listServices;
                dvService.DataBind();

                var listDatasets = db.Datasets;
                //vDataset.PageIndex = 0;

                //BindingList<Dataset> bindingListDatasets = new BindingList<Dataset>(listDatasets);

                vDataset.DataSource = listDatasets; //bindingListDatasets; //listDatasets;
                vDataset.DataBind();

                var listStoredChangelogs = db.StoredChangelogs;
                gwStoredChangeLogs.DataSource = listStoredChangelogs;
                gwStoredChangeLogs.DataBind();
            }
        }


        #region vwConfig

        protected void vwConfig_Load(object sender, EventArgs e)
        {
            lblLogfileText.Text = "";
            var logFilename = GetLogfilenameWarn();
            if (File.Exists(logFilename))
            {
                if (new FileInfo(logFilename).Length > 0)
                {
                    lblLogfileText.Text = "Det finnes varslinger fra abonnent";
                    //TextBoxLogfile.Visible = true;
                }
            }

            if (vDataset.CurrentMode != DetailsViewMode.Insert) // if (!Page.IsPostBack)
            {
                // prevents problem with Insert, if we call this, all the values in event vDataset_ItemInserting will be null
                InitializeAndReadAllData();
            }
        }

        #endregion

        #region vDataset
        protected void vDataset_ItemCommand(object sender, DetailsViewCommandEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;
            if (e.CommandName == "Save")
            {

            }
            if (e.CommandName == "New")
                vDataset.DataBind();

            if (e.CommandName == "Insert")
            {
                //vDataset.DataBind();
                //DetailsViewRow row = vDataset.Rows[1];
                //var name = row.Cells[1].Text;

                
            }
                

        }

        protected void vDataset_ItemDeleteting(object sender, DetailsViewDeleteEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;
            //var datasetId = (int)dv.SelectedValue;
            var listOfDatasets = (List<Dataset>)dv.DataSource;

            var clientDataset = listOfDatasets[dv.PageIndex];
            //geosyncEntities.DeleteDataset(dataSet);

            using (var localDb = new geosyncEntities())
            {
                var dataset =
                    (from d in localDb.Datasets where d.DatasetId == clientDataset.DatasetId select d)
                    .FirstOrDefault();

                if (dataset == null)
                    return;

                localDb.DeleteObject(dataset);
                localDb.SaveChanges();
            }

            vDataset.DataSource = null;
            using (geosyncEntities db = new geosyncEntities())
            {
                var listDatasets = db.Datasets;
                //vDataset.PageIndex = 0;
                vDataset.DataSource = listDatasets;
                vDataset.DataBind();
            }
        }

        protected void vDataset_ItemDeleteted(object sender, DetailsViewDeletedEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;
            //var listDatasets = (List<Dataset>)dv.DataSource;
            //foreach (var dataset in listDatasets)
            //{
            //    geosyncEntities.DeleteDataset(dataset);
            //}

            //vDataset.DataBind();

        }

        protected void vDataset_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
        {
            int i = 1;

        }

        protected void vDataset_ItemInserting(object sender, DetailsViewInsertEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;
            //dv.DataBind();
            //dv.EnableViewState = true; // TEST

            //TextBox name = (TextBox)dv.FindControl("Name");
            //name = vDataset.FindControl("SchemaFileUri") as TextBox;
            //var obj = vDataset.Fields[1].AccessibleHeaderText.ToString();
            //var x = dv.Fields[1].ExtractValuesFromCell();

            //foreach (DetailsViewRow row in dv.Rows)
            //{
            //    var cell1 = row.Cells[1];
            //    var value = cell1.Text;
            //}

            //TextBox name = (TextBox)dv.FindControl("TextBox1");


            //TextBox tb = (TextBox)dv.Rows[1].Cells[1].Controls[0];
            //var text = tb.Text;
            //tb = (TextBox)dv.Rows[0].Cells[1].Controls[0];
            //text = tb.Text;

            var col = GetValues(dv);
            //var col = GetValues(vDataset);


            int i = 1;
            using (geosyncEntities db = new geosyncEntities())
            {
                var ds = new Dataset();
                db.AddObject(ds);
                UpdateDataset(e.Values, ds);
                db.SaveChanges();
            }



            

            vDataset.ChangeMode(DetailsViewMode.ReadOnly);
            //dv.EnableViewState = false; // TEST
            vDataset.DataSource = null;
            using (geosyncEntities db = new geosyncEntities())
            {
                var listDatasets = db.Datasets;
                //vDataset.PageIndex = 0;
                vDataset.DataSource = listDatasets;
                vDataset.DataBind();
            }
        }

        public static IOrderedDictionary GetValues(DetailsView detailsView)
        {
            // See: https://weblogs.asp.net/davidfowler/getting-your-data-out-of-the-data-controls
            IOrderedDictionary values = new OrderedDictionary();
            foreach (DetailsViewRow row in detailsView.Rows)
            {
                // Only look at Data Rows
                if (row.RowType != DataControlRowType.DataRow)
                {
                    continue;
                }
                // Assume the first cell is a header cell
                DataControlFieldCell dataCell = (DataControlFieldCell)row.Cells[0];
                // If we are showing the header for this row then the data is in the adjacent cell
                if (dataCell.ContainingField.ShowHeader)
                {
                    dataCell = (DataControlFieldCell)row.Cells[1];
                }

                dataCell.ContainingField.ExtractValuesFromCell(values, dataCell, row.RowState, true);
            }
            return values;
        }


        protected void vDataset_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
        {

            DetailsView dv = (DetailsView)sender;
            var listOfDatasets = (List<Dataset>)dv.DataSource;
            //var listOfDatasets = (BindingList<Dataset>)dv.DataSource;

            //BindingList<Dataset> clientDataset = (BindingList<Dataset>)listOfDatasets[dv.PageIndex];
            var clientDataset = listOfDatasets[dv.PageIndex];
            
            //BindingList<Dataset> bindingListDatasets = new BindingList<Dataset>(listDatasets);

            // TODO: New values in e.NewValues, add this to list.
            //var l = e.NewValues.
            UpdateDataset(e.NewValues, clientDataset);

            vDataset.ChangeMode(DetailsViewMode.ReadOnly);
            vDataset.DataSource = null;
            using (geosyncEntities db = new geosyncEntities())
            {
                var listDatasets = db.Datasets;
                //vDataset.PageIndex = 0;
                vDataset.DataSource = listDatasets;
                vDataset.DataBind();
            }
        }

        // This event is never sent.
        protected void vDataset_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;
            var listDatasets = (List<Dataset>)dv.DataSource;
            foreach (var dataset in listDatasets)
            {
                geosyncEntities.UpdateDataset(dataset);
            }

            dv.DataBind();
        }

        private bool UpdateDataset(IOrderedDictionary newValues, Dataset dataset)
        {
            if (newValues["Name"] != null) dataset.Name = newValues["Name"].ToString();

            if (newValues["SchemaFileUri"] != null)  dataset.SchemaFileUri = newValues["SchemaFileUri"].ToString();
            if (newValues["DatasetProvider"] != null) dataset.DatasetProvider = newValues["DatasetProvider"].ToString();

            if (newValues["ServerMaxCount"] != null) dataset.ServerMaxCount = Convert.ToInt32( newValues["ServerMaxCount"]);
            if (newValues["DatasetConnection"] != null) dataset.DatasetConnection = newValues["DatasetConnection"].ToString();
            if (newValues["DBSchema"] != null) dataset.DBSchema = newValues["DBSchema"].ToString();
            if (newValues["TransformationConnection"] != null) dataset.TransformationConnection = newValues["TransformationConnection"].ToString();
            if (newValues["DefaultCrs"] != null) dataset.DefaultCrs = newValues["DefaultCrs"].ToString();

            if (newValues["UpperCornerCoords"] != null) dataset.UpperCornerCoords = newValues["UpperCornerCoords"].ToString();
            if (newValues["LowerCornerCoords"] != null) dataset.LowerCornerCoords = newValues["LowerCornerCoords"].ToString();

            if (newValues["TargetNamespace"] != null) dataset.TargetNamespace = newValues["TargetNamespace"].ToString();
            if (newValues["TargetNamespacePrefix"] != null) dataset.TargetNamespacePrefix = newValues["TargetNamespacePrefix"].ToString();
            if (newValues["Version"] != null) dataset.Version = newValues["Version"].ToString();
            if (newValues["Decimals"] != null) dataset.Decimals = newValues["Decimals"].ToString();
            if (newValues["Tolerance"] != null) dataset.Tolerance = Convert.ToDouble(newValues["Tolerance"]);

            geosyncEntities.UpdateDataset(dataset);
            
            return true;
        }

        protected void vDataset_Modechanging(object sender, DetailsViewModeEventArgs e)
        {
            
            DetailsView dv = (DetailsView)sender;
            dv.ChangeMode(e.NewMode);
            dv.DataBind();

            if (e.CancelingEdit)
            {
                InitializeAndReadAllData();
            }
        }

        protected void vDataset_ItemCreated(object sender, EventArgs e)
        {
            // Test FooterRow to make sure all rows have been created 
            DetailsView dv = (DetailsView) sender;
            if (dv.FooterRow != null)
            {
                // The command bar is the last element in the Rows collection
                int commandRowIndex = dv.Rows.Count - 1;
                bool foundDelete = false;
                for (int i = 1; i <= 2; i++)
                {
                    if (i == 2 && !foundDelete)
                    {
                        --commandRowIndex;
                    }
                    if (commandRowIndex > 0)
                    {
                        DetailsViewRow commandRow = dv.Rows[commandRowIndex];

                        // Look for the DELETE button
                        DataControlFieldCell cell = (DataControlFieldCell)commandRow.Controls[0];
                        foreach (Control ctl in cell.Controls)
                        {
                            ImageButton del = ctl as ImageButton;
                            if (del != null)
                            {
                                if (del.CommandName == "Delete")
                                {
                                    foundDelete = true;
                                    del.OnClientClick =
                                        "if (!confirm('Er du sikker på at du vil slette denne posten?')) return;";
                                }
                            }
                        }
                    }

                }
            }
        }

        protected void vDataset_PreRender(object sender, EventArgs e)
        {
            DetailsView dw = (DetailsView) sender;
            if (dw != null)
            {
                DetailsViewRow pagerRow = dw.BottomPagerRow;
                if (pagerRow != null)
                {
                    pagerRow.Visible = true;
                }
            }
        }

        protected void vwDataset_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeAndReadAllData();
            }
        }

        #endregion // vDataset

        #region dvServerConfig

        //protected void dvServerConfig_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        //{
        //    // Event never raised
        //    string URL = ServerConfigData.DownloadUriBase();
        //    IList<Int32> ID = DatasetsData.GetListOfDatasetIDs();
        //}


        protected void dvServerConfig_Modechanging(object sender, DetailsViewModeEventArgs e)
        {

            DetailsView dv = (DetailsView)sender;
            dv.ChangeMode(e.NewMode);
            dv.DataBind();
        }

        //dvServerConfig_Updating
        protected void dvServerConfig_Updating(object sender, DetailsViewUpdateEventArgs e)
        {

            DetailsView dv = (DetailsView) sender;
            var listOfServerConfigs = (List<ServerConfig>) dv.DataSource;


            var clientConfig = listOfServerConfigs[dv.PageIndex];

            // New values in e.NewValues, add this to list.
            UpdateServerconfig(e.NewValues, clientConfig);

            dv.ChangeMode(DetailsViewMode.ReadOnly);
            dv.DataSource = null;

            using (geosyncEntities db = new geosyncEntities())
            {
                var listServerConfigs = db.ServerConfigs;
                dv.DataSource = listServerConfigs;
                dv.DataBind();
            }

        }

        private bool UpdateServerconfig(IOrderedDictionary newValues, ServerConfig serverConfig)
        {
            if (newValues["FTPUrl"] != null) serverConfig.FTPUrl = newValues["FTPUrl"].ToString();

            ServerConfigsEntities.UpdateServerConfig(serverConfig);

            return true;
        }
        #endregion

        #region dvService
        protected void dvService_Modechanging(object sender, DetailsViewModeEventArgs e)
        {

            DetailsView dv = (DetailsView)sender;
            dv.ChangeMode(e.NewMode);
            dv.DataBind();
        }

        protected void dvService_Updating(object sender, DetailsViewUpdateEventArgs e)
        {
            DetailsView dv = (DetailsView)sender;

            var listOfServices = (List<Service>)dv.DataSource;
            var clientService = listOfServices[dv.PageIndex];

            // New values in e.NewValues, add this to list.
            UpdateService(e.NewValues, clientService);


            dv.ChangeMode(DetailsViewMode.ReadOnly);
            dv.DataSource = null;

            using (geosyncEntities db = new geosyncEntities())
            {
                var listServices = db.Services;
                dv.DataSource = listServices;
                dv.DataBind();
            }
        }

        private bool UpdateService(IOrderedDictionary newValues, Service service)
        {
            if (newValues["Title"] != null) service.Title = newValues["Title"].ToString();
            if (newValues["Abstract"] != null) service.Abstract = newValues["Abstract"].ToString();
            if (newValues["Keywords"] != null) service.Keywords = newValues["Keywords"].ToString();
            if (newValues["Fees"] != null) service.Fees = newValues["Fees"].ToString();
            if (newValues["AccessConstraints"] != null) service.AccessConstraints = newValues["AccessConstraints"].ToString();

            if (newValues["ProviderName"] != null) service.ProviderName = newValues["ProviderName"].ToString();
            if (newValues["ProviderSite"] != null) service.ProviderSite = newValues["ProviderSite"].ToString();
            if (newValues["IndividualName"] != null) service.IndividualName = newValues["IndividualName"].ToString();
            if (newValues["Phone"] != null) service.Phone = newValues["Phone"].ToString();
            if (newValues["Facsimile"] != null) service.Facsimile = newValues["Facsimile"].ToString();
            
            if (newValues["Deliverypoint"] != null) service.Deliverypoint = newValues["Deliverypoint"].ToString();
            if (newValues["City"] != null) service.Phone = newValues["City"].ToString();
            if (newValues["PostalCode"] != null) service.PostalCode = newValues["PostalCode"].ToString();
            if (newValues["Country"] != null) service.Country = newValues["Country"].ToString();
            if (newValues["EMail"] != null) service.EMail = newValues["EMail"].ToString();
            
            if (newValues["OnlineResourcesUrl"] != null) service.OnlineResourcesUrl = newValues["OnlineResourcesUrl"].ToString();
            if (newValues["HoursOfService"] != null) service.HoursOfService = newValues["HoursOfService"].ToString();
            if (newValues["ContactInstructions"] != null) service.ContactInstructions = newValues["ContactInstructions"].ToString();
            if (newValues["Role"] != null) service.Role = newValues["Role"].ToString();
            if (newValues["ServiceURL"] != null) service.ServiceURL = newValues["ServiceURL"].ToString();
            
            if (newValues["ServiceID"] != null) service.ServiceID = newValues["ServiceID"].ToString();
            if (newValues["Namespace"] != null) service.Namespace = newValues["Namespace"].ToString();
            if (newValues["SchemaLocation"] != null) service.SchemaLocation = newValues["SchemaLocation"].ToString();

            ServicesEntities.UpdateServices(service);

            return true;
        }

        #endregion

        protected void btnCreateInitialData_Click(object sender, EventArgs e)
        {
            IChangelogProvider changelogprovider;
            int datasetId = Convert.ToInt32(vDataset.SelectedValue);

            lblErrorText.Text = "";
            //lblErrorText.Text = "Vent, oppretter initielle data..";
            btnLErrorfile.Visible = false;
            ResetTextBoxLogfile(TextBoxErrorfile);

            string initType = DatasetsData.DatasetProvider(datasetId);
            //Initiate provider from config/dataset

            Type providerType = Utils.GetProviderType(initType);
            //Type providerType = Assembly.GetExecutingAssembly().GetType(initType);

            changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
            changelogprovider.Intitalize(datasetId);
            try
            {
                var resp = changelogprovider.GenerateInitialChangelog(datasetId);
                //lblErrorText.Text = "";
            }
            catch (Exception ex)
            {
                string innerExMsg = "";
                Exception innerExp = ex.InnerException;
                while (innerExp != null)
                {
                    innerExMsg += string.Format("{0}. \n", innerExp.Message);
                    innerExp = innerExp.InnerException;
                }

                string errorMsg = string.Format("Klarte ikke å lage initiell endringslogg. {0} \n {1}", ex.Message,
                    innerExMsg);
                lblErrorText.Text = errorMsg;
                btnLErrorfile.Visible = true;
            }
        }

        #region mailAndLogfile

        private void ResetTextBoxLogfile(TextBox textBox)
        {
            textBox.Text = "";
            textBox.Rows = 0;
            textBox.Visible = false;
        }

        /// <summary>
        /// Show the subscriber logfile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnLogfile_Click(object sender, EventArgs e)
        {

            // test code
            bool sendMail = false;
            if (false)
            {
                bool isCtlClicked = false;
                var postedValCtlClicked = txtHiddenLogfileControl.Value.ToString();
                if (postedValCtlClicked == "ctl_clicked")
                {
                    isCtlClicked = true;
                }
                Logger.Info("isCtlClicked:{0}", isCtlClicked);
                sendMail = isCtlClicked;
            }

            // Show logfile
            var logFilename = GetLogfilenameWarn();
            if (File.Exists(logFilename))
            {
                Logger.Info("LogFile found:{0}", logFilename);

                if (File.Exists(logFilename))
                {
                    if (new FileInfo(logFilename).Length > 0)
                    {
                        TextBoxLogfile.Visible = true;
                        TextBoxLogfile.Rows = 10;

                        try
                        {
                            var contents = ReadLogfile(logFilename);
                            if (contents != null)
                            {
                                TextBoxLogfile.Text = contents;

                                if (false) // Test only
                                {
                                    if (sendMail)
                                    {
                                        var mail = new Mail();
                                        mail.SendMail(contents);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TextBoxLogfile.Text = ex.Message;
                            //throw;
                        }
                    }
                    else
                    {
                        ResetTextBoxLogfile(TextBoxLogfile);
                    }
                }
                else
                {
                    ResetTextBoxLogfile(TextBoxLogfile);
                }



                // Test Can't use Process.Start from asp.net
                if (false)
                {
                    System.Diagnostics.Process process = null;
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = logFilename
                    };
                    process.Start();
                }

            }
            else
            {
                Logger.Info("Logfile does not exist:{0}", logFilename);
                lblLogfileText.Text = "Ingen varslinger fra abonnent funnet";
                ResetTextBoxLogfile(TextBoxLogfile);
            }

        }

        /// <summary>
        /// Test sending mail.
        ///  This button will only be visibe when pressing CTRL+SHIFT+s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSendmail_Click(object sender, EventArgs e)
        {
            var logFilename = GetLogfilenameWarn();
            if (File.Exists(logFilename))
            {
                Logger.Info("LogFile found:{0}", logFilename);

                if (File.Exists(logFilename))
                {
                    if (new FileInfo(logFilename).Length > 0)
                    {
                        try
                        {
                            var contents = ReadLogfile(logFilename);
                            if (contents != null)
                            {
                                var mail = new Mail();
                                mail.SendMail(contents);

                            }
                        }
                        catch (Exception ex)
                        {
                            TextBoxLogfile.Text = ex.Message;
                            //throw;
                        }
                    }
                }
            }
        }

        protected void btnClearLogfile_Click(object sender, EventArgs e)
        {
            var logFilename = GetLogfilenameWarn();
            if (File.Exists(logFilename))
            {
                Logger.Info("LogFile found:{0}, clear it", logFilename);

#if nlog
                var fileStream = File.Open(logFilename, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();
#else
                using (FileStream fs = new FileStream(logFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                        fs.SetLength(0);
                        fs.Close();
                    
                    if (false)
                    {
                        // test still working logging
                        Log.Warning("Starting Test Serilog Warning emtied!");
                    }
                }
#endif
                Logger.Info("LogFile found:{0}, cleared", logFilename);
                ResetTextBoxLogfile(TextBoxLogfile);
            }
            else
            {
                Logger.Info("Logfile does not exist:{0}", logFilename);
            }

        }

        private string ReadLogfile(string logFilename)
        {
            try
            {
#if nlog
                using (StreamReader stream = File.OpenText(logFilename))
                {
                    var contents = stream.ReadToEnd();
                    stream.Close();
                    return contents;
                }

#else //Serilog
                // W/A reading serilog log-files: https://github.com/serilog/serilog-sinks-file/issues/86
                using (FileStream fs = new FileStream(logFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var contents = sr.ReadToEnd();
                        sr.Close();
                        fs.Close();
                        return contents;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ReadLogfile failed");
                throw;
            }

        }

        protected void btnLErrorfile_Click(object sender, EventArgs e)
        {
            // Show logfile
            var logFilename = GetLogfilenameError();
            if (File.Exists(logFilename))
            {
                Logger.Info("LogFile found:{0}", logFilename);
                //System.Diagnostics.Process.Start(logFilename);

                if (new FileInfo(logFilename).Length > 0)
                {
                    TextBoxErrorfile.Visible = true;
                    TextBoxErrorfile.Rows = 10;
                    try
                    {
                        var contents = ReadLogfile(logFilename);
                        if (contents != null)
                        {
                            TextBoxErrorfile.Text = contents;
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBoxErrorfile.Text = ex.Message;
                        //throw;
                    }
                }
                else
                {
                    ResetTextBoxLogfile(TextBoxErrorfile);
                }
            }
            else
            {
                Logger.Info("Logfile does not exist:{0}", logFilename);
                ResetTextBoxLogfile(TextBoxErrorfile);
            }
        }

#endregion

#region serilog
        private string GetSerilogFilepath()
        {
            // serielog
            var appSettings = ConfigurationManager.AppSettings;
            var serilogPath = appSettings["serilog-filepath"];
            if (serilogPath.ToUpper() == "%TEMP%")
            {
                serilogPath = Path.GetTempPath();
            }

            return serilogPath;
        }
        private string GetLogfilenameLog()
        {

            var serilogPath = GetSerilogFilepath();
            var appSettings = ConfigurationManager.AppSettings;
            var fileName = Path.Combine(serilogPath, appSettings["serilog-logfile"]);

            var date = DateTime.Now.ToString("yyyyMMdd");
            var dir = Path.GetDirectoryName(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName) + date;
            var exTension = Path.GetExtension(fileName);
            fileName = Path.Combine(dir, fileNameWithoutExtension, exTension);

            // nlog
            //var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("logfile");
            //// Need to set timestamp here if filename uses date. 
            //// For example - filename="${basedir}/logs/${shortdate}/trace.log"
            //var logEventInfo = new LogEventInfo {TimeStamp = DateTime.Now};
            //var fileName = fileTarget.FileName.Render(logEventInfo);

            return fileName;
        }

        private string GetLogfilenameWarn()
        {
            // no date on warning file
            var serilogPath = GetSerilogFilepath();
            var appSettings = ConfigurationManager.AppSettings;
            var fileName = Path.Combine(serilogPath, appSettings["serilog-warningfile"]);
            return fileName;
        }

        private string GetLogfilenameError()
        {
            var serilogPath = GetSerilogFilepath();
            var appSettings = ConfigurationManager.AppSettings;
            var fileName = Path.Combine(serilogPath, appSettings["serilog-errorfile"]);

            var date = DateTime.Now.ToString("yyyyMMdd");
            var dir = Path.GetDirectoryName(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName) + date;
            var exTension = Path.GetExtension(fileName);
            fileName = Path.Combine(dir, fileNameWithoutExtension, exTension);

            return fileName;
        }

#endregion

#if nlog
#region nlog

        private string GetLogfilenameWarn()
        {
            
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("warnfile");
            var logEventInfo = new LogEventInfo();
            var fileName = fileTarget.FileName.Render(logEventInfo);
            fileName = fileName.Replace("/", "");
            return fileName;
        }

        private string GetLogfilenameLog()
        {


            // nlog
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
            // Need to set timestamp here if filename uses date. 
            // For example - filename="${basedir}/logs/${shortdate}/trace.log"
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var fileName = fileTarget.FileName.Render(logEventInfo);


            return fileName;
        }

        private string GetLogfilenameError()
        {
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("errorfile");

            // Need to set timestamp here if filename uses date. 
            // For example - filename="${basedir}/logs/${shortdate}/trace.log"
            var logEventInfo = new LogEventInfo {TimeStamp = DateTime.Now};
            var fileName = fileTarget.FileName.Render(logEventInfo);
            return fileName;
        }
#endregion
#endif



        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (!Page.IsPostBack) // // TODO: Geosynk2.0: Fix this
        //    {
        //        using (geosyncEntities db = new geosyncEntities())
        //        {
        //            var listServerConfigs = db.ServerConfigs;
        //            dvServerConfig.DataSource = listServerConfigs;
        //            dvServerConfig.DataBind();


        //            var listServices = db.Services;
        //            dvService.DataSource = listServices;
        //            dvService.DataBind();

        //            var listDatasets = db.Datasets;
        //            vDataset.PageIndex = 0;
        //            vDataset.DataSource = listDatasets;
        //            vDataset.DataBind();

        //            var listStoredChangelogs = db.StoredChangelogs;
        //            gwStoredChangeLogs.DataSource = listStoredChangelogs;
        //            gwStoredChangeLogs.DataBind();

        //        }

        //    }
        //}
    }
}