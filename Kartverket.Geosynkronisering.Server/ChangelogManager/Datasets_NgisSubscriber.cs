using Dapper.Contrib.Extensions;

namespace ChangelogManager
{
    public class Datasets_NgisSubscriber
    {
        public int id { get; set; }
        public int datasetid { get; set; }
        public int subscriberid { get; set; }
        public string subscriberdatasetid { get; set; }
        public string subscriberdatasetname { get; set; }
        [Computed] // this property is computed and should not be part of updates or inserts
        public NgisSubscriber subscriber {get;set;}
        [Computed] // this property is computed and should not be part of updates  or inserts
        public Dataset dataset { get; set; }
    }


}