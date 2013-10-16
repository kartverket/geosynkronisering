using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public class SubscriberDataset
    {
        public int DatasetId { get; set; }
        public string Name { get; set; }
        public int LastIndex { get; set; }
        public string SynchronizationUrl { get; set; }
        public string ClientWfsUrl { get; set; }
        public int MaxCount { get; set; }
        public int ProviderDatasetId { get; set; }
        public string TargetNamespace { get; set; }
        public string MappingFile { get; set; }
    }
}
