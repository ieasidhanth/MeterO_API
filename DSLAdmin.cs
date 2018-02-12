using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.ComponentModel;

namespace ViewPointAPI
{
    [DataObject]
    public class DSLAdmin
    {
        public DSLAdmin()
        {

        }
        [DataObjectMethod(DataObjectMethodType.Select)]
        public DataTable getApplications()
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {

                IeaPoweAppsconn.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter adap = new SqlDataAdapter("Select * from [dbo].Application", IeaPoweAppsconn);

                adap.Fill(dt);
                return dt;
            }

        }
        [DataObjectMethod(DataObjectMethodType.Insert)]
        public int InsertApplication(int App_ID,string App_Name, string App_Description)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                SqlCommand cmd = new SqlCommand("Insert into [dbo].[Application] ([App_Name],[App_Description]) VALUES(@AppName,@AppDesc) ", IeaPoweAppsconn);
                cmd.Parameters.AddWithValue("@AppName", App_Name);
                cmd.Parameters.AddWithValue("@AppDesc", App_Description);
                try
                {
                    int status = cmd.ExecuteNonQuery();
                    return status;


                }
                catch (SqlException ex)
                {
                    return -1;
                }
                finally
                {
                    IeaPoweAppsconn.Close();
                }


            }
        }
        [DataObjectMethod(DataObjectMethodType.Update)]
        public int UpdateApplication(int App_ID, string App_Name, string App_Description)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                SqlCommand cmd = new SqlCommand("Update [dbo].[Application] Set [App_Name]=@AppName,[App_Description]=@AppDesc where App_ID=@App_ID) ", IeaPoweAppsconn);
                cmd.Parameters.AddWithValue("@AppName", App_Name);
                cmd.Parameters.AddWithValue("@AppDesc", App_Description);
                cmd.Parameters.AddWithValue("@App_ID", App_ID);
                try
                {
                    int status = cmd.ExecuteNonQuery();
                    return status;


                }
                catch (SqlException ex)
                {
                    return -1;
                }
                finally
                {
                    IeaPoweAppsconn.Close();
                }


            }
        }
    }
}