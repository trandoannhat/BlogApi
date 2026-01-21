namespace BlogApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ với Post
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        // Quan hệ với User (Người bình luận)
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // (Tùy chọn) Bình luận lồng nhau: Reply cho một comment khác
        public int? ParentId { get; set; }
        public Comment? Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
