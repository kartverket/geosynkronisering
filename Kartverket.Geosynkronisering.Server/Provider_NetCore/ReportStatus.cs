namespace Provider_NetCore
{
    internal class ReportStatus
    {
        public Status status { get; set; }
        public int? last_transaction_number { get; set; }
        public int? last_copy_transaction_number { get; set; }
        public dynamic? message { get; set; }
    }
}