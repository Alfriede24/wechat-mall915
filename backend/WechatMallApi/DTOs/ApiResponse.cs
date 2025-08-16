namespace WechatMallApi.DTOs
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool Success => Code == 0;

        public static ApiResponse<T> Ok(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Code = 0,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message, int code = -1)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = default(T)
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "操作成功")
        {
            return new ApiResponse
            {
                Code = 0,
                Message = message
            };
        }

        public new static ApiResponse Error(string message, int code = -1)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message
            };
        }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;
    }
}