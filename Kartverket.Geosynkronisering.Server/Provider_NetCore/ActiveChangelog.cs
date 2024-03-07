using ChangelogManager;
using Kartverket.GeosyncWCF;

namespace Provider_NetCore
{
    internal class ActiveChangelog
    {
        public ActiveChangelog()
        {
        }

        public ChangelogType changelog { get; set; }
        public int copy_transaction_number { get; set; }
        public Dataset dataset { get; set; }
    }
}