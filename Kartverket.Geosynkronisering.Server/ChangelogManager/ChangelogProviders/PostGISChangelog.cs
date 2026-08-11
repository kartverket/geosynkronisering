using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Xml.Serialization;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using System.Xml.Xsl;
using System.Collections;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{

    #region IChangelogSource Members
    public class PostGISChangelog : SpatialDbChangelog //, IChangelogProvider
    {
        //private geosyncEntities p_db;

        

        public override string GetLastIndex(int datasetId)
        {
            Int64 endChangeId = 0;
            try
            {
                //Connect to postgres database
                Npgsql.NpgsqlConnection conn = null;
                //logger.Info("PostGISChangelog.GetLastIndex" + " NpgsqlConnection:{0}", p_db.Datasets.);
                conn = new NpgsqlConnection(PDbConnectInfo);
                conn.Open();

                //Get max changelogid
                endChangeId = GetMaxChangeLogId(conn, datasetId);

                conn.Close();
                Logger.Info("GetLastIndexResponse endChangeId :{0}{1}", "\t", endChangeId);
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "GetLastIndex Exception:");
                throw new System.Exception("GetLastIndex function failed", exp);
            }

            return endChangeId.ToString();
        }


        //Build changelog responsefile
        public override void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId)
        {
            Logger.Info("MakeChangeLog START");
            try
            {
                if (OptimizedChangeLog.Count == 0)
                {
                    //Connect to postgres database
                    Npgsql.NpgsqlConnection conn = null;
                    conn = new NpgsqlConnection(dbConnectInfo);
                    conn.Open();

                    //Get max changelogid
                    Int64 endChangeId = GetMaxChangeLogId(conn, datasetId);

                    //Prepare query against the changelog table in postgres
                    Npgsql.NpgsqlCommand command = null;
                    PrepareChangeLogQuery(conn, ref command, startChangeId, endChangeId, datasetId);

                    //Execute query against the changelog table and remove unnecessary transactions.
                    
                    FillOptimizedChangeLog(ref command, ref OptimizedChangeLog, startChangeId);

                    conn.Close();
                }
                //Get features from WFS and add transactions to changelogfile
                BuildChangeLogFile(count, wfsUrl, startChangeId, changeLogFileName, datasetId);
               
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "MakeChangeLog function failed:");
                throw new System.Exception("MakeChangeLog function failed", exp);
            }
            Logger.Info("MakeChangeLog END");
        }

        private void FillOptimizedChangeLog(ref Npgsql.NpgsqlCommand command, ref List<OptimizedChangeLogElement> optimizedChangeLog, int startChangeId)
        {
            Logger.Info("FillOptimizedChangeLog START");
            try
            {
                OrderedDictionary tempOptimizedChangeLog = new OrderedDictionary();
                //Fill optimizedChangeLog
                using (NpgsqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string gmlId = dr.GetString(0);

                        // TODO: Fix dirty implementation later - 20121006-Leg: Uppercase First Letter
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
            catch (System.Exception exp)
            {
                Logger.Error(exp,"FillOptimizedChangeLog function failed:");
                throw new System.Exception("FillOptimizedChangeLog function failed", exp);
            }
            Logger.Info("FillOptimizedChangeLog END");
        }

        private void PrepareChangeLogQuery(Npgsql.NpgsqlConnection conn, ref Npgsql.NpgsqlCommand command, int startChangeId, Int64 endChangeId, int datasetId)
        {
            Logger.Info("PrepareChangeLogQuery START");
            try
            {
                //20121021-Leg: Correction "endringsid >= :startChangeId"
                //20121031-Leg: rad is now lokalId

                string sqlSelectGmlIds = "SELECT tabell || '.' || lokalid, type, endringsid FROM " + PDbSchema + ".endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
                //string sqlSelectGmlIds = "SELECT tabell || '.' || rad, type FROM tilbyder.endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
                // string sqlSelectGmlIds = "SELECT tabell || '.' || rad, type FROM tilbyder.endringslogg WHERE endringsid > :startChangeId AND endringsid <= :endChangeId ";
                command = new NpgsqlCommand(sqlSelectGmlIds, conn);

                command.Parameters.Add(new NpgsqlParameter("startChangeId", NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("endChangeId", NpgsqlDbType.Integer));

                command.Prepare();

                command.Parameters[0].Value = startChangeId;
                command.Parameters[1].Value = endChangeId;
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "PrepareChangeLogQuery function failed:");
                throw new System.Exception("PrepareChangeLogQuery function failed", exp);
            }
            Logger.Info("PrepareChangeLogQuery END");
        }

        private Int64 GetMaxChangeLogId(Npgsql.NpgsqlConnection conn, int datasetid)
        {
            Logger.Info("GetMaxChangeLogId START");
            try
            {
                Int64 endChangeId = 0;
       
                string sqlSelectMaxChangeLogId = "SELECT COALESCE(MAX(endringsid),0) FROM " + PDbSchema + ".endringslogg";

                NpgsqlCommand command = new NpgsqlCommand(sqlSelectMaxChangeLogId, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                dr.Read(); //Only one row
                endChangeId = dr.GetInt64(0);
                dr.Close();
                Logger.Info("GetMaxChangeLogId END");
                return endChangeId;
            }
            catch (System.Exception exp)
            {
                Logger.Error(exp, "GetMaxChangeLogId function failed:");
                throw new System.Exception("GetMaxChangeLogId function failed", exp);
            }
        }       
    }
    #endregion

    #region changeLogHandler

    #endregion
}
