using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    public class SqlServerSpatialChangelog : SpatialDbChangelog
    {
        //TODO: Implement SQL Server Spatial provider
        public override string GetLastIndex(int datasetId)
        {
            throw new NotImplementedException();
        }

        public override void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId)
        {
            throw new NotImplementedException();
        }
    }
}