namespace Shared.DocumentDb
{
    public class PagedDbResponse<T> : DbListResponse<T>
    {
        public PagedDbResponse()
        {
            RequestCount = 1;
        }

        public string NextPageToken { get; set; }
    }
}