namespace Pharmacy_API.Supports
{
    public class FilterBase
    {
        public string? Keyword { get; set; }
        public string? OrderBy { get; set; }

        public bool IsDescending { get; set; }

        public string GetOrderDirection()
        {
            return IsDescending ? "DESC" : "ASC";
        }

        private int pageIndex;

        public int PageIndex
        {
            get { return pageIndex == 0 ? 1 : pageIndex; }
            set { pageIndex = value; }
        }

        private int pageSize;

        public int PageSize
        {
            get { return pageSize == 0 ? 1000 : pageSize; }
            set { pageSize = value; }
        }

        public bool IsOutputTotal { get; set; }

        public bool IsDeep { get; set; }

        public int GetSkip()
        {
            return (PageIndex - 1) * PageSize;
        }

        public int GetTake()
        {
            return PageSize;
        }
    }
}