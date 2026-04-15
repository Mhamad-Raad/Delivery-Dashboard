namespace MalDash.Application.Responses.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = [];
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
    }
}