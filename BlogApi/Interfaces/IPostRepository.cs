using BlogApi.DTOs;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<PostSummaryDto>> GetAllPostsAsync(string? categorySlug = null);
        Task<PostDetailDto?> GetPostBySlugAsync(string slug);
        Task<bool> CreatePostAsync(Post post);
        // Thêm các hàm Update, Delete sau...
    }
}
