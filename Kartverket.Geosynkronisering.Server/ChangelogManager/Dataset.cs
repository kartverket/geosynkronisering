using System;
using Dapper.Contrib.Extensions;
namespace ChangelogManager
{
    [Table("Datasets")]
    public class Dataset
    {

        public Dataset()
        {
            this.Decimals = "3";
            this.Tolerance = -1D;
        }

        [Key]
        public int DatasetId { get; set; }
        public string Name { get; set; }
        public string SchemaFileUri { get; set; }
        public string DatasetProvider { get; set; }
        public Nullable<int> ServerMaxCount { get; set; }
        public string DatasetConnection { get; set; }
        public string DBSchema { get; set; }
        public string TransformationConnection { get; set; }
        public string DefaultCrs { get; set; }
        public string UpperCornerCoords { get; set; }
        public string LowerCornerCoords { get; set; }
        public string TargetNamespace { get; set; }
        public string TargetNamespacePrefix { get; set; }
        public string Version { get; set; }
        public string Decimals { get; set; }
        public double Tolerance { get; set; }

    }
}
