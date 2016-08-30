<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CollectionReport.aspx.cs" Inherits="SMS.Report.CollectionReport" %>

<%@ register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:scriptmanager runat="server"></asp:scriptmanager>
        <rsweb:reportviewer id="rptCollectionReportViewer" runat="server"></rsweb:reportviewer>
    </div>
    </form>
</body>
</html>
