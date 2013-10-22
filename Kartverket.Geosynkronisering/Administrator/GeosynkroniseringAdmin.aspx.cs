
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Kartverket.Geosynkronisering.Database;

namespace Kartverket.Geosynkronisering
{
    public partial class GeosynkroniseringAdmin : System.Web.UI.Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
          
            

        }

       

        protected void ChangePage(object sender, CommandEventArgs e)
        {
            
            switch (e.CommandName)
            {
                case "First":
                    if (e.CommandArgument == "DL") vDataset.PageIndex = 0; else gwStoredChangeLogs.PageIndex = 0;
                    break;

                case "Prev":
                    if (e.CommandArgument == "DL")
                    {
                        if (vDataset.PageIndex > 0) vDataset.PageIndex = vDataset.PageIndex - 1;
                    }
                    else
                    {
                        if (gwStoredChangeLogs.PageIndex > 0) gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex - 1;
                    }
                    break;

                case "Next":
                    if (e.CommandArgument == "DL")
                    {
                        if (vDataset.PageIndex < vDataset.PageCount - 1) vDataset.PageIndex = vDataset.PageIndex + 1;
                    }
                    else
                    {
                        if (gwStoredChangeLogs.PageIndex < gwStoredChangeLogs.PageCount - 1) gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageIndex + 1;
                    }
                    break;

                case "Last":
                    if (e.CommandArgument == "DL") vDataset.PageIndex = vDataset.PageCount - 1; else gwStoredChangeLogs.PageIndex = gwStoredChangeLogs.PageCount - 1;
                    break;

            }       
            
        }


        protected void lbtn_Click(object sender, EventArgs e)
        {
            mvwViews.ActiveViewIndex = Convert.ToInt32((sender as LinkButton).CommandName);
            lbtnChangeLog.CssClass = "LinkButton";
            lbtnConfig.CssClass = "LinkButton";
            lbtnDataset.CssClass = "LinkButton";
            lbtnLinkService.CssClass = "LinkButton";
            (sender as LinkButton).CssClass = "LinkButtonSelected";
        }

        protected void vDataset_ItemCreated(object sender, EventArgs e)
        {
            // Test FooterRow to make sure all rows have been created 
            DetailsView dv = (DetailsView)sender;
            if (dv.FooterRow != null)
            {
                // The command bar is the last element in the Rows collection
                int commandRowIndex = dv.Rows.Count - 1;
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
                                del.OnClientClick = "if (!confirm('Er du sikker på at du vil slette denne posten?')) return;"; 
                            }
                        }
                    }
                }
            }
        }

        protected void dvServerConfig_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
           string pwd =  ServerConfigData.FTPPwd();
           string user = ServerConfigData.FTPUser();
           string URL = ServerConfigData.FTPUrl();
           IList<Int32> ID = DatasetsData.GetListOfDatasetIDs();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Geosynkronisering.Database.CapabilitiesDataBuilder cdb = new CapabilitiesDataBuilder();
            GeosyncWCF.REP_CapabilitiesType cbt = cdb.GetCapabilities();
            cdb.SerializeObject(cbt, "C:\\temp\\getcapababilities.xml");
            
            
        }

        protected void vDataset_PreRender(object sender, EventArgs e)
        {
            DetailsView dw = (DetailsView)sender;
            if (dw != null)
            {
                DetailsViewRow pagerRow = dw.BottomPagerRow;
                if (pagerRow != null)
                {
                    pagerRow.Visible = true;                  
                    //foreach (Control ctrl in pagerRow.Controls)
                    //{
                    //    ctrl.Visible = true;
                    //    (ctrl as LinkButton).Enabled = true;
                    //}
                }

            }

        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }

      
    }
}