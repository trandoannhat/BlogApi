namespace BlogApi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // Dùng cho URL: nhatdev.top/category/lap-trinh-net
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
