namespace Shared.DocumentDb
{
    using System.Collections.Generic;

    public class DbListResponse<T>
    {
        public DbListResponse()
        {
            RequestCount = 1;
        }

        public double OperationCost { get; set; }
        public int RequestCount { get; set; }
        public IEnumerable<T> Entities { get; set; }
    }
}