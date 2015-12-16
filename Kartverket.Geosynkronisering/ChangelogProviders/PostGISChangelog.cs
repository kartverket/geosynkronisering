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
                conn = new NpgsqlConnection(p_dbConnectInfo);
                conn.Open();

                //Get max changelogid
                endChangeId = GetMaxChangeLogId(conn, datasetId);

                conn.Close();
                logger.Info("GetLastIndexResponse endChangeId :{0}{1}", "\t", endChangeId);
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("GetLastIndex Exception:", exp);
                throw new System.Exception("GetLastIndex function failed", exp);
            }

            return endChangeId.ToString();
        }


        //Build changelog responsefile
        public override void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId)
        {
            logger.Info("MakeChangeLog START");
            try
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

                List<OptimizedChangeLogElement> optimizedChangeLog = new List<OptimizedChangeLogElement>();
                //Execute query against the changelog table and remove unnecessary transactions.
                FillOptimizedChangeLog(ref command, ref optimizedChangeLog, startChangeId);

                //Get features from WFS and add transactions to changelogfile
                BuildChangeLogFile(count, optimizedChangeLog, wfsUrl, startChangeId, endChangeId, changeLogFileName, datasetId);

                conn.Close();
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("MakeChangeLog function failed:", exp);
                throw new System.Exception("MakeChangeLog function failed", exp);
            }
            logger.Info("MakeChangeLog END");
        }

        private void FillOptimizedChangeLog(ref Npgsql.NpgsqlCommand command, ref List<OptimizedChangeLogElement> optimizedChangeLog, int startChangeId)
        {
            logger.Info("FillOptimizedChangeLog START");
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
                                string tempTransType = optimizedChangeLogElement._transType;
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
                logger.ErrorException("FillOptimizedChangeLog function failed:", exp);
                throw new System.Exception("FillOptimizedChangeLog function failed", exp);
            }
            logger.Info("FillOptimizedChangeLog END");
        }

        private void PrepareChangeLogQuery(Npgsql.NpgsqlConnection conn, ref Npgsql.NpgsqlCommand command, int startChangeId, Int64 endChangeId, int datasetId)
        {
            logger.Info("PrepareChangeLogQuery START");
            try
            {
                //20121021-Leg: Correction "endringsid >= :startChangeId"
                //20121031-Leg: rad is now lokalId

                string sqlSelectGmlIds = "SELECT tabell || '.' || lokalid, type, endringsid FROM " + p_dbSchema + ".endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
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
                logger.ErrorException("PrepareChangeLogQuery function failed:", exp);
                throw new System.Exception("PrepareChangeLogQuery function failed", exp);
            }
            logger.Info("PrepareChangeLogQuery END");
        }

        private Int64 GetMaxChangeLogId(Npgsql.NpgsqlConnection conn, int datasetid)
        {
            logger.Info("GetMaxChangeLogId START");
            try
            {
                Int64 endChangeId = 0;
       
                string sqlSelectMaxChangeLogId = "SELECT COALESCE(MAX(endringsid),0) FROM " + p_dbSchema + ".endringslogg";

                NpgsqlCommand command = new NpgsqlCommand(sqlSelectMaxChangeLogId, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                dr.Read(); //Only one row
                endChangeId = dr.GetInt64(0);
                dr.Close();
                logger.Info("GetMaxChangeLogId END");
                return endChangeId;
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("GetMaxChangeLogId function failed:", exp);
                throw new System.Exception("GetMaxChangeLogId function failed", exp);
            }
        }       
    }
    #endregion

    #region changeLogHandler

    #endregion
}
