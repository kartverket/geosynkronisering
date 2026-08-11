namespace Provider_NetCore
{
    public class JobStatus
    {
        public Status status;
        public Operation operation;


        public JobStatus(Status status)
        {
            this.status = status;
            operation = Operation.SYNC;
        }

        public JobStatus(Status status, Operation operation)
        {
            this.status = status;
            this.operation = operation;
        }
    }
}