namespace BlogApi.DTOs
{
    //tránh việt lồng nhau khó kiểm soát
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }

        // Chỉ lấy thông tin cần thiết, không lấy nguyên Object
        public string CategoryName { get; set; } = string.Empty;
        public List<string> TagNames { get; set; } = new List<string>();
    }
    // Dùng cho trang danh sách (Gọn nhẹ để load nhanh)
    public class PostSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; } // Nên có cái này để user biết bài viết nói về gì
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>(); // Nên hiện tag ở danh sách
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
        public string? Summary { get; set; }
        public string? Thumbnail { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>(); // Nhận danh sách ID của các Tag
    }
}
