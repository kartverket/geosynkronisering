using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    public class OracleSpatialChangelog : SpatialDbChangelog
    {
        //TODO: Implement Oracle Spatial provider
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