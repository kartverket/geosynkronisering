﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GeosynkroniseringAdmin.aspx.cs" Inherits="Kartverket.Geosynkronisering.GeosynkroniseringAdmin" ValidateRequest="false" EnableEventValidation="false"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<link rel="stylesheet" type="text/css" href="GeoSynkWebStyleSheet.css">

<link href="GeoSynkWebStyleSheet.css" rel="stylesheet" type="text/css" />
<script language="javascript" type="text/javascript">

    function setActiveView(mviewID, view) {
        document.getElementById(mviewID).setAttribute("ActiveView", view);
    }

</script>
<head runat="server">
    <title>Geosynkronisering Administrator</title>
    <%--if CTRL+SHIFT+t is pressed, set btnSendmail visible for testing--%>
    <%--Add a hidden control  txtHiddenLogfileControl to detect CTLR pressed for btnLogfile CLicked--%>

    <script type="text/javascript">
        window.addEventListener("keypress", KeyPressed);
        setTimeout(function () {
            document.getElementById('<%= btnSendmail.ClientID %>').style.visibility = "hidden";
        }, 100);

        function KeyPressed() {
            console.log(event.key, event.code, event.keyCode);
            var f2Key = 113;
            if (event.code == "KeyS" && event.ctrlKey && event.shiftKey) {
                document.getElementById('<%= btnSendmail.ClientID %>').style.visibility = "visible";
            }
            else if (event.keyCode == f2Key && event.ctrlKey && event.shiftKey) {
                document.getElementById('<%= btnSendmail.ClientID %>').style.visibility = "visible";
            }
        }

        // Works, but a new browser window will be started, so skip this.
        // window.addEventListener("mouseup", mouseKeyPressed);
        function mouseKeyPressed() {
            if (event.ctrlKey) {
                document.getElementById('<%= txtHiddenLogfileControl.ClientID %>').value = 'ctl_clicked';
            } else {
                document.getElementById('<%= txtHiddenLogfileControl.ClientID %>').value = '';
            }
        }


    </script>
