namespace BlogApi.DTOs
{
    // Dùng cho trang danh sách (Gọn nhẹ để load nhanh)
    public class PostSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Dùng cho trang chi tiết bài viết (Đầy đủ thông tin)
    public class PostDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new(); // Danh sách tên các thẻ
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Dùng để nhận dữ liệu từ Client khi tạo bài viết
    public class PostCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
