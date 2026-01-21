namespace BlogApi.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Thêm trường này
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Constructor cho thành công
        public ApiResponse(T data, string action, string message = "Success")
        {
            Success = true;
            Message = message;
            Description = $"nhatdev - {action}"; // VD: nhatdev - Get all posts
            Data = data;
        }

        // Constructor cho lỗi
        public ApiResponse(string action, string message, List<string>? errors = null)
        {
            Success = false;
            Message = message;
            Description = $"nhatdev - {action}";
            Errors = errors;
        }
    }
}
