<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Kartverket.Geosynkronisering.Administrator.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table style="width: 100%;">
          <tr valign="baseline">
             <td width = 25% align="left" colspan="0" valign="baseline" > </td>
             <td width = 50% align="center" colspan="0" valign="baseline">
                <asp:Login ID="LoginPage" runat="server" BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" FailureText="Pålogging feilet. Prøv igjen." Font-Names="Verdana" Font-Size="0.8em" ForeColor="#333333" LoginButtonText="Logg inn" OnAuthenticate="LoginPage_Authenticate" OnLoggedIn="LoginPage_LoggedIn" OnLoggingIn="LoginPage_LoggingIn" PasswordLabelText="Passord:" PasswordRequiredErrorMessage="Passord er påkrevd." RememberMeText="Husk meg." TitleText="Administratormodul Geosynkronisering" UserNameLabelText="Brukernavn:" UserNameRequiredErrorMessage="Brukernavn er påkrevd." Height="151px" Width="321px">
                    <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
                    <LoginButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284775" />
                    <TextBoxStyle Font-Size="0.8em" />
                    <TitleTextStyle BackColor="#5D7B9D" Font-Bold="True" Font-Size="Small" ForeColor="White" />
                </asp:Login>           
            </td>                          
            <td width = 25% align="left" colspan="0" valign="baseline" > </td>
          </tr>                            
         </table>
    </div>
    </form>
</body>
</html>
