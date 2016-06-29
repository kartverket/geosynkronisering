<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GeosynkroniseringAdmin.aspx.cs" Inherits="Kartverket.Geosynkronisering.GeosynkroniseringAdmin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<LINK REL="stylesheet" TYPE="text/css" HREF="GeoSynkWebStyleSheet.css">

<link href="GeoSynkWebStyleSheet.css" rel="stylesheet" type="text/css" />
<script language="javascript" type="text/javascript">

    function setActiveView(mviewID, view) 
    {
        document.getElementById(mviewID).setAttribute("ActiveView", view);
    }



</script>
<head runat="server">
    <title>Geosynkronisering Administrator</title>
</head>
<body>
    <form id="Administrator" runat="server" name="Geosynkronisering">
    <asp:Panel ID="Panel2" runat="server" CssClass="PanelBlank">
   
    
        <table style="width: 100%;">
            <tr style= "height: 10%">
              <td width="10%">                        
                  <asp:Image ID="Image1" runat="server" Height="61px" 
                      ImageUrl="~/Images/Geosynk.png" Width="64px" />
                </td>
                <td width="60%">                        
                    <asp:Label ID="Label2" runat="server" CssClass="HeaderText" 
                        Text="Geosynkronisering Administrator"></asp:Label>
                </td>
                <td width="20%">                        
                    <asp:Button ID="btnSignOut" runat="server" OnClick="btnSignOut_Click" Text="Logg ut" CssClass="Button" />
                </td>
                <td width="10%">                        
                </td>
            </tr>
            <tr>
                <td width="10%">                        
                </td>
                <td width="80%" colspan="0">  
                    <table style="width: 100%;">
                        <tr valign="baseline">
                            <td align="left" colspan="0" valign="baseline">
                                <asp:LinkButton ID="lbtnConfig" runat="server" CssClass="LinkButtonSelected" 
                                    CommandName="0" onclick="lbtn_Click">Konfigurasjon</asp:LinkButton>
                                <asp:LinkButton ID="lbtnLinkService" runat="server" CssClass="LinkButton" 
                                    CommandName="1" onclick="lbtn_Click">Tjenestekonfigurasjon</asp:LinkButton>
                                <asp:LinkButton ID="lbtnDataset" runat="server" CssClass="LinkButton" 
                                    CommandName="2" onclick="lbtn_Click">Datasett</asp:LinkButton>
                                <asp:LinkButton ID="lbtnChangeLog" runat="server" CssClass="LinkButton" 
                                    CommandName="3" onclick="lbtn_Click">Endringslogg</asp:LinkButton>
                            </td>                          
                        </tr>                            
                     </table>
                    <div style="height: 100%">
                        <table style="width: 100%;">
                        <tr>
                            <td>
                              &nbsp;
                            </td>
                            <td>
                               <asp:MultiView ID="mvwViews" runat="server" ActiveViewIndex="0">
                                   <asp:View ID="vwConfig" runat="server">
                                   <div style="height: 10%">
                                            <asp:Label ID="Label4" runat="server" Text="Konfigurasjon:" 
                                                CssClass="TableHeaderText"></asp:Label>
                                        </div>       
                                        <div style="height: 80%">
                                            <asp:DetailsView ID="dvServerConfig" runat="server" AutoGenerateRows="False" 
                                                CssClass="GridView" DataKeyNames="ID" DataSourceID="edsServerConfig" 
                                                Height="50px" Width="125px" onitemupdated="dvServerConfig_ItemUpdated">
                                                <EditRowStyle CssClass="GridView" />
                                                <EmptyDataRowStyle CssClass="GridView" />
                                                <FieldHeaderStyle CssClass="FieldHeader" />
                                                <Fields>
                                                    <asp:BoundField DataField="FTPUrl" HeaderText="Download URL" 
                                                        SortExpression="FTPUrl" >
                                                    <HeaderStyle CssClass="FieldHeader" />
                                                    </asp:BoundField>
                                                    <asp:CommandField ButtonType="Image" CancelImageUrl="~/Images/Edit_UndoHS.png" 
                                                        CancelText="Avbryt" DeleteImageUrl="~/Images/delete_12x12.png" 
                                                        DeleteText="Slett" EditImageUrl="~/Images/EditTableHS.png" EditText="Rediger" 
                                                        InsertText="Sett inn" NewText="Ny" SelectText="Velg" ShowEditButton="True" 
                                                        UpdateImageUrl="~/Images/saveHS.png" UpdateText="Lagre" />
                                                </Fields>
                                            </asp:DetailsView>
                                        </div>
                                        <div style="height: 10%">
                                            &nbsp;                                
                                        </div>
                                       
                                   </asp:View>
                                   <asp:View ID="vwService" runat="server">
                                   <div style="height: 10%">
                                            <asp:Label ID="Label5" runat="server" Text="Service konfigurasjon:" 
                                                CssClass="TableHeaderText"></asp:Label>
                                        </div>       
                                        <div style="height: 80%">
                                            <asp:DetailsView ID="dvService" runat="server" AutoGenerateRows="False" 
                                                CssClass="GridView" DataKeyNames="ServiceID" DataSourceID="edsService" 
                                                Height="50px" Width="125px">
                                                <EditRowStyle CssClass="GridView" />
                                                <FieldHeaderStyle CssClass="FieldHeader" />
                                                <Fields>
                                                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Abstract" HeaderText="Abstract" 
                                                        SortExpression="Abstract" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Keywords" HeaderText="Keywords" 
                                                        SortExpression="Keywords" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Fees" HeaderText="Fees" SortExpression="Fees" />
                                                    <asp:BoundField DataField="AccessConstraints" HeaderText="AccessConstraints" 
                                                        SortExpression="AccessConstraints" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="ProviderName" HeaderText="ProviderName" 
                                                        SortExpression="ProviderName" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="ProviderSite" HeaderText="ProviderSite" 
                                                        SortExpression="ProviderSite" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="IndividualName" HeaderText="IndividualName" 
                                                        SortExpression="IndividualName" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Facsimile" HeaderText="Facsimile" 
                                                        SortExpression="Facsimile" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Deliverypoint" HeaderText="Deliverypoint" 
                                                        SortExpression="Deliverypoint" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="PostalCode" HeaderText="PostalCode" 
                                                        SortExpression="PostalCode" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Country" HeaderText="Country" 
                                                        SortExpression="Country" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="EMail" HeaderText="EMail" SortExpression="EMail" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="OnlineResourcesUrl" 
                                                        HeaderText="OnlineResourcesUrl" SortExpression="OnlineResourcesUrl" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="HoursOfService" HeaderText="HoursOfService" 
                                                        SortExpression="HoursOfService" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="ContactInstructions" HeaderText="ContactInstructions" 
                                                        SortExpression="ContactInstructions" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" >
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="ServiceURL" HeaderText="ServiceURL" 
                                                        SortExpression="ServiceURL" />
                                                    <asp:BoundField DataField="ServiceID" HeaderText="ServiceID" ReadOnly="True" SortExpression="ServiceID" />
                                                    <asp:BoundField DataField="Namespace" HeaderText="Namespace" SortExpression="Namespace" />
                                                    <asp:BoundField DataField="SchemaLocation" HeaderText="SchemaLocation" SortExpression="SchemaLocation" />
                                                    <asp:CommandField ButtonType="Image" CancelImageUrl="~/Images/Edit_UndoHS.png" 
                                                        CancelText="Avbryt" DeleteImageUrl="~/Images/delete_12x12.png" 
                                                        DeleteText="Slett" EditImageUrl="~/Images/EditTableHS.png" EditText="Rediger" 
                                                        InsertText="Sett inn" NewText="Ny" SelectImageUrl="~/Images/AddTableHS.png" 
                                                        SelectText="Velg" ShowEditButton="True" UpdateImageUrl="~/Images/saveHS.png" 
                                                        UpdateText="Lagre" />
                                                </Fields>
                                            </asp:DetailsView>
                                        </div>
                                        <div style="height: 10%">
                                            &nbsp;                                
                                        </div>
                                       
                                   </asp:View>
                                   <asp:View ID="vwDataset" runat="server">
                                        <div style="height: 10%">
                                            <asp:Label ID="Label1" runat="server" Text="Dataset:" 
                                                CssClass="TableHeaderText"></asp:Label>
                                        </div>                                               
                                        <div style="height: 50%">                                          
                                            <asp:DetailsView ID="vDataset" runat="server" AllowPaging="True" 
                                                CssClass="GridView" DataSourceID="edsDataset" Height="50px" Width="125px" 
                                                AutoGenerateRows="False" DataKeyNames="DatasetId" 
                                                onitemcreated="vDataset_ItemCreated" onprerender="vDataset_PreRender">
                                                <EditRowStyle CssClass="GridView" />
                                                <EmptyDataRowStyle CssClass="GridView" />
                                                <FieldHeaderStyle CssClass="FieldHeader" />
                                                <Fields>
                                                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                                    <asp:BoundField DataField="SchemaFileUri" HeaderText="SchemaFileUri" 
                                                        SortExpression="SchemaFileUri" />
                                                    <asp:BoundField DataField="DatasetProvider" HeaderText="DatasetProvider" 
                                                        SortExpression="DatasetProvider" />
                                                    <asp:BoundField DataField="ServerMaxCount" HeaderText="ServerMaxCount" 
                                                        SortExpression="ServerMaxCount" />
                                                    <asp:BoundField DataField="DatasetConnection" HeaderText="DatasetConnection" 
                                                        SortExpression="DatasetConnection" />
                                                    <asp:BoundField DataField="DBSchema" HeaderText="DBSchema" 
                                                        SortExpression="DBSchema" />
                                                    <asp:BoundField DataField="TransformationConnection" 
                                                        HeaderText="TransformationConnection" 
                                                        SortExpression="TransformationConnection" />
                                                    <asp:BoundField DataField="DefaultCrs" HeaderText="DefaultCrs" 
                                                        SortExpression="DefaultCrs" />
                                                    <asp:BoundField DataField="UpperCornerCoords" HeaderText="UpperCornerCoords" 
                                                        SortExpression="UpperCornerCoords" />
                                                    <asp:BoundField DataField="LowerCornerCoords" HeaderText="LowerCornerCoords" 
                                                        SortExpression="LowerCornerCoords" />
                                                    <asp:BoundField DataField="TargetNamespace" HeaderText="TargetNamespace" 
                                                        SortExpression="TargetNamespace" />
                                                         <asp:BoundField DataField="TargetNamespacePrefix" HeaderText="TargetNamespacePrefix" 
                                                        SortExpression="TargetNamespacePrefix" />
                                                    <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" 
                                                        ShowInsertButton="True" ButtonType="Image" 
                                                        CancelImageUrl="~/Images/Edit_UndoHS.png" CancelText="Avbryt" 
                                                        DeleteImageUrl="~/Images/delete_12x12.png" DeleteText="Slett" 
                                                        EditImageUrl="~/Images/EditTableHS.png" EditText="Rediger" 
                                                        InsertText="Lagre" NewImageUrl="~/Images/AddTableHS.png" NewText="Ny" 
                                                        SelectText="Velg" UpdateImageUrl="~/Images/saveHS.png" UpdateText="Lagre" 
                                                        InsertImageUrl="~/Images/saveHS.png" />
                                                </Fields>
                                                <InsertRowStyle CssClass="GridView" />
                                                <PagerSettings Mode="NextPreviousFirstLast" Position="Bottom" Visible="true" />
                                                <PagerTemplate>
                                                    <asp:ImageButton ID="btnFirst" runat="server" CommandName="First" OnCommand="ChangePage" CommandArgument="DL"
                                                        ImageUrl="~/Images/DataContainer_MoveFirstHS.png" /> 
                                                    <asp:ImageButton ID="btnPrev" runat="server" CommandName="Prev" OnCommand="ChangePage" CommandArgument="DL"
                                                        ImageUrl="~/Images/DataContainer_MovePreviousHS.png" />
                                                    <asp:ImageButton ID="btnNext" runat="server" CommandName="Next" OnCommand="ChangePage" CommandArgument="DL"
                                                        ImageUrl="~/Images/DataContainer_MoveNextHS.png" />
                                                    <asp:ImageButton ID="btnLast" runat="server" CommandName="Last" OnCommand="ChangePage" CommandArgument="DL"
                                                        ImageUrl="~/Images/DataContainer_MoveLastHS.png" />
                                                </PagerTemplate>
                                                <PagerStyle CssClass="GridView" />
                                            </asp:DetailsView>
                                        </div>
                                       <div style="height: 10%"> 
                                           &nbsp;
                                           &nbsp;
                                       </div>  
                                       <div style="height: 20%">
                                           <table width="50%">
                                                <tr>
                                            <td>
                                            <asp:Button ID="btnCreateInitialData" runat="server" Text="Opprett initielle data" CssClass="Button" OnClick="btnCreateInitialData_Click" Width="147px" />
                                                </td>
                                            <td width ="50%">

                                                <asp:Label ID="lblErrorText" runat="server" CssClass="ErrorLabel"></asp:Label>

                                            </td>
                                            </tr>
                                               </table>                                           
                                       </div>                                                                 
                                       <div style="height: 10%"> 
                                       </div>                          
                                    </asp:View>
                                    <asp:View ID="vwChangeLog" runat="server">
                                         <div id="divChangeLog" style="display: block; height: 30%;"  > 
                                            <div style="height: 10%">
                                                <asp:Label ID="Label3" runat="server" CssClass="TableHeaderText" 
                                                    Text="Endringslogg:"></asp:Label>
                                            </div>
                                            <div style="height: 80%">    
                                                <asp:GridView ID="gwStoredChangeLogs" runat="server" AllowSorting="True" 
                                                    DataSourceID="edsStoredChangeLogs" Width="100%" CssClass="GridView" 
                                                    AllowPaging="True" AutoGenerateColumns="False" DataKeyNames="ChangelogId" 
                                                    EnableSortingAndPagingCallbacks="True">
                                                    <Columns>
                                                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" >
                                                        <ControlStyle CssClass="FieldHeader" />
                                                    </asp:BoundField>
                                                        <asp:BoundField DataField="OrderUri" HeaderText="OrderUri" 
                                                            SortExpression="OrderUri" />
                                                        <asp:BoundField DataField="StartIndex" HeaderText="StartIndex" 
                                                            SortExpression="StartIndex" />
                                                        <asp:BoundField DataField="DownloadUri" HeaderText="DownloadUri" 
                                                            SortExpression="DownloadUri" />
                                                        <asp:BoundField DataField="EndIndex" HeaderText="EndIndex" 
                                                            SortExpression="EndIndex" />
                                                        <asp:BoundField DataField="Status" HeaderText="Status" 
                                                            SortExpression="Status" />
                                                        <asp:CheckBoxField DataField="Stored" HeaderText="Stored" 
                                                            SortExpression="Stored" />
                                                        <asp:BoundField DataField="ChangelogId" HeaderText="ChangelogId" 
                                                            ReadOnly="True" SortExpression="ChangelogId" />
                                                    </Columns>
                                                    <HeaderStyle CssClass="FieldHeader" />
                                                    <PagerSettings Mode="NextPreviousFirstLast" Position="Bottom" Visible="true" />
                                                    <PagerTemplate>
                                                        <asp:ImageButton ID="btnFirstCL" runat="server" CommandName="First" OnCommand="ChangePage" CommandArgument="CL"
                                                            ImageUrl="~/Images/DataContainer_MoveFirstHS.png" /> 
                                                        <asp:ImageButton ID="btnPrevCL" runat="server" CommandName="Prev" OnCommand="ChangePage" CommandArgument="CL"
                                                            ImageUrl="~/Images/DataContainer_MovePreviousHS.png" />
                                                        <asp:ImageButton ID="btnNextCL" runat="server" CommandName="Next" OnCommand="ChangePage" CommandArgument="CL"
                                                            ImageUrl="~/Images/DataContainer_MoveNextHS.png" />
                                                        <asp:ImageButton ID="btnLastCL" runat="server" CommandName="Last" OnCommand="ChangePage" CommandArgument="CL"
                                                            ImageUrl="~/Images/DataContainer_MoveLastHS.png" />
                                                    </PagerTemplate>
                                                    <PagerStyle CssClass="GridView" />
                                                </asp:GridView>
    
                                            </div>
                                             <div style="height: 10%">
                                            &nbsp;
                                        </div>
                                        </div>                                      
                                    </asp:View>
                                </asp:MultiView>
                            </td>
                            <td>
                                &nbsp;
                                &nbsp;
                            </td>
                        </tr>                            
                     </table>
                         &nbsp;
                    </div>                  
                   
                   
               
                <td width="10%">
                 &nbsp;
                </td>
            </tr>
            <tr>
            <td>
              &nbsp;
              </td>
            </tr>
        </table>
      </asp:Panel>
    <asp:EntityDataSource ID="edsDataset" runat="server" 
        ConnectionString="name=geosyncEntities" DefaultContainerName="geosyncEntities" 
        EnableFlattening="False" EntitySetName="Datasets" EnableDelete="True" 
        EnableInsert="True" EnableUpdate="True">
    </asp:EntityDataSource>
    <asp:EntityDataSource ID="edsStoredChangeLogs" runat="server" 
        ConnectionString="name=geosyncEntities" DefaultContainerName="geosyncEntities" 
        EnableFlattening="False" EntitySetName="StoredChangelogs">
    </asp:EntityDataSource>
    <asp:EntityDataSource ID="edsServerConfig" runat="server" 
        ConnectionString="name=geosyncEntities" DefaultContainerName="geosyncEntities" 
        EnableFlattening="False" EnableUpdate="True" EntitySetName="ServerConfigs"> 
    </asp:EntityDataSource>
    <asp:EntityDataSource ID="edsService" runat="server" 
        ConnectionString="name=geosyncEntities" DefaultContainerName="geosyncEntities" 
        EnableFlattening="False" EnableUpdate="True" EntitySetName="Services">
    </asp:EntityDataSource>
    </form>
</body>
</html>
