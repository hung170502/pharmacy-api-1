namespace Pharmacy_API.Supports
{
    public class PagedDto<T> where T : new()
    {
        public IReadOnlyList<T> Data { get; }
        public PagedDto(int total, IReadOnlyList<T> data)
        {
            TotalRecords = total;
            Data = data;
        }

        public int TotalRecords { get; set; }
    }
}