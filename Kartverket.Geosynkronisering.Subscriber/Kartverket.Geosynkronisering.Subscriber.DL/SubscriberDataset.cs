namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public class SubscriberDataset
    {
        public int DatasetId { get; set; }
        public string Name { get; set; }
        public long LastIndex { get; set; }
        public string SynchronizationUrl { get; set; }
        public string ClientWfsUrl { get; set; }
        public int MaxCount { get; set; }
        public string ProviderDatasetId { get; set; }
        public string TargetNamespace { get; set; }
        public string MappingFile { get; set; }
        public long? AbortedEndIndex { get; set; }
        public long? AbortedTransaction { get; set; }
        public string AbortedChangelogPath { get; set; }
    }
}