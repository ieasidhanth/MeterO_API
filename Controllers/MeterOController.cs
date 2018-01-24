using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Data;
using Newtonsoft.Json.Linq;
using ViewPointAPI.Models;
using System.IO;
using System.DirectoryServices.AccountManagement;
using System.Web.Mail;
using System.Net.Mail;
using System.Text;
using System.Web.Configuration;

namespace ViewPointAPI.Controllers
{
    public class MeterOController : ApiController
    {
        private DSL_MeterO obj;
        private List<Dictionary<string, object>> jobList;
        private List<Dictionary<string, object>> locationList;
        //gets all jobs
        [HttpGet]
        public List<Dictionary<string, object>> getJobs()
        {
            obj = new DSL_MeterO();
            DataTable dt;
            dt = obj.getAllJobs();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            jobList = rows;
            return rows;

        }
        //gets all equipments associated with a job
        [HttpPost]
        public List<Dictionary<string, object>> getEquipments(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            //JArray v = JArray.Parse(postedString);
            dynamic d = JObject.Parse(Convert.ToString(postedString));
            string jobId = Convert.ToString(d.jobId);
            obj = new DSL_MeterO();
            DataTable dt;
            dt = obj.getEquipments(jobId);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }

            return rows;


        }
        [HttpPost]
        public List<Dictionary<string, object>> ClearAllNotes(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            //JArray v = JArray.Parse(postedString);
            dynamic d = JObject.Parse(Convert.ToString(postedString));
            string jobId = Convert.ToString(d.jobId);
            obj = new DSL_MeterO();
            DataTable dt;
            dt = obj.deleteallNotes(jobId);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }

