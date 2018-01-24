using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using System.Web.Script.Serialization;
using System.Text;
using System.Timers;
using System.Web.Configuration;

namespace ViewPointAPI
{
    //class to access Data from viewPoint database
    public class DSL_MeterO
    {
        //connection object
        private SqlConnection sqlconn;
        private SqlConnection transitDBconn;
        private SqlConnection IEA_PowerApps;

        public DSL_MeterO()
        {
            //sqlconn = null;
            //Connection conn = new Connection();
            //sqlconn = conn.initiateConnection();
            //transitDBconn = conn.InitiateTransitDBConncetion();
            //IEA_PowerApps = conn.InitiatePowerAppsDBConnection();


        }
       
       //gets all jobs in company 53  
        public DataTable getAllJobs()
        {
            //Gets all Job with equipments
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                VpointDBConn.Open();
                SqlDataAdapter adap = new SqlDataAdapter("Select Distinct([Job]), [Description] from [Viewpoint].dbo.JCJM where Job in (SELECT Distinct([Job])  FROM[Viewpoint].[dbo].[EMEM]   where EMCo = 53) and JCCo=53 order by Job; ", VpointDBConn);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                VpointDBConn.Dispose();
                return dt;
            }
            


        }
        //gets equipments with job id
        public DataTable getEquipments(string JobID)
        {
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                VpointDBConn.Open();
                SqlDataAdapter adap = new SqlDataAdapter("SELECT [Equipment],[VINNumber] as SerialNo,[Description],[LicensePlateNo] as LicenseNumber,[HourReading],[OdoReading] ,[udJobSiteAssignment] as JobAssign ,[udReferenceNumber] from Viewpoint.dbo.EMEM where Job='" + JobID + "' and EMCo='53'", VpointDBConn);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                VpointDBConn.Dispose();
                dt.Columns.Add("NewHr", typeof(string));
                dt.Columns.Add("NewOdo", typeof(string));
                dt.Columns.Add("Job", typeof(string));
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("CreatedDate", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    row["NewHr"] = "";
                    row["NewOdo"] = "";
                    row["Job"] = "";
                    row["CreatedBy"] = "";
                    row["CreatedDate"] = "";

                }
                return dt;
            }

               

        }
        public DataTable reviewEquipmentEntry(string JobID)
        {
            
            DataTable dt = new DataTable();
            dt.Columns.Add("JobDesc", typeof(string));
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                // do some stuff
                SqlDataAdapter adap = new SqlDataAdapter("SELECT * from dbo.vreviewSubmitData where Job='" + JobID + "'", IeaPoweAppsconn);
                adap.Fill(dt);
                IeaPoweAppsconn.Dispose();

            }
            
            
           
            DataTable dt_viewpoint = new DataTable();
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                VpointDBConn.Open();
                SqlDataAdapter vpointadap = new SqlDataAdapter("Select * from Viewpoint.dbo.EMEM where Job='" + JobID + "'", VpointDBConn);
                vpointadap.Fill(dt_viewpoint);
                vpointadap.Dispose();

            }
                
            foreach(DataRow drow in dt.Rows)
            {
                string equipment = Convert.ToString(drow["Equipment"]);
                foreach(DataRow viewpointRow in dt_viewpoint.Rows)
                {
                    if(Convert.ToString(viewpointRow["Equipment"])==equipment)
                    {
                        double vpointHrReading = Convert.ToDouble(viewpointRow["HourReading"]);
                        double MeterOHrReading = Convert.ToDouble(drow["NewHr"]);
                        double vpointOdoReading = Convert.ToDouble(viewpointRow["OdoReading"]);
                        double MeterOodoReading = Convert.ToDouble(drow["NewOdo"]);
                        if ((vpointHrReading==MeterOHrReading) && (vpointOdoReading == MeterOodoReading ))
                        {
                            if(deleteRow(Convert.ToInt32(drow["ID"]))>0)
                            {
                                dt.Rows.Remove(drow);
                            }

                        }
                    }
                }
            }
            
           
            return dt;

        }
        public int deleteRow(int ID)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                string query = "Delete from MeterOTransaction where ID=" + ID;
                SqlCommand cmd = new SqlCommand(query, IeaPoweAppsconn);
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
                    IeaPoweAppsconn.Dispose();
                }
            }
            
        }
      
  







        public string convertToCSV(JArray json)
        {
            try
            {
                
                DataTable dt = new DataTable();
                dt.Columns.Add("Equipment", typeof(string));
                dt.Columns.Add("Blank", typeof(string));
                dt.Columns.Add("New Recorded Hours", typeof(string));
                dt.Columns.Add("DateIn", typeof(string));
                dt.Columns.Add("Blank1", typeof(string));
                dt.Columns.Add("New Recorded OdoMeter", typeof(string));
                dt.Columns.Add("Job", typeof(string));
                foreach (JObject obj in json)
                {
                    DataRow row = dt.NewRow();
                    row[0] = Convert.ToString(obj["Equipment"]);
                    row[1] = Convert.ToString(obj["Blank"]);
                    row[2] = Convert.ToString(obj["New Recorded Hours"]);
                    row[3] = Convert.ToString(obj["DateIn"]);
                    row[4] = Convert.ToString(obj["Blank1"]);
                    row[5] = Convert.ToString(obj["New Recorded OdoMeter"]);
                    row[6] = Convert.ToString(obj["Job"]);
                    dt.Rows.Add(row);


                }
           
                string csv = table_to_csv(dt);
                return csv;

            }
            catch (Exception ex)
            {
                return "failed";
            }
            
            

        }
        //for exporting list
        public string convertToCSVList(JArray json)
        {
            try
            {

                DataTable dt = new DataTable();
                
                dt.Columns.Add("Equipment", typeof(string));
                dt.Columns.Add("SerialNo", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("License_No", typeof(string));
                dt.Columns.Add("Job_Assigned", typeof(string));
                dt.Columns.Add("Last Recorderd Hours", typeof(string));
                dt.Columns.Add("Last Recorded Odometer", typeof(string));
                dt.Columns.Add("New Recorded Hours", typeof(string));
                dt.Columns.Add("New Odometer Reading", typeof(string));
                
                
                foreach (JObject obj in json)
                {
                    DataRow row = dt.NewRow();
                    row[0] = Convert.ToString(obj["Equipment"]);
                    row[1] = Convert.ToString(obj["SerialNo"]);
                    row[2] = Convert.ToString(obj["Description"]);
                    row[3] = Convert.ToString(obj["License_No"]);
                    row[4] = Convert.ToString(obj["Job_Assigned"]);
                    row[5] = Convert.ToString(obj["Last Recorderd Hours"]);
                    row[6] = Convert.ToString(obj["Last Recorded Odometer"]);
                    
                    row[7] = Convert.ToString(obj["New Recorded Hours"]);
                    row[8] = Convert.ToString(obj["New Odometer Reading"]);
                   
                    dt.Rows.Add(row);


                }

                string csv = ToCSV(dt,",");
                return csv;

            }
            catch (Exception ex)
            {
                return "failed";
            }



        }

        public string ToCSV(DataTable table, string delimator)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
            }
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(CsvEscape(row[i].ToString()));
                    result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
                }
            }
            return result.ToString().TrimEnd(new char[] { '\r', '\n' });
            //return result.ToString();
        }
        public string CsvEscape(string value)
        {
            if (value.Contains(","))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }



        public string table_to_csv(DataTable table)
        {
            string file = "";

          

            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                    file = string.Concat(file, item.ToString(), ",");

                file = file.Remove(file.LastIndexOf(','), 1);
                file = string.Concat(file, "\r\n");
            }

            return file;
        }

        public string table_to_csv_withHeader(DataTable table)
        {
            string file = "";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                file = string.Concat(file, table.Columns[i].ColumnName.ToString(), ",");
                file = file.Remove(file.LastIndexOf(','), 1);
                file = string.Concat(file, "\r\n");

            }


            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                    file = string.Concat(file, item.ToString(), ",");

                file = file.Remove(file.LastIndexOf(','), 1);
                file = string.Concat(file, "\r\n");
            }

            return file;
        }


        
        public int saveTransaction(JArray json)
        {
            Boolean status = false;
            foreach(JObject eq in json)
            {
                status = processEquipments(eq);
                if(status==false)
                {
                    return  -1;

                }
               
            }
            if (status)
                return 1;
            else
                return -1;
            
        }


        public Boolean processEquipments(JObject equipment )
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                string query = "Select * from dbo.MeterOTransaction where Equipment='" + equipment["Equipment"] + "' and SerialNo='" + equipment["SerialNo"] + "'";
                Boolean insertStatus = false;
                SqlCommand cmd = new SqlCommand(query, IeaPoweAppsconn);
                SqlDataAdapter adap = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(adap);

                    foreach (DataRow row in dt.Rows)
                    {
                        row["NewHr"] = equipment["NewHr"];
                        row["Equipment"] = equipment["Equipment"];
                        row["SerialNo"] = equipment["SerialNo"];
                        row["Description"] = equipment["Description"];
                        row["LicenseNumber"] = equipment["LicenseNumber"];
                        row["HourReading"] = equipment["HourReading"];
                        row["OdoReading"] = equipment["OdoReading"];
                        row["udJobSiteAssignment"] = equipment["udJobAssignment"];
                        row["udReferenceNumber"] = equipment["udReferenceNumber"];
                        row["NewHr"] = equipment["NewHr"];
                        row["NewOdo"] = equipment["NewOdo"];
                        row["Job"] = equipment["Job"];
                        row["CreatedBy"] = equipment["CreatedBy"];
                        row["CreatedDateTime"] = equipment["CreatedDateTime"];

                    }
                    try
                    {
                        adap.Update(dt);
                        insertStatus = true;
                        return insertStatus;
                    }
                    catch (SqlException ex)
                    {
                        insertStatus = false;
                        return insertStatus;
                    }



                }
                else
                {
                    SqlBulkCopy bulkCopy = new SqlBulkCopy(IeaPoweAppsconn);
                    DataTable table = new DataTable();
                    DataColumn ID = new DataColumn("ID", typeof(Int32));
                    DataColumn MBatchID = new DataColumn("MBatchID", typeof(Int32));
                    DataColumn Equipment = new DataColumn("Equipment", typeof(string));
                    DataColumn SerialNo = new DataColumn("SerialNo", typeof(string));

                    DataColumn Description = new DataColumn("Description", typeof(string));
                    DataColumn LicenseNumber = new DataColumn("LicenseNumber", typeof(string));
                    DataColumn HourReading = new DataColumn("HourReading", typeof(string));
                    DataColumn OdoReading = new DataColumn("OdoReading", typeof(string));
                    DataColumn udJobSiteAssignment = new DataColumn("udJobSiteAssignment", typeof(string));
                    DataColumn udReferenceNumber = new DataColumn("udReferenceNumber", typeof(string));
                    DataColumn NewHr = new DataColumn("NewHr", typeof(string));
                    DataColumn NewOdo = new DataColumn("NewOdo", typeof(string));
                    DataColumn Job = new DataColumn("Job", typeof(string));
                    DataColumn CreatedBy = new DataColumn("CreatedBy", typeof(string));
                    DataColumn CreatedDateTime = new DataColumn("CreatedDateTime", typeof(string));

                    table.Columns.Add(ID);
                    table.Columns.Add(MBatchID);
                    table.Columns.Add(Equipment);
                    table.Columns.Add(SerialNo);

                    table.Columns.Add(Description);
                    table.Columns.Add(LicenseNumber);
                    table.Columns.Add(HourReading);
                    table.Columns.Add(OdoReading);
                    table.Columns.Add(udJobSiteAssignment);
                    table.Columns.Add(udReferenceNumber);
                    table.Columns.Add(NewHr);
                    table.Columns.Add(NewOdo);
                    table.Columns.Add(Job);
                    table.Columns.Add(CreatedBy);
                    table.Columns.Add(CreatedDateTime);

                    SqlCommand cmd1 = new SqlCommand("select max(MBatchID) from dbo.MeterOTransaction;", IeaPoweAppsconn);
                    var obj = cmd1.ExecuteScalar();
                    int nextBatchId = 0;
                    if (obj == DBNull.Value)
                    {
                        nextBatchId = 1000;
                    }
                    else
                    {
                        nextBatchId = ((int)obj) + 1;

                    }

                    DataRow row1 = table.NewRow();
                    row1["MBatchID"] = nextBatchId;
                    row1["Equipment"] = equipment["Equipment"];
                    row1["SerialNo"] = equipment["SerialNo"];
                    row1["Description"] = equipment["Description"];
                    row1["LicenseNumber"] = equipment["LicenseNumber"];
                    row1["HourReading"] = equipment["HourReading"];
                    row1["OdoReading"] = equipment["OdoReading"];
                    row1["udJobSiteAssignment"] = equipment["udJobAssignment"];
                    row1["udReferenceNumber"] = equipment["udReferenceNumber"];
                    row1["NewHr"] = equipment["NewHr"];
                    row1["NewOdo"] = equipment["NewOdo"];
                    row1["Job"] = equipment["Job"];
                    row1["CreatedBy"] = equipment["CreatedBy"];
                    row1["CreatedDateTime"] = equipment["CreatedDateTime"];
                    table.Rows.Add(row1);
                    //nextJobId = nextJobId + 1;






                    bulkCopy.DestinationTableName = "dbo.MeterOTransaction";
                    try
                    {
                        bulkCopy.WriteToServer(table);
                        insertStatus = true;
                        IeaPoweAppsconn.Dispose();
                        return insertStatus;

                    }
                    catch (SqlException ex)
                    {
                        insertStatus = false;
                        IeaPoweAppsconn.Dispose();
                        return insertStatus;

                    }

                }
            }
                


        }
        public int InsertEquipment(JArray json)
        {

            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                SqlBulkCopy bulkCopy = new SqlBulkCopy(IeaPoweAppsconn);
                DataTable dt = new DataTable();
                DataColumn ID = new DataColumn("ID", typeof(Int32));
                DataColumn MBatchID = new DataColumn("MBatchID", typeof(Int32));
                DataColumn Equipment = new DataColumn("Equipment", typeof(string));
                DataColumn SerialNo = new DataColumn("SerialNo", typeof(string));

                DataColumn Description = new DataColumn("Description", typeof(string));
                DataColumn LicenseNumber = new DataColumn("LicenseNumber", typeof(string));
                DataColumn HourReading = new DataColumn("HourReading", typeof(string));
                DataColumn OdoReading = new DataColumn("OdoReading", typeof(string));
                DataColumn udJobAssignment = new DataColumn("udJobAssignment", typeof(string));
                DataColumn udReferenceNumber = new DataColumn("udReferenceNumber", typeof(string));
                DataColumn NewHr = new DataColumn("NewHr", typeof(string));
                DataColumn NewOdo = new DataColumn("NewOdo", typeof(string));
                DataColumn Job = new DataColumn("Job", typeof(string));
                DataColumn CreatedBy = new DataColumn("CreatedBy", typeof(string));
                DataColumn CreatedDateTime = new DataColumn("CreatedDateTime", typeof(string));

                dt.Columns.Add(ID);
                dt.Columns.Add(MBatchID);
                dt.Columns.Add(SerialNo);
                dt.Columns.Add(Equipment);
                dt.Columns.Add(Description);
                dt.Columns.Add(LicenseNumber);
                dt.Columns.Add(HourReading);
                dt.Columns.Add(OdoReading);
                dt.Columns.Add(udJobAssignment);
                dt.Columns.Add(udReferenceNumber);
                dt.Columns.Add(NewHr);
                dt.Columns.Add(NewOdo);
                dt.Columns.Add(Job);
                dt.Columns.Add(CreatedBy);
                dt.Columns.Add(CreatedDateTime);

                SqlCommand cmd = new SqlCommand("select max(MBatchID) from dbo.MeterOTransaction;", IeaPoweAppsconn);
                int nextBatchId = ((int)cmd.ExecuteScalar()) + 1;
                foreach (var equipment in json)
                {

                    DataRow row1 = dt.NewRow();
                    row1["MBatchID"] = nextBatchId;
                    row1["Equipment"] = equipment["Equipment"];
                    row1["SerialNo"] = equipment["SerialNo"];
                    row1["Description"] = equipment["Description"];
                    row1["LicenseNumber"] = equipment["LicenseNumber"];
                    row1["HourReading"] = equipment["HourReading"];
                    row1["OdoReading"] = equipment["OdoReading"];
                    row1["udJobAssignment"] = equipment["udJobAssignment"];
                    row1["udReferenceNumber"] = equipment["udReferenceNumber"];
                    row1["NewHr"] = equipment["NewHr"];
                    row1["NewOdo"] = equipment["NewOdo"];
                    row1["Job"] = equipment["Job"];
                    row1["CreatedBy"] = equipment["CreatedBy"];
                    row1["CreatedDateTime"] = equipment["CreatedDateTime"];
                    dt.Rows.Add(row1);
                    //nextJobId = nextJobId + 1;

                }




                bulkCopy.DestinationTableName = "dbo.MeterOTransaction";
                try
                {
                    bulkCopy.WriteToServer(dt);
                    IeaPoweAppsconn.Dispose();

                }
                catch (SqlException ex)
                {
                    IeaPoweAppsconn.Dispose();
                    return -1;

                }
                return 1;
            }




            

        }


       

        public int getSessionID(string userID)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                string query = "Insert into User_Activity Values('" + userID + "','" + DateTime.Now + "','','Active');SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, IeaPoweAppsconn);
                int status = Convert.ToInt32(cmd.ExecuteScalar());
                if (status > 0)
                {
                    IeaPoweAppsconn.Dispose();
                    return status;

                }
                else
                {
                    IeaPoweAppsconn.Dispose();
                    return -1;

                }
            }
               
            
        }

        public int updateSession(string sessionID,string message)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                string query = "Update User_Activity SET User_Status='" + message + "', log_out_time='" + DateTime.Now + "' where session_ID=" + sessionID + ";";
                SqlCommand cmd = new SqlCommand(query, IeaPoweAppsconn);
                int status = cmd.ExecuteNonQuery();
                if (status > 0)
                {
                    IeaPoweAppsconn.Dispose();
                    return status;

                }
                else
                {
                    IeaPoweAppsconn.Dispose();
                    return -1;

                }
            }
                


        }
        //check user access level from database
        public DataTable checkUserRole(string samaccountName)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                SqlDataAdapter adap = new SqlDataAdapter("Select * from vUserRoles where Username='" + samaccountName + "'", IeaPoweAppsconn);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                IeaPoweAppsconn.Dispose();
                return dt;
            }

               

        }
        


    }
}