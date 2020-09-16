using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Kartverket.Geosynkronisering.Database;
using System.Reflection;
using NLog;
using NLog.Targets;

namespace Kartverket.Geosynkronisering
{
    public partial class GeosynkroniseringAdmin : Page
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void ChangePage(object sender, CommandEventArgs e)
        {

            switch (e.CommandName)
            {
                case "First":
                    if (e.CommandArgument == "DL") vDataset.PageIndex = 0;
                    else gwStoredChangeLogs.PageIndex = 0;
                    break;

                case "Prev":
                    if (e.CommandArgument == "DL")
                        if (vDataset.PageIndex > 0) vDataset.PageIndex = vDataset.PageIndex - 1;
                        else if (gwStoredChangeLogs.PageIndex > 0)
                            gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex - 1;
                    break;

                case "Next":
                    if (e.CommandArgument == "DL")
                        if (vDataset.PageIndex < vDataset.PageCount - 1) vDataset.PageIndex = vDataset.PageIndex + 1;
                        else if (gwStoredChangeLogs.PageIndex < gwStoredChangeLogs.PageCount - 1)
                            gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex + 1;
                    break;

                case "Last":
                    if (e.CommandArgument == "DL") vDataset.PageIndex = vDataset.PageCount - 1;
                    else gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageCount - 1;
                    break;
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

        protected void vDataset_ItemCreated(object sender, EventArgs e)
        {
            // Test FooterRow to make sure all rows have been created 
            DetailsView dv = (DetailsView) sender;
            if (dv.FooterRow != null)
            {
                // The command bar is the last element in the Rows collection
                int commandRowIndex = dv.Rows.Count - 1;
                if (commandRowIndex > 0)
                {
                    DetailsViewRow commandRow = dv.Rows[commandRowIndex];

                    // Look for the DELETE button
                    DataControlFieldCell cell = (DataControlFieldCell) commandRow.Controls[0];
                    foreach (Control ctl in cell.Controls)
                    {
                        ImageButton del = ctl as ImageButton;
                        if (del != null)
                        {
                            if (del.CommandName == "Delete")
                            {
                                del.OnClientClick =
                                    "if (!confirm('Er du sikker på at du vil slette denne posten?')) return;";
                            }
                        }
                    }
                }
            }
        }

        protected void dvServerConfig_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            string URL = ServerConfigData.DownloadUriBase();
            IList<Int32> ID = DatasetsData.GetListOfDatasetIDs();
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

        protected void btnCreateInitialData_Click(object sender, EventArgs e)
        {
            IChangelogProvider changelogprovider;
            int datasetId = Convert.ToInt32(vDataset.SelectedValue);

            lblErrorText.Text = "";
            btnLErrorfile.Visible = false;
            ResetTextBoxLogfile(TextBoxErrorfile);

            string initType = DatasetsData.DatasetProvider(datasetId);
            //Initiate provider from config/dataset

            Type providerType = Assembly.GetExecutingAssembly().GetType(initType);
            changelogprovider = Activator.CreateInstance(providerType) as IChangelogProvider;
            changelogprovider.Intitalize(datasetId);
            try
            {
                var resp = changelogprovider.GenerateInitialChangelog(datasetId);
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
                var fileStream = File.Open(logFilename, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();
                Logger.Info("LogFile found:{0}, cleared", logFilename);
                ResetTextBoxLogfile(TextBoxLogfile);
            }
            else
            {
                Logger.Info("Logfile does not exist:{0}", logFilename);
            }

        }

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
        }

        private string ReadLogfile(string logFilename)
        {
            try
            {
                using (StreamReader stream = File.OpenText(logFilename))
                {
                    var contents = stream.ReadToEnd();
                    stream.Close();
                    return contents;
                }
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
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("logfile");

            // Need to set timestamp here if filename uses date. 
            // For example - filename="${basedir}/logs/${shortdate}/trace.log"
            var logEventInfo = new LogEventInfo {TimeStamp = DateTime.Now};
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
    }
}