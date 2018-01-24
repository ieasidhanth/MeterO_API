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




namespace ViewPointAPI
{
    //class to access Data from viewPoint database
    public class DSL
    {
        //connection object
        private SqlConnection sqlconn;
        private SqlConnection transitDBconn;
        public DSL()
        {
            //sqlconn = null;
            Connection conn = new Connection();
            sqlconn = conn.initiateConnection();
            transitDBconn = conn.InitiateTransitDBConncetion();


        }
       
       //gets all jobs in company 53  
        public DataTable getAllJobs()
        {
            //Gets all Job with equipments
            
            SqlDataAdapter adap = new SqlDataAdapter("Select Distinct([Job]), [Description] from [Viewpoint].dbo.JCJM where Job in (SELECT Distinct([Job])  FROM[Viewpoint].[dbo].[EMEM]   where EMCo = 53) and JCCo=53; ",sqlconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            
            return dt;
            


        }
        //gets equipments with job id
        public DataTable getEquipments(string JobID)
        {
            SqlDataAdapter adap = new SqlDataAdapter("SELECT [Equipment],[Description],[Manufacturer] from Viewpoint.dbo.EMEM where Job='"+JobID+"' and EMCo='53'", sqlconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);

            return dt;

        }
        //get all location locations in compnay 53
        public DataTable getAllLocations()
        {
            //Gets all Job with equipments

            SqlDataAdapter adap = new SqlDataAdapter("Select EMCo,EMLoc,[Description] from Viewpoint.dbo.EMLM where EMCo='53' and Active='Y' and EMLoc in ('00','1-CALL OFF','2-JOB YARD');", sqlconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);

            return dt;



        }
        // get all equipments from viewpoint of dept 15 and 19;ownership status as owner and company 53
        public DataTable getAllEquipments()
        {
            //SqlDataAdapter adap = new SqlDataAdapter("select VINNumber as SerialNo,a.Equipment As EquipmentID,a.[Description] As [Description],b.[Description] as JobDescription,a.Job as JobID from Viewpoint.dbo.EMEM as a join Viewpoint.dbo.JCJM as b on a.[Job] =b.Job where a.Department in ('15', '19') and a.EMCo = '53' and b.JCCo = '53' and a.OwnershipStatus = 'O'", sqlconn);
            SqlDataAdapter adap1 = new SqlDataAdapter("select VINNumber as SerialNo,a.Equipment As EquipmentID,a.[Description] As [Description],a.Job as Job,a.Location as Location,AttachToEquip from Viewpoint.dbo.EMEM as a  where a.Department in ('15', '19') and a.EMCo = '53' and a.OwnershipStatus = 'O' and a.VINNumber is not null and a.Status='A'", sqlconn);
            SqlDataAdapter adap2 = new SqlDataAdapter("SELECT [EMCo],[Equipment],[Attachments],[Description]  FROM EMEMAttachToEquip where EMCo = 53 and Equipment is not null", sqlconn);
            DataTable attachmentdt = new DataTable();
            adap2.Fill(attachmentdt);
            DataTable dt = new DataTable();
            adap1.Fill(dt);
            Dictionary<string, string> jobcodes = fetchJobCodes();
            Dictionary<string, string> locationcodes = fetchLocationCodes();
            dt.Columns.Add("JobID", typeof(string));
            dt.Columns.Add("JobDescription", typeof(string));
            dt.Columns.Add("Locked", typeof(string));
            dt.Columns.Add("Attachment", typeof(string));
            dt.Columns.Add("AttachmentDesc", typeof(string));
            dt.Columns.Add("AttachmentList", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                if(Convert.ToString(dr["Location"])=="" && Convert.ToString(dr["Job"])!="")
                {
                    dr["JobID"] = dr["Job"];
                    string JobDesc = "";
                    jobcodes.TryGetValue(Convert.ToString(dr["Job"]), out JobDesc);
                    dr["JobDescription"] = JobDesc;

                }
                else if(Convert.ToString(dr["Job"]) == "" && Convert.ToString(dr["Location"]) != "")
                {
                    dr["JobID"] = dr["Location"];
                    string JobDesc = "";
                    locationcodes.TryGetValue(Convert.ToString(dr["Location"]), out JobDesc);
                    dr["JobDescription"] = JobDesc;

                }
                else
                {
                    dr["JobID"] = "";
                    dr["JobDescription"] = "";

                }
                dr["Locked"] = "false";
                //if(Convert.ToString(dr["AttachToEquip"])!="")
                //{
                //    dr["Attachment"] = "true";
                //    var tempAttachDesc = "";
                //    DataRow[] filteredRows = dt.Select("EquipmentID='" + dr["AttachToEquip"] + "'");
                //    foreach (DataRow updaterow in filteredRows)
                //    {
                //        //dt.Rows.Remove(deleterow);
                //        updaterow["AttachToEquip"] = dr["EquipmentID"];
                //        updaterow["Attachment"] = "true";
                //        updaterow["AttachmentDesc"] = dr["Description"];
                //        tempAttachDesc = Convert.ToString(updaterow["Description"]);

                //    }
                //    dr["AttachmentDesc"] = tempAttachDesc;

                //}
                //else
                //{
                //    dr["Attachment"] = "false";

                //}
                string equipmentID = Convert.ToString(dr["EquipmentID"]);
                DataRow[] foundrows= null;

                foundrows = attachmentdt.Select("Equipment='" + equipmentID + "'");
                if (foundrows.Length>0)
                {
                    
                    string attchList = "";
                    string attchDescList = "";
                    foreach(DataRow row in foundrows)
                    {
                        attchList = attchList + row["Attachments"] + "#";
                        attchDescList = attchDescList + row["Description"] + "$";
                        dr["Attachment"] = "true";

                    }
                    
                    dr["AttachmentList"] = attchList.Substring(0, attchList.LastIndexOf('#'));
                    
                    dr["AttachToEquip"] = attchList.Substring(0, attchList.LastIndexOf('#'));
                    dr["AttachmentDesc"] = attchDescList.Substring(0, attchDescList.LastIndexOf('$'));
                }
                else
                {
                    dr["Attachment"] = "false";
                    dr["AttachmentList"] = "";
                    dr["AttachToEquip"] = Convert.ToString("");
                    dr["AttachmentDesc"] = Convert.ToString("");

                }

            }
            //Uncomment below line for lock functionality
            //dt = filterInProcessRows(dt);
            return dt;
        }




        // get all equipments from viewpoint of dept 15 and 19;ownership status as owner and company 53 and also if they are in a batch fetches the details
        public DataTable getAllEquipmentsWBatchDetails()
        {
            //SqlDataAdapter adap = new SqlDataAdapter("select VINNumber as SerialNo,a.Equipment As EquipmentID,a.[Description] As [Description],b.[Description] as JobDescription,a.Job as JobID from Viewpoint.dbo.EMEM as a join Viewpoint.dbo.JCJM as b on a.[Job] =b.Job where a.Department in ('15', '19') and a.EMCo = '53' and b.JCCo = '53' and a.OwnershipStatus = 'O'", sqlconn);
            SqlDataAdapter adap1 = new SqlDataAdapter("SELECT SerialNo,EquipmentID ,[Description],Job,Location,BatchId as CurrentBatch FROM EMEM_wBatch", sqlconn);
            DataTable dt = new DataTable();
            adap1.Fill(dt);
            Dictionary<string, string> jobcodes = fetchJobCodes();
            Dictionary<string, string> locationcodes = fetchLocationCodes();
            dt.Columns.Add("JobID", typeof(string));
            dt.Columns.Add("JobDescription", typeof(string));
            dt.Columns.Add("Locked", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToString(dr["Location"]) == "" && Convert.ToString(dr["Job"]) != "")
                {
                    dr["JobID"] = dr["Job"];
                    string JobDesc = "";
                    jobcodes.TryGetValue(Convert.ToString(dr["Job"]), out JobDesc);
                    dr["JobDescription"] = JobDesc;

                }
                else if (Convert.ToString(dr["Job"]) == "" && Convert.ToString(dr["Location"]) != "")
                {
                    dr["JobID"] = dr["Location"];
                    string JobDesc = "";
                    locationcodes.TryGetValue(Convert.ToString(dr["Location"]), out JobDesc);
                    dr["JobDescription"] = JobDesc;

                }
                else
                {
                    dr["JobID"] = "";
                    dr["JobDescription"] = "";

                }
                if(dr["CurrentBatch"]==DBNull.Value)
                {
                    dr["Locked"] = "false";

                }
                else
                {
                    dr["Locked"] = "true";

                }
                

            }

           // dt = filterInProcessRowsV2(dt);
            return dt;
        }

