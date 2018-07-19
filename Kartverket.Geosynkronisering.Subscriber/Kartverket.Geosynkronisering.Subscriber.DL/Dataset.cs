using Dapper.Contrib.Extensions;

namespace Kartverket.Geosynkronisering.Subscriber.DL
{
    public class Dataset
    {
        public string ProviderDatasetId { get; set; }
        public string Name { get; set; }
        public int MaxCount { get; set; }
        //public string Applicationschema { get; set; }
        public string TargetNamespace { get; set; }
        
        public string SyncronizationUrl { get; set; }

        [Key]
        public int DatasetId { get; set; }
        public long LastIndex { get; set; }
        public string ClientWfsUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MappingFile { get; set; }
        public long? AbortedEndIndex { get; set; }
        public long? AbortedTransaction { get; set; }
        public string AbortedChangelogPath { get; set; }
        public string ChangelogDirectory { get; set; }
        public string AbortedChangelogId { get; set; }
        public string Version { get; set; }
        public double Tolerance { get; set; }
        public string EpsgCode { get; set; }
        public string Decimals { get; set; }
    }
}