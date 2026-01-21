using BlogApi.DTOs;

namespace BlogApi.Services
{
    public interface IPostService
    {
        Task<bool> CreateBlogPost(PostCreateDto dto);
        Task<IEnumerable<PostSummaryDto>> GetRecentPostsAsync();
    }
}