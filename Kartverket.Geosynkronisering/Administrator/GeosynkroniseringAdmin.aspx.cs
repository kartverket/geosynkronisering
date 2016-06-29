using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Kartverket.Geosynkronisering.Database;
using System.Reflection;

namespace Kartverket.Geosynkronisering
{
    public partial class GeosynkroniseringAdmin : Page
    {
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
            lbtnDataset.CssClass = "LinkButton";
            lbtnLinkService.CssClass = "LinkButton";
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

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }

        protected void btnCreateInitialData_Click(object sender, EventArgs e)
        {
            IChangelogProvider changelogprovider;
            int datasetId = Convert.ToInt32(vDataset.SelectedValue);
            lblErrorText.Text = "";
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
            }
        }
    }
}