        public DataTable filterInProcessRowsV2(DataTable dt)
        {
            SqlDataAdapter adap = new SqlDataAdapter("select * from dbo.TransitTransactions where Locked='true'", transitDBconn);
            DataTable transactTable = new DataTable();
            adap.Fill(transactTable);
            Console.WriteLine(transactTable);
            foreach (DataRow row in transactTable.Rows)
            {
                string serialNo = Convert.ToString(row["SerialNO"]);
                string equipmentId = Convert.ToString(row["EquipmentID"]);
                string TransactLoc = Convert.ToString(row["TransferLocID"]);
                //DataRow[] filteredRows=dt.Select("EquipmentID='"+equipmentId+ "' AND JobID <> '"+TransactLoc+"'");
                DataRow[] filteredRows = dt.Select("EquipmentID='" + equipmentId + "'");

                // DataTable temp = dt.Select("EquipmentID='" + equipmentId + "' AND JobID <>'" + TransactLoc + "'").CopyToDataTable();
                // Console.WriteLine(temp);



                foreach (DataRow updaterow in filteredRows)
                {
                    //dt.Rows.Remove(deleterow);
                    if(updaterow["CurrentBatch"]== DBNull.Value)
                    {
                        updaterow["Locked"] = "false";
                        row["Status"] = "Completed";
                        row["Locked"] = "false";


                    }
                    else
                    { 
                        
                      updaterow["Locked"] = "true";
                        row["Status"] = "InProgress";
                        row["Locked"] = "true";
                    }


                }


            }
            UpdateTransit(transactTable);
            return dt;
        }


