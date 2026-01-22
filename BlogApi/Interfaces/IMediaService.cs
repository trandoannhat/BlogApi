using CloudinaryDotNet.Actions;

namespace BlogApi.Interfaces;

public interface IMediaService
{
    Task<ImageUploadResult> AddImageAsync(IFormFile file);
    Task<DeletionResult> DeleteImageAsync(string publicId);
}
