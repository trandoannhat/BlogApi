namespace BlogApi.Models;

public class Media
{
    public int Id { get; set; }
    public string PublicId { get; set; } // Dùng để xóa ảnh trên Cloudinary
    public string Url { get; set; }      // Đường dẫn ảnh hiển thị
    public string FileName { get; set; } // Tên gốc của file
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