        public void UpdateTransit(DataTable Transact)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(transitDBconn);
            bulkCopy.DestinationTableName = "dbo.TransitTransactions";
            try
            {
                bulkCopy.WriteToServer(Transact);

            }
            catch (SqlException ex)
            {
                

            }


        }



        //to filter out rowsV1 lock checking in WebAPI

        public DataTable filterInProcessRows(DataTable dt)
        {
            SqlDataAdapter adap = new SqlDataAdapter("select * from dbo.TransitTransactions where Locked='true'", transitDBconn);
            DataTable transactTable = new DataTable();
            adap.Fill(transactTable);
            Console.WriteLine(transactTable);
            foreach(DataRow row in transactTable.Rows)
            {
                string serialNo = Convert.ToString(row["SerialNO"]);
                string equipmentId = Convert.ToString(row["EquipmentID"]);
                string TransactLoc = Convert.ToString(row["TransferLocID"]);
                //DataRow[] filteredRows=dt.Select("EquipmentID='"+equipmentId+ "' AND JobID <> '"+TransactLoc+"'");
                DataRow[] filteredRows = dt.Select("SerialNo='" + serialNo +"'");

                // DataTable temp = dt.Select("EquipmentID='" + equipmentId + "' AND JobID <>'" + TransactLoc + "'").CopyToDataTable();
                // Console.WriteLine(temp);



                foreach (DataRow updaterow in filteredRows)
                {
                    //dt.Rows.Remove(deleterow);
                    updaterow["Locked"] = "true";


                }
                
                
            }
            return dt;
        }
        public Dictionary<string, string> fetchLocationCodes()
        {
            SqlDataAdapter adap = new SqlDataAdapter("select EMLoc as LocCode, Description as LocDesc from EMLM where EMCo='53'and Active='Y'", sqlconn);
            DataTable dt = new DataTable();
            Dictionary<string, string> d = new Dictionary<string, string>();
            adap.Fill(dt);
            foreach(DataRow dr in dt.Rows)
            {
                string key = dr["LocCode"].ToString();
                string value = dr["LocDesc"].ToString();
                if(!d.ContainsKey(key))
                {
                    d.Add(key, value);
                }
               

            }
            return d;


        }

