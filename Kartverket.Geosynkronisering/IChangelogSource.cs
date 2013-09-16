using Kartverket.GeosyncWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kartverket.Geosynkronisering
{
    public interface IChangelogProvider
    {
         void SetDb(geosyncEntities db);

        //XmlDocument GetCapabilities();
         string GetLastIndex(int datasetId);
        //XmlDocument DescribeFeatureType();
        //ListStoredChangelogsResponse ListStoredChangelogs();
         OrderChangelog OrderChangelog(int startIndex, int count, string todo_filter, int datasetId);
        //GetChangelogStatusResponse GetChangelogStatus(int changelogid);
        //GetChangelogResponse GetChangelog(int changelogid);
        //void AcknowledgeChangelogDownloaded(int changelogid);
        //void CancelChangelog(int changelogid);

    }
}
