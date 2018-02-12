<%@ Page Title="" Language="C#" MasterPageFile="~/master_page.Master" AutoEventWireup="true" CodeBehind="Applications.aspx.cs" Inherits="ViewPointAPI.Admin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p style="font-size: x-large">
        <strong><em>IEA Power Apps Applications</em></strong></p>
    <p>
        &nbsp;</p>
    <p>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4" DataSourceID="ODBApplications" ForeColor="#333333" GridLines="None" Width="1037px" AllowPaging="True" AllowSorting="True">
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            <Columns>
                <asp:CommandField ShowEditButton="True" />
                <asp:BoundField DataField="App_ID" HeaderText="Application ID" ReadOnly="True" />
                <asp:BoundField DataField="App_Name" HeaderText="Application Name" />
                <asp:BoundField DataField="App_Description" HeaderText="Application Description" />
            </Columns>
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
        </asp:GridView>
    </p>
    <p>
        &nbsp;</p>
    <p>
        <asp:DetailsView ID="DetailsView1" runat="server" AllowPaging="True" AutoGenerateRows="False" CellPadding="4" DataSourceID="ODBApplications" ForeColor="#333333" GridLines="None" Height="50px" Width="1043px">
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            <CommandRowStyle BackColor="#E2DED6" Font-Bold="True" />
            <EditRowStyle BackColor="#999999" />
            <FieldHeaderStyle BackColor="#E9ECF1" Font-Bold="True" />
            <Fields>
                <asp:CommandField ShowInsertButton="True" />
                <asp:BoundField DataField="App_ID" HeaderText="Application ID" ReadOnly="True" Visible="False" />
                <asp:BoundField DataField="App_Name" HeaderText="Application Name" />
                <asp:BoundField DataField="App_Description" HeaderText="Application Description" />
            </Fields>
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        </asp:DetailsView>
    </p>
    <p>
        <asp:ObjectDataSource ID="ODBApplications" runat="server" InsertMethod="InsertApplication" SelectMethod="getApplications" TypeName="ViewPointAPI.DSLAdmin" UpdateMethod="UpdateApplication">
            <InsertParameters>
                <asp:Parameter Name="App_ID" Type="Int32" />
                <asp:Parameter Name="App_Name" Type="String" />
                <asp:Parameter Name="App_Description" Type="String" />
            </InsertParameters>
            <UpdateParameters>
                <asp:Parameter Name="App_ID" Type="Int32" />
                <asp:Parameter Name="App_Name" Type="String" />
                <asp:Parameter Name="App_Description" Type="String" />
            </UpdateParameters>
        </asp:ObjectDataSource>
    </p>
</asp:Content>