        public Dictionary<string, string> fetchJobCodes()
        {
            SqlDataAdapter adap = new SqlDataAdapter("select Job as JobCode,Description as JobDesc from JCJM where JCCo='53'", sqlconn);
            DataTable dt = new DataTable();
            Dictionary<string, string> d = new Dictionary<string, string>();
            adap.Fill(dt);
            foreach (DataRow dr in dt.Rows)
            {
                string key = dr["JobCode"].ToString();
                string value = dr["JobDesc"].ToString();
                if (!d.ContainsKey(key))
                {
                    d.Add(key, value);
                }


            }
            return d;

        }




        public string convertToCSV(JArray json)
        {
            
            XmlNode xml = JsonConvert.DeserializeXmlNode("{records:{record:" + json + "}}");
            
            XmlDocument xmldoc = new XmlDocument();
            //Create XmlDoc Object
            xmldoc.LoadXml(xml.InnerXml);
            //Create XML Steam 
            var xmlReader = new XmlNodeReader(xmldoc);
            DataSet dataSet = new DataSet();
            //Load Dataset with Xml
            dataSet.ReadXml(xmlReader);
            //return single table inside of dataset

            //string csv = ToCSV(dataSet.Tables[0],",");
            string csv = table_to_csv(dataSet.Tables[0]);
            return csv;

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
                    result.Append(row[i].ToString());
                    result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
                }
            }
            return result.ToString().TrimEnd(new char[] { '\r', '\n' });
            //return result.ToString();
        }


        public string table_to_csv(DataTable table)
        {
            string file = "";

            //foreach (DataColumn col in table.Columns)
            //    file = string.Concat(file, col.ColumnName, ",");

            //file = file.Remove(file.LastIndexOf(','), 1);
            //file = string.Concat(file, "\r\n");

            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                    file = string.Concat(file, item.ToString(), ",");

                file = file.Remove(file.LastIndexOf(','), 1);
                file = string.Concat(file, "\r\n");
            }

            return file;
        }
        public string UpdatevEMLocationHistory(JArray equipmentList)
        {
            string check_faulty_equipments=validateEquipments(equipmentList, Convert.ToString(equipmentList[0]["jobDate"]));
            if (check_faulty_equipments.Length == 0)
            {
                dynamic v = equipmentList;
                SqlDataAdapter adap = new SqlDataAdapter("Select * from vEMLocationHistory", sqlconn);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                SqlCommandBuilder builder = new SqlCommandBuilder(adap);

                // add rows to dataset

                builder.GetInsertCommand();
                foreach (var equipment in v)
                {
                    DataRow row = dt.NewRow();
                    try
                    {
                        row["EMCo"] = 53;
                        row["Equipment"] = equipment.Equipment;
                        row["Sequence"] = GetSequenceNo(Convert.ToString(equipment.Equipment));
                        row["DateIn"] = Convert.ToDateTime(equipment.jobDate);
                        row["TimeIn"] = Convert.ToDateTime(equipment.jobDate);
                        row["ToJCCo"] = 53;
                        if (equipment.ToJob == "")
                        {
                            row["ToLocation"] = equipment.ToLocation;
                            row["ToJob"] = null;

                        }
                        else if (equipment.ToLocation == "")
                        {
                            row["ToJob"] = equipment.ToJob;
                            row["ToLocation"] = null;

                        }
                        row["Memo"] = "";
                        row["EstDateOut"] = DBNull.Value;
                        row["DateTimeIn"] = Convert.ToDateTime(equipment.jobDate);
                        row["Notes"] = "";
                        row["UniqueAttchID"] = DBNull.Value;
                        row["CreatedBy"] = equipment.CreatedByUserID;
                        row["CreatedDate"] = DateTime.Now;
                        row["ModifiedBy"] = equipment.CreatedByUserID;
                        row["ModifiedDate"] = DBNull.Value;
                        dt.Rows.Add(row);


                    }
                    catch (Exception ex)
                    {

                    }





                }
                try
                {
                    adap.Update(dt);
                    return "1";

                }
                catch (SqlException ex)
                {
                   return "-1";
                   


                }
            }
            else
            {
                return check_faulty_equipments;
            }
            
            

        }



        //Updated after 
        public string UpdatevEMLocationHistoryV2(JArray equipmentList)
        {
            string check_faulty_equipments = validateEquipments(equipmentList, Convert.ToString(equipmentList[0]["jobDate"]));
            if (check_faulty_equipments.Length == 0)
            {
                dynamic v = equipmentList;
                bool error = false;             
                foreach (var equipment in v)
                {
                    
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = sqlconn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "Insert into vEMLocationHistory(EMCo,Equipment,Sequence,DateIn,TimeIn,ToJCCo,ToJob,ToLocation,Memo,EstDateOut,DateTimeIn,Notes,UniqueAttchID,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate) VALUES (@EMCo,@Equipment,@Sequence,@DateIn,@TimeIn,@ToJCCo,@ToJob,@ToLocation,@Memo,@EstDateOut,@DateTimeIn,@Notes,@UniqueAttchID,@CreatedBy,@CreatedDate,@ModifiedBy,@ModifiedDate)";
                        cmd.Parameters.AddWithValue("@EMCo", 53);
                        cmd.Parameters.AddWithValue("@Equipment", Convert.ToString(equipment.Equipment));
                        UInt32 SeqNo = GetSequenceNo(Convert.ToString(equipment.Equipment));
                        cmd.Parameters.AddWithValue("@Sequence", Convert.ToInt32(SeqNo));
                        cmd.Parameters.AddWithValue("@DateIn", Convert.ToDateTime(equipment.jobDate));
                        cmd.Parameters.AddWithValue("@TimeIn", Convert.ToDateTime(equipment.jobDate));
                        cmd.Parameters.AddWithValue("@ToJCCo", 53);
                            if (equipment.ToJob == "")
                            {
                                cmd.Parameters.AddWithValue("@ToLocation", Convert.ToString(equipment.ToLocation));
                                cmd.Parameters.AddWithValue("@ToJob", DBNull.Value);

                            }
                            else if (equipment.ToLocation == "")
                            {
                                cmd.Parameters.AddWithValue("@ToLocation", DBNull.Value);
                                cmd.Parameters.AddWithValue("@ToJob", Convert.ToString(equipment.ToJob));

                            }
                            cmd.Parameters.AddWithValue("@Memo", "");
                            cmd.Parameters.AddWithValue("@EstDateOut", DBNull.Value);
                            cmd.Parameters.AddWithValue("@DateTimeIn", Convert.ToDateTime(equipment.jobDate));
                            cmd.Parameters.AddWithValue("@Notes", "");
                            cmd.Parameters.AddWithValue("@UniqueAttchID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", Convert.ToString(equipment.CreatedByUserID));
                            DateTime Ctime = DateTime.Now;
                            cmd.Parameters.AddWithValue("@CreatedDate", Ctime);
                            cmd.Parameters.AddWithValue("@ModifiedBy", Convert.ToString(equipment.CreatedByUserID));
                            cmd.Parameters.AddWithValue("@ModifiedDate", DBNull.Value);
                            try
                            {
                                int recordsAffected = cmd.ExecuteNonQuery();
                                if (recordsAffected > 0)
                                {
                                    if (Convert.ToString(equipment.HasAttachment) == "true")

                                    {
                                       string alist = Convert.ToString(equipment.AttachmentList);
                                       string[] list = null;
                                       if(alist.Contains('#'))
                                       {
                                            
                                            list =alist.Split('#');
                                       }
                                       else
                                       {
                                            list = new string[1];
                                            list[0] = alist;
                                       }
                                       foreach (string attachment in list )
                                        {
                                            int recentSequenceNo = Convert.ToInt32(SeqNo);
                                            UInt32 LocationHistoryID = GetLocationHistoryID(Convert.ToString(equipment.Equipment), recentSequenceNo);
                                            int AttachmentInsertStatus = InsertAttachedEquipment(Convert.ToInt32(LocationHistoryID), Convert.ToInt32(recentSequenceNo), Convert.ToString(equipment.Equipment), attachment, Convert.ToString(equipment.CreatedByUserID), Ctime);
                                            if (!(AttachmentInsertStatus > 0))
                                            {
                                                error = true;
                                                break;
                                            }
                                            else
                                            {
                                                error = false;
                                                
                                            }
                                        }
                                    }
                                    

                                }
                                else
                                {
                                    error = true;
                                    break;
                                }
                           }
                            catch(SqlException ex)
                            {
                              return "-1";

                            }
                            catch(Exception ex)
                            {
                               return "-1";

                            }
                            
                        
                             
                                    
                   




                }
                if (error == true)
                    return "-1";
                else
                    return "1";
                
            }
            else
            {
                return check_faulty_equipments;
            }



        }

        public UInt32 GetLocationHistoryID(string equipmentID, int SequenceNo)
        {
            SqlCommand cmd = new SqlCommand("select LocationHistoryId from vEMLocationHistory where Equipment='" + equipmentID + "' and Sequence="+SequenceNo+";", sqlconn);
            try
            {
                UInt32 LocationHistoryID = Convert.ToUInt32(cmd.ExecuteScalar());
                return LocationHistoryID;

            }
            catch (SqlException ex)
            {
                return Convert.ToUInt32(-1);
            }


        }
        public int InsertAttachedEquipment(Int32 LocHistoryID, Int32 SeqNo, string equipmentID, string attachedEquip,string createdBy, DateTime createDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlconn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Insert into vEMLocationHistoryAttach(LocationHistoryId,EMCo,Equipment,Sequence,AttachedEquipment,AttachedSequence,Memo,OverrideDateTime,DateIn,TimeIn,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,UniqueAttchID) VALUES (@LocationHistoryId,@EMCo,@Equipment,@Sequence,@AttachedEquipment,@AttachedSequence,@Memo,@OverrideDateTime,@DateIn,@TimeIn,@CreatedBy,@CreatedDate,@ModifiedBy,@ModifiedDate,@UniqueAttchID)";
            cmd.Parameters.AddWithValue("@LocationHistoryId", LocHistoryID);
            cmd.Parameters.AddWithValue("@EMCo", 53);
            cmd.Parameters.AddWithValue("@Equipment", equipmentID);
            cmd.Parameters.AddWithValue("@Sequence", SeqNo);
            cmd.Parameters.AddWithValue("@AttachedEquipment", attachedEquip);
            cmd.Parameters.AddWithValue("@AttachedSequence", 1);
            cmd.Parameters.AddWithValue("@Memo", "");
            cmd.Parameters.AddWithValue("@OverrideDateTime", 'N');
            cmd.Parameters.AddWithValue("@DateIn", DBNull.Value);
            cmd.Parameters.AddWithValue("@TimeIn", DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
            cmd.Parameters.AddWithValue("@CreatedDate", createDate);
            cmd.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
            cmd.Parameters.AddWithValue("@ModifiedDate", DBNull.Value);
            cmd.Parameters.AddWithValue("@UniqueAttchID", DBNull.Value);
            try
            {
                int recordsAffected = cmd.ExecuteNonQuery();
                return recordsAffected;

            }
            catch (SqlException ex)
            {
                return -1;
            }

        }


        






        public string validateEquipments(JArray equipmentList, string DateIn)
        {
            string eList = "";
            dynamic v = equipmentList;
            foreach (var equipment in v)
            {
                eList = eList+"'" + equipment.Equipment + "',";
                




            }
            string temp=eList.Substring(0, eList.LastIndexOf(','));
            string query = "select t1.Equipment,t1.DateTimeIn,t2.[Description] From (select a.Equipment,a.DateTimeIn from  Viewpoint.dbo.vEMLocationHistory a INNER JOIN (select Equipment, MAX(Sequence) AS Seq from Viewpoint.dbo.vEMLocationHistory  where Equipment in (" + temp + ") group by Equipment) b ON a.Equipment = b.Equipment AND a.Sequence = b.Seq where DateTimeIn >= '"+ Convert.ToDateTime(DateIn) + "') t1 JOIN Viewpoint.dbo.EMEM t2 on t1.Equipment=t2.Equipment where t2.EMCo='53'";
            SqlDataAdapter adap = new SqlDataAdapter(query, sqlconn);
            DataTable dt = new DataTable();
            try
            {
                adap.Fill(dt);

            }
            catch(SqlException ex)
            {
                Console.WriteLine("Error");
                return "failed";

            }

            
            string faultyEquipList = "";
            foreach(DataRow dr in dt.Rows)
            {
                faultyEquipList=faultyEquipList+Convert.ToString(dr["Equipment"])+"#"+Convert.ToString(dr["Description"])+"#"+ Convert.ToString(dr["DateTimeIn"]) + "$";

            }
            if(faultyEquipList.Length>0)
            {
                return faultyEquipList.Substring(0, faultyEquipList.LastIndexOf('$'));

            }
            else
            {
                return faultyEquipList;
            }
            
                
                
           }



        //get each equipment next Sequence no;
        public UInt32 GetSequenceNo(string equipmentID)
        {
            SqlCommand cmd = new SqlCommand("select max(Sequence) from vEMLocationHistory where Equipment='"+equipmentID+"';", sqlconn);
            try
            {
                UInt32 nextSequence = Convert.ToUInt32(cmd.ExecuteScalar()) + 1;
                return nextSequence;

            }
            catch (SqlException ex)
            {
                return Convert.ToUInt32(-1);
            }
            

        }
        public int SchdeuleJob(JArray json)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(transitDBconn);
            DataTable dt = new DataTable();
            DataColumn ID = new DataColumn("ID", typeof(Int32));
            DataColumn JobID = new DataColumn("JobID",typeof(Int32));
            DataColumn SerialNo = new DataColumn("SerialNo", typeof(string));
            DataColumn EquipmentID = new DataColumn("EquipmentID", typeof(string));
            DataColumn EquipmentName = new DataColumn("EquipmentName", typeof(string));
            DataColumn TransferLocID = new DataColumn("TransferLocID", typeof(string));
            DataColumn TransferLocName = new DataColumn("TransferLocName", typeof(string));
            DataColumn jobDate = new DataColumn("jobDate", typeof(string));
            DataColumn CreatedBY = new DataColumn("CreatedBY", typeof(string));
            DataColumn CreatedTime = new DataColumn("CreatedTime", typeof(string));
            DataColumn Status = new DataColumn("Status", typeof(string));
            DataColumn Locked = new DataColumn("Locked", typeof(string));
            dt.Columns.Add(ID);
            dt.Columns.Add(JobID);
            dt.Columns.Add(SerialNo);
            dt.Columns.Add(EquipmentID);
            dt.Columns.Add(EquipmentName);
            dt.Columns.Add(TransferLocID);
            dt.Columns.Add(TransferLocName);
            dt.Columns.Add(jobDate);
            dt.Columns.Add(CreatedBY);
            dt.Columns.Add(CreatedTime);
            dt.Columns.Add(Status);
            dt.Columns.Add(Locked);
            SqlCommand cmd = new SqlCommand("select max(JobID) from dbo.TransitTransactions;", transitDBconn);
            int nextJobId = ((int)cmd.ExecuteScalar()) + 1;
            foreach (var equipment in json)
            {

                DataRow row1 = dt.NewRow();
                row1["JobID"] = nextJobId;
                row1["SerialNo"] = equipment["SerialNo"];
                row1["EquipmentID"] = equipment["EquipmentID"];
                row1["EquipmentName"] = equipment["EquipmentDescription"];
                row1["TransferLocID"] = equipment["TransferLocID"];
                row1["TransferLocName"] = equipment["TransferLocName"];
                row1["jobDate"] = equipment["jobDate"];
                row1["CreatedBY"] = equipment["CreatedBY"];
                row1["CreatedTime"] = equipment["CreatedTime"];
                row1["Status"] = "Processed";
                row1["Locked"] = "";
                dt.Rows.Add(row1);
                //nextJobId = nextJobId + 1;

            }
            

            

            bulkCopy.DestinationTableName = "dbo.TransitTransactions";
            try
            {
                bulkCopy.WriteToServer(dt);

            }
            catch (SqlException ex)
            {
                return -1;

            }
            return 1;
            
        }


        public DataTable getScheduledBatches()
        {
            SqlDataAdapter adap = new SqlDataAdapter("select distinct(JobID),createdTime,CreatedBy from [dbo].[TransitTransactions] where JobID<>1000 order by JobID desc", transitDBconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            return dt;
            
        }
        public DataTable getBatchDetails(string batchId)
        {
            SqlDataAdapter adap = new SqlDataAdapter("Select * from [dbo].[TransitTransactions] where JobID='"+batchId+"'", transitDBconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            
            return dt;

        }
        public string updateBatchStatus(string jobID, string message)
        {
            SqlDataAdapter adap = new SqlDataAdapter("Select * from [dbo].[TransitTransactions] where JobID='" + jobID + "'", transitDBconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            foreach(DataRow row in dt.Rows)
            {
                if(message=="Cancel")
                {
                    row["Status"] = "Cancelled";
                    row["Locked"] = "false";

                }
                else if(message=="Completed")
                {
                    row["Status"] = "Processed";
                    row["Locked"] = "false";
                }
            }
            SqlCommandBuilder builder = new SqlCommandBuilder(adap);
            adap.UpdateCommand = builder.GetUpdateCommand();
            try
            {
                adap.Update(dt);
                dt.AcceptChanges();
                return "Success";

            }
            catch(SqlException ex)
            {
                return "failure";

            }
            

        }
        //Fetch equipment transfer history
        public DataTable getEquipmentHistory(string equipmentID)
        {
            SqlDataAdapter adap = new SqlDataAdapter("select Top(10)* from EMLocationHistory where Equipment='" + equipmentID+"' and EMCo=53 order by Sequence desc", sqlconn);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            adap.Dispose();
            Dictionary<string, string> jobList = fetchJobCodes();
            Dictionary<string, string> locationList = fetchLocationCodes();
            dt.Columns.Add("JobDesc", typeof(string));
            dt.Columns.Add("LocationDesc", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToString(dr["ToLocation"]) == "" && Convert.ToString(dr["ToJob"]) != "")
                {
                    
                    string JobDesc = "";
                    jobList.TryGetValue(Convert.ToString(dr["ToJob"]), out JobDesc);
                    dr["JobDesc"] = JobDesc;
                    dr["LocationDesc"] = "";

                }
                else if (Convert.ToString(dr["ToJob"]) == "" && Convert.ToString(dr["ToLocation"]) != "")
                {
                    string LocDesc = "";
                    locationList.TryGetValue(Convert.ToString(dr["ToLocation"]), out LocDesc);
                    dr["LocationDesc"] = LocDesc;
                    dr["JobDesc"] = "";

                }
                else
                {
                    dr["JobDesc"] = "";
                    dr["LocationDesc"] = "";

                }


            }
            return dt;
        }

        public int getSessionID(string userID)
        {
            string query = "Insert into User_Activity Values('" + userID + "','" + DateTime.Now + "','','Active');SELECT SCOPE_IDENTITY();";
            SqlCommand cmd = new SqlCommand(query, transitDBconn);
            int status = Convert.ToInt32(cmd.ExecuteScalar());
            if(status>0)
            {
                return status;

            }
            else
            {
                return -1;

            }
            
        }

        public int updateSession(string sessionID,string message)
        {
            string query = "Update User_Activity SET User_Status='" + message + "', log_out_time='"+DateTime.Now+"' where session_ID=" + sessionID + ";";
            SqlCommand cmd = new SqlCommand(query, transitDBconn);
            int status = cmd.ExecuteNonQuery();
            if (status > 0)
            {
                return status;

            }
            else
            {
                return -1;

            }


        }
        


    }
}