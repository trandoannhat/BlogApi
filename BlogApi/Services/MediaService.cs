using BlogApi.Helpers;
using BlogApi.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BlogApi.Services;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;

    public MediaService(IOptions<CloudinarySettings> config)
    {
        // Khởi tạo Cloudinary với thông số từ appsettings.json
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddImageAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                // Tự động tối ưu ảnh khi upload (Resize, Crop...)
                Transformation = new Transformation().Height(1200).Width(1200).Crop("limit"),
                Folder = "nhatdev_blog" // Ảnh sẽ vào folder này trên Cloudinary
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        return uploadResult;
    }

    public async Task<DeletionResult> DeleteImageAsync(string publicId)
    {
        return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}