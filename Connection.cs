using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ViewPointAPI
{
    public class Connection
    {
        private SqlConnection sqlconn;
        private SqlConnection transitDBconn;
        private SqlConnection IEA_powerApps;

        //// Initiates a connection to ViewpointDB
        public SqlConnection initiateConnection()
        {
            try
            {
                sqlconn = new SqlConnection(Convert.ToString(WebConfigurationManager.ConnectionStrings["viewPointDBConnection"]));
                sqlconn.Open();
                return sqlconn;
            }
            catch (SqlException e)
            {
                return null;

            }
        }
        public void dispose(SqlConnection conn)
        {
            conn.Dispose();

        }
        // Initiates a connection to TransitDB
        public SqlConnection InitiateTransitDBConncetion()
        {
            try
            {
                transitDBconn = new SqlConnection(Convert.ToString(WebConfigurationManager.ConnectionStrings["TransitDBConnection"]));
                transitDBconn.Open();
                return transitDBconn;
            }
            catch (SqlException e)
            {
                return null;

            }

        }

        // Initiates a connection to IEA-Power Apps
        public SqlConnection InitiatePowerAppsDBConnection()
        {
            try
            {
                IEA_powerApps = new SqlConnection(Convert.ToString(WebConfigurationManager.ConnectionStrings["IEAPowerApps-DBConnection"]));
                IEA_powerApps.Open();
                return IEA_powerApps;
            }
            catch (SqlException e)
            {
                return null;

            }

        }



    }
}