</head>
<body>
    <form id="Administrator" runat="server" name="Geosynkronisering">
        <asp:Panel ID="Panel2" runat="server" CssClass="PanelBlank" OnLoad="Panel2_Load">


            <table style="width: 100%;">
                <tr style="height: 10%">
                    <td width="10%">
                        <asp:Image ID="Image1" runat="server" Height="61px"
                            ImageUrl="~/Images/GeosynkAlphaBig2.png" Width="64px" />
                    </td>
                    <td width="60%">
                        <asp:Label ID="Label2" runat="server" CssClass="HeaderText"
                            Text="Geosynkronisering Administrator 2.0 Beta"></asp:Label>
                            <%--Text="Geosynkronisering Administrator 1.2.3"></asp:Label>--%>
                    </td>
                    <td width="20%">&nbsp;</td>
                    <td width="10%"></td>
                </tr>
                <tr>
                    <td width="10%"></td>
                    <td width="80%" colspan="0">
                        <table style="width: 100%;">
                            <tr valign="baseline">
                                <td align="left" colspan="0" valign="baseline">
                                    <asp:LinkButton ID="lbtnConfig" runat="server" CssClass="LinkButtonSelected"
                                        CommandName="0" OnClick="lbtn_Click">Konfigurasjon</asp:LinkButton>
                                    <asp:LinkButton ID="lbtnDataset" runat="server" CssClass="LinkButton"
                                        CommandName="1" OnClick="lbtn_Click">Datasett</asp:LinkButton>
                                    <asp:LinkButton ID="lbtnChangeLog" runat="server" CssClass="LinkButton"
                                        CommandName="2" OnClick="lbtn_Click">Endringslogg</asp:LinkButton>
                                </td>
                            </tr>
                        </table>
                        <div style="height: 100%">
                            <table style="width: 100%;">
                                <tr>
                                    <td>&nbsp;
                                    </td>
                                    <td>
                                        <asp:MultiView ID="mvwViews" runat="server" ActiveViewIndex="0">
                                            <asp:View ID="vwConfig" runat="server" OnLoad="vwConfig_Load">
                                                <div style="height: 10%">
                                                    <asp:Label ID="Label4" runat="server" Text="Nedlasting:"
                                                        CssClass="TableHeaderText"></asp:Label>
                                                </div>
                                                <div style="height: 80%">
                                                    <asp:DetailsView ID="dvServerConfig" runat="server" AutoGenerateRows="False"
                                                        CssClass="GridView" DataKeyNames="ID"
                                                        Height="50px" Width="100%"
                                                                     OnModeChanging="dvServerConfig_Modechanging"
                                                                     OnItemUpdating="dvServerConfig_Updating"
                                                                     EnableViewState ="False">
                                                        <EditRowStyle CssClass="GridView" />
                                                        <EmptyDataRowStyle CssClass="GridView" />
                                                        <FieldHeaderStyle CssClass="FieldHeader" />
                                                        <Fields>
                                                            <asp:BoundField DataField="FTPUrl" HeaderText="Download URL"
                                                                SortExpression="FTPUrl">
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
                                                    <asp:Label ID="Label5" runat="server" CssClass="TableHeaderText" Text="Service:"></asp:Label>
                                                    <asp:DetailsView ID="dvService" runat="server" AutoGenerateRows="False" CssClass="GridView" DataKeyNames="ServiceID" Height="50px" Width="100%"
                                                                     OnModeChanging="dvService_Modechanging"
                                                                     OnItemUpdating="dvService_Updating"
                                                                     EnableViewState ="False">
                                                        <EditRowStyle CssClass="GridView" />
                                                        <FieldHeaderStyle CssClass="FieldHeader" />
                                                        <Fields>
                                                            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                                                            <asp:BoundField DataField="Abstract" HeaderText="Abstract" SortExpression="Abstract" />
                                                            <asp:BoundField DataField="Keywords" HeaderText="Keywords" SortExpression="Keywords" />
                                                            <asp:BoundField DataField="Fees" HeaderText="Fees" SortExpression="Fees" />
                                                            <asp:BoundField DataField="AccessConstraints" HeaderText="AccessConstraints" SortExpression="AccessConstraints" />
                                                            <asp:BoundField DataField="ProviderName" HeaderText="ProviderName" SortExpression="ProviderName" />
                                                            <asp:BoundField DataField="ProviderSite" HeaderText="ProviderSite" SortExpression="ProviderSite" />
                                                            <asp:BoundField DataField="IndividualName" HeaderText="IndividualName" SortExpression="IndividualName" />
                                                            <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                                                            <asp:BoundField DataField="Facsimile" HeaderText="Facsimile" SortExpression="Facsimile" />
                                                            <asp:BoundField DataField="Deliverypoint" HeaderText="Deliverypoint" SortExpression="Deliverypoint" />
                                                            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
                                                            <asp:BoundField DataField="PostalCode" HeaderText="PostalCode" SortExpression="PostalCode" />
                                                            <asp:BoundField DataField="Country" HeaderText="Country" SortExpression="Country" />
                                                            <asp:BoundField DataField="EMail" HeaderText="EMail" SortExpression="EMail" />
                                                            <asp:BoundField DataField="OnlineResourcesUrl" HeaderText="OnlineResourcesUrl" SortExpression="OnlineResourcesUrl" />
                                                            <asp:BoundField DataField="HoursOfService" HeaderText="HoursOfService" SortExpression="HoursOfService" />
                                                            <asp:BoundField DataField="ContactInstructions" HeaderText="ContactInstructions" SortExpression="ContactInstructions" />
                                                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                                            <asp:BoundField DataField="ServiceURL" HeaderText="ServiceURL" SortExpression="ServiceURL" />
                                                            <asp:BoundField DataField="ServiceID" HeaderText="ServiceID" ReadOnly="True" SortExpression="ServiceID" />
                                                            <asp:BoundField DataField="Namespace" HeaderText="Namespace" SortExpression="Namespace" />
                                                            <asp:BoundField DataField="SchemaLocation" HeaderText="SchemaLocation" SortExpression="SchemaLocation" />
                                                            <asp:CommandField ButtonType="Image" CancelImageUrl="~/Images/Edit_UndoHS.png" CancelText="Avbryt" DeleteImageUrl="~/Images/delete_12x12.png" DeleteText="Slett" EditImageUrl="~/Images/EditTableHS.png" EditText="Rediger" InsertText="Sett inn" NewText="Ny" SelectImageUrl="~/Images/AddTableHS.png" SelectText="Velg" ShowEditButton="True" UpdateImageUrl="~/Images/saveHS.png" UpdateText="Lagre" />
                                                        </Fields>
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
                                                                <asp:Button ID="btnLogfile" runat="server" Text="Vis loggfil" CssClass="Button" OnClick="btnLogfile_Click" Width="147px" ToolTip="Viser logg med varslinger fra abonnent" />
                                                                <input type="hidden" runat="server" id="txtHiddenLogfileControl" />
                                                            </td>
                                                            <td>
                                                                <asp:Button ID="btnClearLogfile" runat="server" Text="Tøm loggfil" CssClass="Button" OnClick="btnClearLogfile_Click" Width="147px" ToolTip="Tøm logg med varslinger fra abonnent" />
                                                            </td>
                                                            <td>
                                                                <asp:Button ID="btnSendmail" runat="server" Text="Sendmail" CssClass="Button" OnClick="btnSendmail_Click" Width="147px" ToolTip="Test Sendmail" />
                                                            </td>
                                                            <td width="50%">
                                                                <asp:Label ID="lblLogfileText" runat="server" CssClass="ErrorLabel"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                                <div style="height: 10%">
                                                    <asp:TextBox ID="TextBoxLogfile" runat="server" TextMode="MultiLine" ReadOnly="True" Visible="False" Width="90%"></asp:TextBox>
                                                </div>
                                            </asp:View>
                                            <asp:View ID="vwDataset" runat="server" OnLoad="vwDataset_Load">>
                                                <div style="height: 10%">
                                                    <asp:Label ID="Label1" runat="server" Text="Dataset:"
                                                        CssClass="TableHeaderText"></asp:Label>
                                                </div>
                                                <div style="height: 50%">
                                                    <asp:DetailsView ID="vDataset" runat="server" AllowPaging="True"
                                                        CssClass="GridView" Height="50px" Width="100%"
                                                        AutoGenerateRows="False" DataKeyNames="DatasetId"
                                                        OnItemCreated="vDataset_ItemCreated" OnPreRender="vDataset_PreRender" OnItemUpdating="vDataset_ItemUpdating"
                                                                     AutoGenerateInsertButton="false"
                                                                     AutoGenerateEditButton="false" 
                                                                     AutoGenerateDeleteButton="false"
                                                                     EnableViewState ="False"
                                                                     OnModeChanging="vDataset_Modechanging" OnItemUpdated="vDataset_ItemUpdated" OnItemCommand="vDataset_ItemCommand"
                                                                     OnItemDeleting="vDataset_ItemDeleteting" OnItemDeleted="vDataset_ItemDeleteted"
                                                                     OnItemInserting="vDataset_ItemInserting">
                                                        <EditRowStyle CssClass="GridView" />
                                                        <EmptyDataRowStyle CssClass="GridView" />
                                                        <FieldHeaderStyle CssClass="FieldHeader" />
                                                        <Fields>
                                                            <asp:BoundField DataField="DatasetId" HeaderText="DatasetId" ReadOnly="True" InsertVisible="True"/>
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
                                                            <asp:BoundField DataField="Version" HeaderText="Version" SortExpression="Version" />
                                                            <asp:BoundField DataField="Tolerance" HeaderText="Tolerance" />
                                                            <asp:BoundField DataField="Decimals" HeaderText="Decimals" />
                                                            <asp:CommandField ButtonType="Image" CancelImageUrl="~/Images/Edit_UndoHS.png" CancelText="Avbryt" DeleteImageUrl="~/Images/delete_12x12.png" DeleteText="Slett" EditImageUrl="~/Images/EditTableHS.png" EditText="Rediger" InsertImageUrl="~/Images/saveHS.png" InsertText="Lagre" NewImageUrl="~/Images/AddTableHS.png" NewText="Ny" SelectText="Velg" ShowDeleteButton="True" ShowEditButton="True" ShowInsertButton="True" UpdateImageUrl="~/Images/saveHS.png" UpdateText="Lagre"/>
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
                                                            <td>
                                                                <asp:Button ID="btnLErrorfile" runat="server" Text="Vis loggfil" CssClass="Button" OnClick="btnLErrorfile_Click" Width="147px" ToolTip="Viser feillogg for initiell endringslogg" Visible="False" />
                                                            </td>
                                                            <td width="50%">

                                                                <asp:Label ID="lblErrorText" runat="server" CssClass="ErrorLabel"></asp:Label>

                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                                <div style="height: 10%">
                                                    <asp:TextBox ID="TextBoxErrorfile" runat="server" TextMode="MultiLine" ReadOnly="True" Visible="False" Width="90%"></asp:TextBox>
                                                </div>
                                            </asp:View>
                                            <asp:View ID="vwChangeLog" runat="server">
                                                <div id="divChangeLog" style="display: block; height: 30%;">
                                                    <div style="height: 10%">
                                                        <asp:Label ID="Label3" runat="server" CssClass="TableHeaderText"
                                                            Text="Endringslogg:"></asp:Label>
                                                    </div>
                                                    <div style="height: 80%">
                                                        <asp:GridView ID="gwStoredChangeLogs" runat="server" AllowSorting="True" Width="100%" CssClass="GridView"
                                                            AllowPaging="True" AutoGenerateColumns="False" DataKeyNames="ChangelogId"
                                                            EnableSortingAndPagingCallbacks="True">
                                                            <Columns>
                                                                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name">
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
                                                                <asp:BoundField DataField="DateCreated" HeaderText="DateCreated"
                                                                                ReadOnly="True" SortExpression="DateCreated" />
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
                                    <td>&nbsp;
                                &nbsp;
                                    </td>
                                </tr>
                            </table>
                            &nbsp;
                        </div>



                        <td width="10%">&nbsp;
                        </td>
                </tr>
                <tr>
                    <td>&nbsp;
                    </td>
                </tr>
            </table>
        </asp:Panel>
  <%--      <asp:EntityDataSource ID="edsDataset" runat="server"
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
        </asp:EntityDataSource>--%>
    </form>
</body>
</html>
