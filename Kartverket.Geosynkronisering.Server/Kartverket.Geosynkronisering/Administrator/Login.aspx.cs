using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace Kartverket.Geosynkronisering.Administrator
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LoginPage_Authenticate(object sender, AuthenticateEventArgs e)
        {
            string pwd = LoginPage.Password;
            string usr = LoginPage.UserName;
            e.Authenticated = FormsAuthentication.Authenticate(usr, pwd);
            
        }

        protected void LoginPage_LoggingIn(object sender, LoginCancelEventArgs e)
        {

            
            

        }

        protected void LoginPage_LoggedIn(object sender, EventArgs e)
        {
            
        }
    }
}