            return rows;


        }
        [HttpPost]
        public List<Dictionary<string, object>> reviewSubmit(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            //JArray v = JArray.Parse(postedString);
            dynamic d = JObject.Parse(Convert.ToString(postedString));
            string jobId = Convert.ToString(d.jobId);
            obj = new DSL_MeterO();
            DataTable dt;
            dt = obj.reviewEquipmentEntry(jobId);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }

            return rows;


        }
        //this method checks if user did not submit all readings for all equipments on jobsite, if not shows them the list of reamining equipments and previous recorded meter readings
        [HttpPost]
        public List<Dictionary<string, object>> reviewremainingEquipments(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            DSL_MeterO obj = new DSL_MeterO();
            DataTable remainingEquipments=obj.remainingEquipments_notSubmitted(v);
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            if (remainingEquipments!=null)
            {
                
                foreach (DataRow dr in remainingEquipments.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in remainingEquipments.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }

            }
            return rows;

        }
        //Generate template
        [HttpPost]
        public string SubmitReadings(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            string jobName = "";
            string jobDesc="";
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            foreach (var equipment in v)
            {
                JObject item = new JObject();
                JObject itemPushJob = new JObject();
                //for converting it to CSV
                
                item.Add(new JProperty("Equipment", Convert.ToString(equipment.Equipment)));
                item.Add(new JProperty("Blank","") );
                item.Add(new JProperty("New Recorded Hours", equipment.NewHr));
                string indate = DateTime.Now.ToShortDateString();
               
                item.Add(new JProperty("DateIn",indate));
                item.Add(new JProperty("Blank1", ""));
                item.Add(new JProperty("New Recorded OdoMeter", equipment.NewOdo));
                item.Add(new JProperty("Job", equipment.Job));
                jobName = equipment.Job;
                jobDesc = equipment.JobDesc;
                toCsv.Add(item);
               



            }
            Console.WriteLine(toCsv);
            
            obj = new DSL_MeterO();
            DataTable remainingequipments = new DataTable();
            remainingequipments = obj.remainingEquipments_notSubmitted(v);
            if(remainingequipments.Rows.Count>0)
            {
                foreach(DataRow row in remainingequipments.Rows)
                {
                    JObject item = new JObject();
                    item.Add(new JProperty("Equipment", Convert.ToString(row["Equipment"])));
                    item.Add(new JProperty("Blank", ""));
                    item.Add(new JProperty("New Recorded Hours", Convert.ToString(row["HourReading"])));
                    string indate = DateTime.Now.ToShortDateString();

                    item.Add(new JProperty("DateIn", indate));
                    item.Add(new JProperty("Blank1", ""));
                    item.Add(new JProperty("New Recorded OdoMeter", Convert.ToString(row["OdoReading"])));
                    item.Add(new JProperty("Job", jobName));
                    toCsv.Add(item);


                }
            }
            try
            {
                string csv = obj.convertToCSV(toCsv);

                if(csv!="failed")
                {
                    string date = DateTime.Now.ToString().Replace(':', '_').Replace('/', '-');
                    string filename = jobName + "-MeterO-" + date + ".csv";
                    string exe_path = @"C:\\Data\\";

                    MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(csv));
                    //var sw = new StreamWriter(memoryStream);
                    //sw.WriteLine(csv);

                    //File.WriteAllText(exe_path + filename + ".csv",memoryStream.GetBuffer());



                    if(sendEmail(memoryStream, filename, jobDesc)== "success")
                    {
                        return "Success";

                    }
                    else
                    {
                        return "Fail";
                    }
                    

                }
                else
                {
                    return "Fail";
                }
                

            }
            catch (Exception ex)
            {
                return ("Fail");

            }



        }



        public string sendEmail(MemoryStream memoryStream,string filename, string jobdesc)
        {
            try
            {
                String userName = Convert.ToString(WebConfigurationManager.AppSettings["FromEmail"]);
                String password = Convert.ToString(WebConfigurationManager.AppSettings["FromEmailPassword"]);
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(new MailAddress(Convert.ToString(WebConfigurationManager.AppSettings["ToEmail"])));
                msg.From = new MailAddress(userName);
                msg.Subject = "Meter Reading submitted for " + jobdesc;
                msg.Body = "This is an auto generated email. do not reply";
                msg.IsBodyHtml = true;
                System.Net.Mail.Attachment attch = new System.Net.Mail.Attachment(memoryStream, filename);
                msg.Attachments.Add(attch);
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.office365.com";
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
                return "success";

            }
            catch(Exception ex)
            {
                return "Email_Fail";

            }
            
        }



        public string sendEmailQuery(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            var email = v[0];
            string jobDesc = email.job;
            string ccEmail = email.fromemail;
            string subject = email.sub;
            string body = email.body;
           // string toemail = "";

            try
            {
                String userName = Convert.ToString(WebConfigurationManager.AppSettings["FromEmail"]);
                String password = Convert.ToString(WebConfigurationManager.AppSettings["FromEmailPassword"]);
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(new MailAddress(Convert.ToString(WebConfigurationManager.AppSettings["ToEmail"])));
                msg.CC.Add(new MailAddress(ccEmail));
                msg.From = new MailAddress(Convert.ToString(WebConfigurationManager.AppSettings["FromEmail"]));
                
                //msg.CCTo = new MailAddress(fromEmail);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
               // System.Net.Mail.Attachment attch = new System.Net.Mail.Attachment(memoryStream, filename);
                //msg.Attachments.Add(attch);
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.office365.com";
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
                return "success";

            }
            catch (Exception ex)
            {
                return "Email_Fail";

            }

        }


        //Transfer Batch - This is  Schedule Batch V2 for Viewpoint 6.13 where there is no Batch concept for Equipment Transfers.
        [HttpPost]
        public string AddTransaction(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            foreach (var equipment in v)
            {
                JObject item = new JObject();
                JObject itemPushJob = new JObject();

                

                // for pushing into Transit Database
                itemPushJob.Add(new JProperty("Equipment", Convert.ToString(equipment.Equipment)));
                itemPushJob.Add(new JProperty("SerialNo", equipment.SerialNo));
                itemPushJob.Add(new JProperty("Description", Convert.ToString(equipment.Description)));
                itemPushJob.Add(new JProperty("LicenseNumber", Convert.ToString(equipment.LicenseNumer)));
                itemPushJob.Add(new JProperty("HourReading", Convert.ToString(equipment.HourReading)));
                itemPushJob.Add(new JProperty("OdoReading", Convert.ToString(equipment.OdoReading)));
                itemPushJob.Add(new JProperty("udJobSiteAssignment", Convert.ToString(equipment.udJobSiteAssignment)));
                itemPushJob.Add(new JProperty("udReferenceNumber", Convert.ToString(equipment.udReferenceNumber)));
                itemPushJob.Add(new JProperty("NewHr", Convert.ToString(equipment.NewHr)));
                itemPushJob.Add(new JProperty("NewOdo", Convert.ToString(equipment.NewOdo)));
                itemPushJob.Add(new JProperty("Job", Convert.ToString(equipment.Job)));
                itemPushJob.Add(new JProperty("CreatedBy", Convert.ToString(equipment.CreatedBy)));
                itemPushJob.Add(new JProperty("CreatedDateTime", DateTime.Now.ToString()));
                
                pushJob.Add(itemPushJob);




            }
            obj = new DSL_MeterO();
            try
            {
                
               
                    int MeterODBUpdate = obj.saveTransaction(pushJob);
                    if (MeterODBUpdate == 1)
                        return "Success";
                    else
                        return "failed";


                
                



            }
            catch (Exception ex)
            {
                return ("failed");

            }
        }

        //Test API
        [HttpGet]
        public string testApi()
        {
            return "API Working Fine!";
        }


        [HttpPost]
        public string updateEquipment(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            foreach (var equipment in v)
            {
                JObject item = new JObject();
                JObject itemPushJob = new JObject();



                // for pushing into Transit Database
                itemPushJob.Add(new JProperty("Equipment", Convert.ToString(equipment.Equipment)));
                itemPushJob.Add(new JProperty("SerialNo", equipment.SerialNo));
                itemPushJob.Add(new JProperty("Description", Convert.ToString(equipment.Description)));
                itemPushJob.Add(new JProperty("LicenseNumber", Convert.ToString(equipment.LicenseNumer)));
                
                itemPushJob.Add(new JProperty("udReferenceNumber", Convert.ToString(equipment.udReferenceNumber)));
               
                itemPushJob.Add(new JProperty("CreatedBy", Convert.ToString(equipment.CreatedBy)));
                itemPushJob.Add(new JProperty("CreatedDateTime", DateTime.Now.ToString()));

                pushJob.Add(itemPushJob);




            }
            obj = new DSL_MeterO();
            try
            {


                int MeterODBUpdate = obj.updateEquipmentInfoViewpoint(pushJob);
                if (MeterODBUpdate == 1)
                    return "Success";
                else
                    return "failed";







            }
            catch (Exception ex)
            {
                return ("failed");

            }
        }





        //validate User
        [HttpPost]
        public List<Dictionary<string, object>> validateUser(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic d = JObject.Parse(Convert.ToString(postedString));
            //JArray v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            string username = Convert.ToString(d.username);
            string pwd = Convert.ToString(d.password);
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            row = new Dictionary<string, object>();
            obj = new DSL_MeterO();
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "IEA"))
            {
                bool isValid = pc.ValidateCredentials(username, pwd, ContextOptions.Negotiate);
                if (isValid)
                {
                    using (UserPrincipal user = UserPrincipal.FindByIdentity(pc, username))
                    {
                        if (user != null)
                        {

                            //string employeeID = user.EmployeeId;
                            //check access rights
                            DataTable userRoles;
                            userRoles=obj.checkUserRole(user.SamAccountName);
                            string RoleInfo = null;
                            foreach(DataRow userrolerow in userRoles.Rows)
                            {
                                RoleInfo = RoleInfo + "App_ID:" + userrolerow["App_ID"] + ";" + "App_Name:" + userrolerow["App_Name"] + ";" + "Role_ID:" + userrolerow["Role_ID"] + ";" + "Role_Name:" + userrolerow["Role_Name"] + ";" + "UserAccessID:" + userrolerow["ID"] + "#";

                            }
                            
                            if(userRoles.Rows.Count>0)
                            {
                                
                                int get_session_id = obj.getSessionID(user.SamAccountName);
                                if (get_session_id > 0)
                                {
                                    string Name = user.DisplayName;
                                    row.Add("UserName", username);
                                    row.Add("Name", Name);
                                    row.Add("UserValidated", "true");
                                    row.Add("UserID", user.SamAccountName);
                                    row.Add("SessionID", get_session_id);
                                    row.Add("Access", "true");
                                    row.Add("RoleInfo", RoleInfo.Substring(0, RoleInfo.LastIndexOf('#'))); 
                                    rows.Add(row);

                                }
                                else
                                {
                                    string Name = user.DisplayName;
                                    row.Add("UserName", username);
                                    row.Add("Name", Name);
                                    row.Add("UserValidated", "true");
                                    row.Add("UserID", user.SamAccountName);
                                    row.Add("SessionID", -1);
                                    row.Add("Access", "true");
                                    row.Add("RoleInfo", "");
                                    rows.Add(row);

                                }
                                return rows;

                            }
                            else
                            {
                                string Name = user.DisplayName;
                                row.Add("UserName", username);
                                row.Add("Name", Name);
                                row.Add("UserValidated", "true");
                                row.Add("UserID", user.SamAccountName);
                                row.Add("SessionID", -1);
                                row.Add("Access", "false");
                                row.Add("RoleInfo", "");
                                rows.Add(row);
                                return rows;

                            }
                            

                        }
                        else
                        {

                            row.Add("UserName", username);
                            row.Add("Name", "Not Found");
                            row.Add("UserValidated", "true");
                            row.Add("SessionID", -1);
                            row.Add("Access", "false");
                            rows.Add(row);
                            //string employeeID = user.EmployeeId;
                            return rows;

                        }
                    }
                }
                else
                {
                    row.Add("UserName", username);
                    row.Add("Name", "Not Found");
                    row.Add("UserValidated", "false");
                    rows.Add(row);
                    //string employeeID = user.EmployeeId;
                    return rows;


                }


            }
        }
        //Generate template
        [HttpPost]
        public string EmailEquipments(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            string jobName = "";
            string jobDesc = "";
            string toemail = "";
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            foreach (var equipment in v)
            {
                JObject item = new JObject();
                JObject itemPushJob = new JObject();
                //for converting it to CSV

                item.Add(new JProperty("Equipment", Convert.ToString(equipment.Equipment)));
                item.Add(new JProperty("SerialNo", Convert.ToString(equipment.SerialNo)));
                item.Add(new JProperty("Description", Convert.ToString(equipment.Description)));
                item.Add(new JProperty("License No", Convert.ToString(equipment.LicenseNumber)));
                item.Add(new JProperty("Job Assigned", Convert.ToString(equipment.JobAssign)));
                item.Add(new JProperty("Last Recorderd Hours", Convert.ToString(equipment.HourReading)));
                item.Add(new JProperty("Last Recorded Odometer", Convert.ToString(equipment.OdoReading)));
                item.Add(new JProperty("New Hour Reading", ""));
                item.Add(new JProperty("New Odometer Reading", ""));
                item.Add(new JProperty("Job", equipment.JobDesc));
                jobName = equipment.Job;
                jobDesc = equipment.JobDesc;
                toemail = equipment.EmailTo;
                toCsv.Add(item);




            }
            Console.WriteLine(toCsv);

            obj = new DSL_MeterO();
            try
            {
                string csv = obj.convertToCSVList(toCsv);

                if (csv != "failed")
                {
                    string date = DateTime.Now.ToString().Replace(':', '_').Replace('/', '-');
                    string filename ="Export from " +jobName + "-MeterO-" + date + ".csv";
                    string exe_path = @"C:\\Data\\";


                    MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(csv));
                    //var sw = new StreamWriter(memoryStream);
                    //sw.WriteLine(csv);

                    //File.WriteAllText(exe_path + filename + ".csv",memoryStream.GetBuffer());



                    if (sendEmailTo(memoryStream, filename, jobDesc, toemail) == "success")
                    {
                        return "Success";

                    }
                    else
                    {
                        return "Fail";
                    }


                }
                else
                {
                    return "Fail";
                }


            }
            catch (Exception ex)
            {
                return ("Fail");

            }




        }
        [HttpPost]
        public string disconnectSession(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic d = JObject.Parse(Convert.ToString(postedString));
            //JArray v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            string sessionId = Convert.ToString(d.SessionId);
            string message = Convert.ToString(d.message);
            obj = new DSL_MeterO();
            int logoutStatus = obj.updateSession(sessionId, message);
            if (logoutStatus > 0)
                return "success";
            else
                return "failed";

        }


        public string sendEmailTo(MemoryStream memoryStream, string filename, string jobdesc, string To)
        {
            try
            {
                
                String userName = Convert.ToString(WebConfigurationManager.AppSettings["FromEmail"]);
                String password = Convert.ToString(WebConfigurationManager.AppSettings["FromEmailPassword"]);
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(new MailAddress(Convert.ToString(To)));
                msg.From = new MailAddress(userName);
                msg.Subject = "Equipment Export from " + jobdesc;
                msg.Body = "This is an auto generated email. do not reply";
                msg.IsBodyHtml = true;
                System.Net.Mail.Attachment attch = new System.Net.Mail.Attachment(memoryStream, filename);
                msg.Attachments.Add(attch);
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.office365.com";
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
                return "success";

            }
            catch (Exception ex)
            {
                return "Email_Fail";

            }

        }

        [HttpPost]
        public string deleteTransaction(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            obj = new DSL_MeterO();
            int status = -1;
            foreach (var equipment in v)
            {


                try
                {
                    int deleteStatus = obj.deleteRowTransaction(Convert.ToString(equipment.Equipment));
                    if (deleteStatus == 1)
                    {
                        status = 1;
                    }
                    else
                    {
                        status = -1;
                        break;
                    }

                }
                catch (Exception ex)
                {
                    status = -1;
                }





            }


            if (status == 1)
                return "Success";
            else
                return "Fail";

        }




        [HttpPost]
        public string updateNotesViewpoint(HttpRequestMessage req)
        {
            var postedString = req.Content.ReadAsStringAsync().Result;
            dynamic v = JArray.Parse(postedString);
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //dynamic d = JObject.Parse(Convert.ToString(postedString));
            //string month = Convert.ToDateTime(d.month);
            JArray toCsv = new JArray();
            JArray pushJob = new JArray();
            foreach (var equipment in v)
            {
                JObject item = new JObject();
                JObject itemPushJob = new JObject();



                // for pushing into Transit Database
                itemPushJob.Add(new JProperty("Equipment", Convert.ToString(equipment.Equipment)));
                itemPushJob.Add(new JProperty("SerialNo", equipment.SerialNo));
                itemPushJob.Add(new JProperty("Description", Convert.ToString(equipment.Description)));
                itemPushJob.Add(new JProperty("LicenseNumber", Convert.ToString(equipment.LicenseNumer)));
                itemPushJob.Add(new JProperty("Notes", Convert.ToString(equipment.Notes)));
                itemPushJob.Add(new JProperty("udReferenceNumber", Convert.ToString(equipment.udReferenceNumber)));

                itemPushJob.Add(new JProperty("CreatedBy", Convert.ToString(equipment.CreatedBy)));
                itemPushJob.Add(new JProperty("CreatedDateTime", DateTime.Now.ToString()));

                pushJob.Add(itemPushJob);




            }
            obj = new DSL_MeterO();
            try
            {


                int MeterODBUpdate = obj.updateEquipmentNotesViewpoint(pushJob);
                if (MeterODBUpdate == 1)
                    return "Success";
                else
                    return "failed";







            }
            catch (Exception ex)
            {
                return ("failed");

            }
        }









    }
}
