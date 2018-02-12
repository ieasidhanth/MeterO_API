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
            DataTable dt_viewPoint;
            DataTable dt_MeterO;
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                VpointDBConn.Open();
                SqlDataAdapter adap = new SqlDataAdapter("SELECT [Equipment],[VINNumber] as SerialNo,[Description],[LicensePlateNo] as LicenseNumber,[HourReading],[OdoReading] ,[udJobSiteAssignment] as JobAssign ,[udReferenceNumber],udMeterFieldNotes as Notes from Viewpoint.dbo.EMEM where Job='" + JobID + "' and EMCo='53' and udMeterType='Metered'", VpointDBConn);
                dt_viewPoint = new DataTable();
                adap.Fill(dt_viewPoint);
                VpointDBConn.Dispose();
                dt_viewPoint.Columns.Add("NewHr", typeof(string));
                dt_viewPoint.Columns.Add("NewOdo", typeof(string));
                dt_viewPoint.Columns.Add("Job", typeof(string));
                dt_viewPoint.Columns.Add("CreatedBy", typeof(string));
                dt_viewPoint.Columns.Add("CreatedDate", typeof(string));
                dt_viewPoint.Columns.Add("Saved_MeterO", typeof(string));
                dt_viewPoint.Columns.Add("HasNotes", typeof(string));
                foreach (DataRow row in dt_viewPoint.Rows)
                {
                    row["NewHr"] = "";
                    row["NewOdo"] = "";
                    row["Job"] = "";
                    row["CreatedBy"] = "";
                    row["CreatedDate"] = "";
                    row["Saved_MeterO"] = "false";
                    if (row["Notes"] != DBNull.Value)
                    {
                        row["HasNotes"] = "true";

                    }
                    else
                    {
                        row["HasNotes"] = "false";

                    }

                }

            }
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                SqlDataAdapter adap = new SqlDataAdapter("SELECT [MBatchID],[Equipment],[SerialNo],[Description],[LicenseNumber],[HourReading] ,[OdoReading] ,[udJobSiteAssignment],[udReferenceNumber],[NewHr],[NewOdo],[Job] FROM MeterOTransaction where Job='" + JobID + "'", IeaPoweAppsconn);
                dt_MeterO = new DataTable();
                adap.Fill(dt_MeterO);
            }
            foreach (DataRow row in dt_viewPoint.Rows)
            {
                string equipment_Vpoint = Convert.ToString(row["Equipment"]);
                string serialno_Vpoint = Convert.ToString(row["SerialNo"]);
                DataRow[] filteredRow = dt_MeterO.Select("Equipment='" + equipment_Vpoint + "' and SerialNo='" + serialno_Vpoint + "'");
                if (filteredRow.Length > 0)
                {
                    row["NewHr"] = Convert.ToString(filteredRow[0]["NewHr"]);
                    row["NewOdo"] = Convert.ToString(filteredRow[0]["NewOdo"]);
                    row["Saved_MeterO"] = "true";
                }

            }
            return dt_viewPoint;


        }
        // gets list of equipments assigned to an employee 
        public DataTable getPersonalEquipments(string employeeID)
        {
            if(employeeID!=null)
            {
                DataTable dt_viewPoint;
                DataTable dt_MeterO;
                using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
                {
                    VpointDBConn.Open();
                    SqlDataAdapter adap = new SqlDataAdapter("SELECT [Equipment],[VINNumber] as SerialNo,[Description],[LicensePlateNo] as LicenseNumber,[HourReading],[OdoReading] ,[udJobSiteAssignment] as JobAssign ,[udReferenceNumber],udMeterFieldNotes as Notes from Viewpoint.dbo.EMEM where Operator='" + employeeID + "' and EMCo='53' and Department=20 and udMeterType='Metered'", VpointDBConn);
                    dt_viewPoint = new DataTable();
                    adap.Fill(dt_viewPoint);
                    VpointDBConn.Dispose();
                    dt_viewPoint.Columns.Add("NewHr", typeof(string));
                    dt_viewPoint.Columns.Add("NewOdo", typeof(string));
                    dt_viewPoint.Columns.Add("Job", typeof(string));
                    dt_viewPoint.Columns.Add("CreatedBy", typeof(string));
                    dt_viewPoint.Columns.Add("CreatedDate", typeof(string));
                    dt_viewPoint.Columns.Add("Saved_MeterO", typeof(string));
                    dt_viewPoint.Columns.Add("HasNotes", typeof(string));
                    foreach (DataRow row in dt_viewPoint.Rows)
                    {
                        row["NewHr"] = "";
                        row["NewOdo"] = "";
                        row["Job"] = "";
                        row["CreatedBy"] = "";
                        row["CreatedDate"] = "";
                        row["Saved_MeterO"] = "false";
                        if (row["Notes"] != DBNull.Value)
                        {
                            row["HasNotes"] = "true";

                        }
                        else
                        {
                            row["HasNotes"] = "false";

                        }

                    }

                }
                //using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
                //{
                //    IeaPoweAppsconn.Open();
                //    SqlDataAdapter adap = new SqlDataAdapter("SELECT [MBatchID],[Equipment],[SerialNo],[Description],[LicenseNumber],[HourReading] ,[OdoReading] ,[udJobSiteAssignment],[udReferenceNumber],[NewHr],[NewOdo],[Job] FROM MeterOTransaction where Job='" + JobID + "'", IeaPoweAppsconn);
                //    dt_MeterO = new DataTable();
                //    adap.Fill(dt_MeterO);
                //}
                //foreach (DataRow row in dt_viewPoint.Rows)
                //{
                //    string equipment_Vpoint = Convert.ToString(row["Equipment"]);
                //    string serialno_Vpoint = Convert.ToString(row["SerialNo"]);
                //    DataRow[] filteredRow = dt_MeterO.Select("Equipment='" + equipment_Vpoint + "' and SerialNo='" + serialno_Vpoint + "'");
                //    if (filteredRow.Length > 0)
                //    {
                //        row["NewHr"] = Convert.ToString(filteredRow[0]["NewHr"]);
                //        row["NewOdo"] = Convert.ToString(filteredRow[0]["NewOdo"]);
                //        row["Saved_MeterO"] = "true";
                //    }

                //}
                return dt_viewPoint;

            }
            else
            {
                return null;
            }
            


        }

        public DataTable reviewEquipmentEntry(string JobID)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("JobDesc", typeof(string));
            dt.Columns.Add("Notes", typeof(string));
            dt.Columns.Add("HasNotes", typeof(string));
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

            var rowsToDelete = new List<DataRow>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string equipment = Convert.ToString(dt.Rows[i]["Equipment"]);
                foreach (DataRow viewpointRow in dt_viewpoint.Rows)
                {
                    if (Convert.ToString(viewpointRow["Equipment"]) == equipment)
                    {
                        if (viewpointRow["udMeterFieldNotes"] != DBNull.Value)
                        {
                            dt.Rows[i]["Notes"] = Convert.ToString(viewpointRow["udMeterFieldNotes"]);
                            dt.Rows[i]["HasNotes"] = "true";
                        }
                        else
                        {
                            dt.Rows[i]["Notes"] = "";
                            dt.Rows[i]["HasNotes"] = "false";
                        }
                        double vpointHrReading = Convert.ToDouble(viewpointRow["HourReading"]);
                        double MeterOHrReading = Convert.ToDouble(dt.Rows[i]["NewHr"]);
                        double vpointOdoReading = Convert.ToDouble(viewpointRow["OdoReading"]);
                        double MeterOodoReading = Convert.ToDouble(dt.Rows[i]["NewOdo"]);
                        if (((vpointHrReading == MeterOHrReading) && (vpointOdoReading == MeterOodoReading)) || ((vpointHrReading > MeterOHrReading) && (vpointOdoReading > MeterOodoReading)))
                        {
                            if (deleteRow(Convert.ToInt32(dt.Rows[i]["ID"])) > 0)
                            {
                                //dt.Rows.Remove(drow);
                                //dt.Rows.RemoveAt(i);
                                rowsToDelete.Add(dt.Rows[i]);
                            }

                        }
                    }
                }

            }
            rowsToDelete.ForEach(x => dt.Rows.Remove(x));
            //foreach(DataRow drow in dt.Rows)
            //{
            //    string equipment = Convert.ToString(drow["Equipment"]);
            //    foreach(DataRow viewpointRow in dt_viewpoint.Rows)
            //    {
            //        if(Convert.ToString(viewpointRow["Equipment"])==equipment)
            //        {
            //            double vpointHrReading = Convert.ToDouble(viewpointRow["HourReading"]);
            //            double MeterOHrReading = Convert.ToDouble(drow["NewHr"]);
            //            double vpointOdoReading = Convert.ToDouble(viewpointRow["OdoReading"]);
            //            double MeterOodoReading = Convert.ToDouble(drow["NewOdo"]);
            //            if ((vpointHrReading==MeterOHrReading) && (vpointOdoReading == MeterOodoReading ))
            //            {
            //                if(deleteRow(Convert.ToInt32(drow["ID"]))>0)
            //                {
            //                    dt.Rows.Remove(drow);
            //                }

            //            }
            //        }
            //    }
            //}


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
                    row[3] = Convert.ToString(obj["License No"]);
                    row[4] = Convert.ToString(obj["Job_Assigned"]);
                    row[5] = Convert.ToString(obj["Last Recorderd Hours"]);
                    row[6] = Convert.ToString(obj["Last Recorded Odometer"]);

                    row[7] = Convert.ToString(obj["New Recorded Hours"]);
                    row[8] = Convert.ToString(obj["New Odometer Reading"]);

                    dt.Rows.Add(row);


                }

                string csv = ToCSV(dt, ",");
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
            foreach (JObject eq in json)
            {
                status = processEquipments(eq);
                if (status == false)
                {
                    return -1;

                }

            }
            if (status)
                return 1;
            else
                return -1;

        }
        public int saveTrip(JArray json)
        {
            JObject insertItem = (JObject)json[0];
            if (insertItem != null)
            {
                using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
                {

                    SqlCommand cmd = new SqlCommand("[dbo].[ud_spInsertMeteroDailyLogSS]", viewpointDBConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrCo", Convert.ToInt32(insertItem["PrCo"]));
                    cmd.Parameters.AddWithValue("@EmpID", Convert.ToInt32(insertItem["EmployeeID"]));
                    cmd.Parameters.AddWithValue("@Email", Convert.ToString(insertItem["EmailID"]));
                    cmd.Parameters.AddWithValue("@EquipmentID", Convert.ToString(insertItem["EquipmentID"]));
                    cmd.Parameters.AddWithValue("@EmCo", Convert.ToInt32(insertItem["EquipmentCompany"]));
                    cmd.Parameters.AddWithValue("@EquipDesc", Convert.ToString(insertItem["EquipmentDesc"]));
                    cmd.Parameters.AddWithValue("@TripDate", Convert.ToString(insertItem["TDate"]));
                    cmd.Parameters.AddWithValue("@TripDesc", Convert.ToString(insertItem["TDesc"]));
                    cmd.Parameters.AddWithValue("@TripFrom", Convert.ToString(insertItem["TFrom"]));
                    cmd.Parameters.AddWithValue("@TripTo", Convert.ToString(insertItem["TTo"]));
                    cmd.Parameters.AddWithValue("@TripCat", Convert.ToString(insertItem["TCat"]));
                    cmd.Parameters.AddWithValue("@TripMiles", Convert.ToString(insertItem["TMiles"]));
                    cmd.Parameters.AddWithValue("@SubmittedDate", Convert.ToString(insertItem["TSubmittedDate"]));
                    cmd.Parameters.AddWithValue("@Status", Convert.ToString(insertItem["TStatus"]));
                    cmd.Parameters.AddWithValue("@CreatedBy", Convert.ToString(insertItem["TCreatedBy"]));
                    cmd.Parameters.AddWithValue("@CreatedDate", Convert.ToString(insertItem["TCreatedDate"]));

                    try
                    {
                        cmd.Connection = viewpointDBConnection;
                        viewpointDBConnection.Open();
                        int insertedRows = Convert.ToInt32(cmd.ExecuteNonQuery());
                        if (insertedRows == 1)
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        };



                    }
                    catch (SqlException ex)
                    {
                        viewpointDBConnection.Dispose();
                        return -1; ;
                    }
                    finally
                    {
                        viewpointDBConnection.Dispose();

                    }
                }
            }


            else
            {
                return -1;
            }
        }


            
        
        //fetches pending trip logs from viewpoint
        public DataTable fetchPendingDailyogs(string employeeID, string company)
        {
            DataTable dt = new DataTable();
            if (employeeID!=null && company!=null)
            {
                using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
                {
                    viewpointDBConnection.Open();
                    string query = "select  *,CONVERT(datetime,[CreatedDate],101) as CDate from   [Viewpoint].[IEA\\ssharma].udMeterODailyLog where EmployeeID=" + Convert.ToInt32(employeeID) + " and Status='Saved' and PrCo=" + Convert.ToInt32(company)+ " order by CDate desc;";
                    SqlDataAdapter adap = new SqlDataAdapter(query, viewpointDBConnection);
                    adap.Fill(dt);
                    viewpointDBConnection.Close();



                }

            }
            
            return dt;
        }


        public int updateEquipmentInfoViewpoint(JArray json)
        {
            Boolean status = false;
            foreach (JObject eq in json)
            {
                status = updateReferenceNumber(eq);
                if (status == false)
                {
                    return -1;

                }

            }
            if (status)
                return 1;
            else
                return -1;


        }
        public int updateEquipmentNotesViewpoint(JArray json)
        {
            Boolean status = false;
            foreach (JObject eq in json)
            {
                status = updateFieldNotes(eq);
                if (status == false)
                {
                    return -1;

                }

            }
            if (status)
                return 1;
            else
                return -1;


        }

        public Boolean updateReferenceNumber(JObject equipment)
        {
            string equipmentID = Convert.ToString(equipment["Equipment"]);
            string referenceNo = Convert.ToString(equipment["udReferenceNumber"]);
            using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                viewpointDBConnection.Open();
                SqlCommand cmd = new SqlCommand("update EMEM set [udReferenceNumber]= @RefNo from EMEM where [EMCo]=53 and [Equipment]=@EquipmentID", viewpointDBConnection);
                cmd.Parameters.AddWithValue("@RefNo", referenceNo);
                cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                try
                {
                    if ((cmd.ExecuteNonQuery()) > 0)
                    {
                        viewpointDBConnection.Dispose();
                        return true;

                    }
                    else
                    {
                        viewpointDBConnection.Dispose();
                        return false;
                    }

                }
                catch (SqlException ex)
                {
                    viewpointDBConnection.Dispose();
                    return false;
                }

            }

        }
        //update personal log entry
        public Boolean updateLogEntry(JObject entry)
        {
            int TripID = Convert.ToInt32(entry["Trip_ID"]);
            string TripDate = Convert.ToString(entry["TripDate"]);
            string TripDesc = Convert.ToString(entry["Trip_Desc"]);
            string TripFrom = Convert.ToString(entry["Trip_From"]);
            string TripTo = Convert.ToString(entry["Trip_To"]);
            string TripCategory = Convert.ToString(entry["Trip_Category"]);
            string Miles = Convert.ToString(entry["Miles"]);
            string ModifiedDate = Convert.ToString(DateTime.Now);
            using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                viewpointDBConnection.Open();
                SqlCommand cmd = new SqlCommand("update [Viewpoint].[IEA\\ssharma].udMeterODailyLog set [TripDate]= @TripDate,[Trip_Desc]=@TripDesc,[Trip_From]=@TripFrom,[Trip_To]=@TripTo,[Trip_Category]=@TripCat,[Miles]=@Miles,[ModifiedDate]=@ModifiedDate from EMEM where [Trip_ID]=@Trip_ID", viewpointDBConnection);
                cmd.Parameters.AddWithValue("@TripDate", TripDate);
                cmd.Parameters.AddWithValue("@TripDesc", TripDesc);
                cmd.Parameters.AddWithValue("@TripFrom", TripFrom);
                cmd.Parameters.AddWithValue("@TripTo", TripTo);
                cmd.Parameters.AddWithValue("@TripCat", TripCategory);
                cmd.Parameters.AddWithValue("@Miles", Miles);
                cmd.Parameters.AddWithValue("@ModifiedDate", ModifiedDate);
                cmd.Parameters.AddWithValue("@Trip_ID", TripID);
                
                try
                {
                    if ((cmd.ExecuteNonQuery()) > 0)
                    {
                        viewpointDBConnection.Dispose();
                        return true;

                    }
                    else
                    {
                        viewpointDBConnection.Dispose();
                        return false;
                    }

                }
                catch (SqlException ex)
                {
                    viewpointDBConnection.Dispose();
                    return false;
                }

            }

        }
        
        //updates the row with status deleted and modified date
        public Boolean DeleteLogEntry(JObject entry)
        {
            int TripID = Convert.ToInt32(entry["Trip_ID"]);
           
            string Status = "Deleted";
            string ModifiedDate = Convert.ToString(DateTime.Now);
            using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                viewpointDBConnection.Open();
                SqlCommand cmd = new SqlCommand("update [Viewpoint].[IEA\\ssharma].udMeterODailyLog set [Status]=@Status,[ModifiedDate]=@ModifiedDate from EMEM where [Trip_ID]=@Trip_ID", viewpointDBConnection);
                cmd.Parameters.AddWithValue("@Status", Status);
                cmd.Parameters.AddWithValue("@ModifiedDate", ModifiedDate);
                cmd.Parameters.AddWithValue("@Trip_ID", TripID);

                try
                {
                    if ((cmd.ExecuteNonQuery()) > 0)
                    {
                        viewpointDBConnection.Dispose();
                        return true;

                    }
                    else
                    {
                        viewpointDBConnection.Dispose();
                        return false;
                    }

                }
                catch (SqlException ex)
                {
                    viewpointDBConnection.Dispose();
                    return false;
                }

            }

        }
        public Boolean updateFieldNotes(JObject equipment)
        {
            string equipmentID = Convert.ToString(equipment["Equipment"]);
            string notes = Convert.ToString(equipment["Notes"]);
            using (SqlConnection viewpointDBConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                viewpointDBConnection.Open();
                SqlCommand cmd = new SqlCommand("update EMEM set [udMeterFieldNotes]= @Notes from EMEM where [EMCo]=53 and [Equipment]=@EquipmentID", viewpointDBConnection);
                if (notes.Length == 0)
                {
                    cmd.Parameters.AddWithValue("@Notes", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Notes", notes);

                }

                cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                try
                {
                    if ((cmd.ExecuteNonQuery()) > 0)
                    {
                        viewpointDBConnection.Dispose();
                        return true;

                    }
                    else
                    {
                        viewpointDBConnection.Dispose();
                        return false;
                    }

                }
                catch (SqlException ex)
                {
                    viewpointDBConnection.Dispose();
                    return false;
                }

            }

        }


        public Boolean processEquipments(JObject equipment)
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

        public int updateSession(string sessionID, string message)
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
        public int deleteRowTransaction(string equipment)
        {
            using (SqlConnection IeaPoweAppsconn = new SqlConnection(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"].ToString()))
            {
                IeaPoweAppsconn.Open();
                string query = "Delete from MeterOTransaction where Equipment='" + equipment + "'";
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
        public DataTable deleteallNotes(string JobID)
        {
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                VpointDBConn.Open();

                DataTable returntable;
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand = VpointDBConn.CreateCommand();
                sqlCommand.CommandText = "Update Viewpoint.dbo.EMEM SET udMeterFieldNotes= NULL where Job='" + JobID + "' and EMCo='53' and udMeterType='Metered'";



                try
                {
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status > 0)
                    {
                        returntable = getEquipments(JobID);
                        return returntable;

                    }
                    else
                    {
                        return null;
                    }

                }
                catch (SqlException sqlex)
                {
                    return null;

                }
                finally
                {
                    VpointDBConn.Dispose();

                }




            }
        }
        public DataTable remainingEquipments_notSubmitted(dynamic submittedEquipments)
        {
            using (SqlConnection VpointDBConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"].ToString()))
            {
                var equip = submittedEquipments[0];
                DataTable viewpointTable = new DataTable();
                string jobId = equip.Job;
                SqlDataAdapter adap = new SqlDataAdapter("SELECT [Equipment],[VINNumber] as SerialNo,[Description],[LicensePlateNo] as LicenseNumber,[HourReading],[OdoReading] ,[udJobSiteAssignment] as JobAssign ,[udReferenceNumber],udMeterFieldNotes as Notes from Viewpoint.dbo.EMEM where Job='" + jobId + "' and EMCo='53' and udMeterType='Metered'", VpointDBConn);
                adap.Fill(viewpointTable);
                VpointDBConn.Dispose();
                var rowstodelete = new List<DataRow>();
                foreach (var equipment in submittedEquipments)
                {
                    foreach (DataRow row in viewpointTable.Rows)
                    {
                        if (equipment.Equipment == Convert.ToString(row["Equipment"]))
                        {
                            rowstodelete.Add(row);

                        }

                    }

                }
                rowstodelete.ForEach(x => viewpointTable.Rows.Remove(x));
                return viewpointTable;

            }



        }
    }
}