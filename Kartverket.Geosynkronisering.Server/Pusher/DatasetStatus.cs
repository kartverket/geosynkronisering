using System;

namespace Provider_NetCore
{
    public class DatasetStatus
    {
        public string access { get; set; }
        public string crs_EPSG { get; set; }
        public DateTime? dataset_last_modified { get; set; }
        public Guid id { get; set; }
        public int? last_copy_transaction_number { get; set; }
        public string name { get; set; }
        public double resolution { get; set; }
        public string schema_url { get; set; }
    }
}