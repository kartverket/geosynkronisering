namespace ChangelogManager
{
    public class Datasets_NgisSubscriber
    {
        public int id { get; set; }
        public int datasetid { get; set; }
        public int subscriberid { get; set; }
        public int subscriberdatasetid { get; set; }
        public NgisSubscriber subscriber {get;set;}
        public Dataset dataset { get; set; }
    }
}