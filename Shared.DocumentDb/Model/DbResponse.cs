namespace Shared.DocumentDb
{
    public class DbResponse<T>
    {
        public DbResponse()
        {
            RequestCount = 1;
        }

        public string ETag { get; set; }
        public double OperationCost { get; set; }
        public int RequestCount { get; set; }
        public T Entity { get; set; }
    }
}