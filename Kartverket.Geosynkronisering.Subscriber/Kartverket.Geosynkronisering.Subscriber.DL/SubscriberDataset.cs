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
        public string Applicationschema { get; set; }
        public string MappingFile { get; set; }
        public long? AbortedEndIndex { get; set; }
        public long? AbortedTransaction { get; set; }
        public string AbortedChangelogPath { get; set; }
        public string ChangelogDirectory { get; set; }
        public string AbortedChangelogId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Version { get; set; }
    }
}