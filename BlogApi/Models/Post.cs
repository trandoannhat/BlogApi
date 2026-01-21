namespace BlogApi.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; } // Tóm tắt ngắn cho trang danh sách
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; } // Ảnh đại diện bài viết
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = false;

        // Quan hệ 1-N: Một bài viết thuộc về 1 danh mục
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Quan hệ N-N: Một bài viết có nhiều tags
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }
}
