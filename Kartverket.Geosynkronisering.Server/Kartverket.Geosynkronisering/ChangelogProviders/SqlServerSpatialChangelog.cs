using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    /// <summary>
    /// SQL Server Spatial Provider part
    /// Written by Lars Eggan, NOIS, march 2016.
    /// </summary>
    public class SqlServerSpatialChangelog : SpatialDbChangelog
    {
        public override string GetLastIndex(int datasetId)
        {
            Int64 endChangeId = 0;
            try
            {
                //Connect to SQL Server  database

                SqlConnection conn = null;
                using (conn = new SqlConnection(PDbConnectInfo))
                {
                    conn.Open();
                    //Get max changelogid
                    endChangeId = GetMaxChangeLogId(conn, datasetId);
                    conn.Close();
                    Logger.Info("SqlServerSpatialChangelog.GetLastIndexResponse endChangeId :{0}{1}", "\t", endChangeId);
                }
            }

            catch (System.Exception exp)
            {
                Logger.Error(exp, "GetLastIndex Exception:");
                throw new System.Exception("GetLastIndex function failed", exp);
            }

            return endChangeId.ToString();
        }
        public override void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId)
        {
            Logger.Info("SqlServerSpatialChangelog..MakeChangeLog START");
            try
            {
                if (OptimizedChangeLog.Count == 0)
                {
                    //Connect to postgres database
                    SqlConnection conn = null;
                    conn = new SqlConnection(dbConnectInfo);
                    conn.Open();

                    //Get max changelogid
                    endChangeId = GetMaxChangeLogId(conn, datasetId);

                    //Prepare query against the changelog table in postgres
                    SqlCommand command = null;
                    PrepareChangeLogQuery(conn, ref command, startChangeId, endChangeId, datasetId);

                    //Execute query against the changelog table and remove unnecessary transactions.

                    FillOptimizedChangeLog(ref command, ref OptimizedChangeLog, startChangeId);
                }
                //Get features from WFS and add transactions to changelogfile
                BuildChangeLogFile(count, wfsUrl, startChangeId, changeLogFileName, datasetId);

                
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "SqlServerSpatialChangelog.MakeChangeLog function failed:");
                throw new System.Exception("MakeChangeLog function failed", exp);
            }
            Logger.Info("SqlServerSpatialChangelog.MakeChangeLog END");

        }

        private void FillOptimizedChangeLog(ref SqlCommand command,
            ref List<OptimizedChangeLogElement> optimizedChangeLog, int startChangeId)
        {
            Logger.Info("SqlServerSpatialChangelog.FillOptimizedChangeLog START");
            try
            {

                OrderedDictionary tempOptimizedChangeLog = new OrderedDictionary();
                //Fill optimizedChangeLog
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string gmlId = dr.GetString(0);

                        //gmlId = char.ToUpper(gmlId[0]) + gmlId.Substring(1);

                        string transType = dr.GetString(1);
                        long changelogId = dr.GetInt64(2);


                        OptimizedChangeLogElement optimizedChangeLogElement;
                        if (transType.Equals("D"))
                        {
                            //Remove if inserted or updated earlier in this sequence of transactions
                            if (tempOptimizedChangeLog.Contains(gmlId))
                            {
                                optimizedChangeLogElement = (OptimizedChangeLogElement)tempOptimizedChangeLog[gmlId];
                                string tempTransType = optimizedChangeLogElement.TransType;
                                tempOptimizedChangeLog.Remove(gmlId);
                                if (tempTransType.Equals("U"))
                                {
                                    //Add delete if last operation was update. 
                                    tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                                }
                            }
                            else
                            {
                                tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                            }
                        }
                        else
                        {
                            if (!tempOptimizedChangeLog.Contains(gmlId))
                            {
                                tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                            }
                        }
                    }
                }

                //Fill optimizedChangeLog
                foreach (var item in tempOptimizedChangeLog.Values)
                {
                    optimizedChangeLog.Add((OptimizedChangeLogElement)item);
                }


            }
            catch (Exception exp)
            {

                Logger.Error(exp, "SqlServerSpatialChangelog.FillOptimizedChangeLog function failed:");
                throw new System.Exception("FillOptimizedChangeLog function failed", exp);
            }
            Logger.Info("SqlServerSpatialChangelog.FillOptimizedChangeLog END");

        }


        private void PrepareChangeLogQuery(SqlConnection conn, ref SqlCommand command, int startChangeId, Int64 endChangeId, int datasetId)
        {
            Logger.Info("SqlServerSpatialChangelog.PrepareChangeLogQuery START");
            try
            {
                // 20160302-Leg: SQL server has different syntax from PostGIS
                string sqlSelectGmlIds = "SELECT tabell + '.' +  CONVERT(nvarchar(50),lokalid), type, endringsid FROM " + PDbSchema + ".endringslogg WHERE endringsid >= @startChangeId AND endringsid <= @endChangeId ORDER BY endringsid";
                // string sqlSelectGmlIds = "SELECT tabell || '.' || lokalid, type, endringsid FROM " + p_dbSchema + ".endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
                Logger.Info("SqlServerSpatialChangelog.PrepareChangeLogQuery sqlSelectGmlIds: {0}",sqlSelectGmlIds);

                command = new SqlCommand(sqlSelectGmlIds, conn);
                command.Parameters.Add(new SqlParameter("startChangeId", SqlDbType.Int));
                command.Parameters.Add(new SqlParameter("endChangeId", SqlDbType.Int));

                command.Prepare();

                command.Parameters[0].Value = startChangeId;
                command.Parameters[1].Value = endChangeId;
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "SqlServerSpatialChangelog.PrepareChangeLogQuery function failed:");
                throw new System.Exception("PrepareChangeLogQuery function failed", exp);
            }
            Logger.Info("SqlServerSpatialChangelog.PrepareChangeLogQuery END");
        }


        private Int64 GetMaxChangeLogId(SqlConnection conn, int datasetid)
        {
            Logger.Info("SqlServerSpatialChangelog.GetMaxChangeLogId START");
            try
            {
                Int64 endChangeId = 0;

                string sqlSelectMaxChangeLogId = "SELECT COALESCE(MAX(endringsid),0) FROM " + PDbSchema + ".endringslogg";

                SqlCommand cmd = new SqlCommand(sqlSelectMaxChangeLogId, conn);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read(); //Only one row
                endChangeId = dr.GetInt64(0);
                dr.Close();

                Logger.Info("SqlServerSpatialChangelog.GetMaxChangeLogId END");
                return endChangeId;
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "GetMaxChangeLogId function failed:");
                throw new System.Exception("GetMaxChangeLogId function failed", exp);
            }
        }

    }
}