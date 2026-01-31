using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class MediaController : BaseController
{
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _uow;

    public MediaController(IMediaService mediaService, IUnitOfWork uow)
    {
        _mediaService = mediaService;
        _uow = uow;
    }

    // ===============================
    // GET: api/media
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetMediaList()
    {
        var medias = await _uow.Medias.GetAllAsync();
        return SuccessResponse(
            medias.OrderByDescending(x => x.CreatedAt),
            "Lấy danh sách ảnh thành công"
        );
    }

    // ===============================
    // POST: api/media/upload
    // ===============================
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadMediaRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return ErrorResponse("Upload", "Vui lòng chọn file ảnh");

        // 1. Upload Cloudinary
        var uploadResult = await _mediaService.AddImageAsync(request.File);
        if (uploadResult.Error != null)
            return ErrorResponse("Cloudinary", uploadResult.Error.Message);

        // 2. Lưu DB
        var media = new Media
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            FileName = request.File.FileName,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Medias.AddAsync(media);
        await _uow.CompleteAsync();

        return SuccessResponse(media, "Upload ảnh thành công");
    }

    // ===============================
    // PUT: api/media/{id}
    // ===============================
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] UploadMediaRequest request)
    {
        var media = await _uow.Medias.GetByIdAsync(id);
        if (media == null)
            return ErrorResponse("Update", "Ảnh không tồn tại");

        if (request.File == null || request.File.Length == 0)
            return ErrorResponse("Update", "Vui lòng chọn ảnh mới");

        // 1. Xóa ảnh cũ trên Cloudinary
        if (!string.IsNullOrEmpty(media.PublicId))
        {
            var deleteResult = await _mediaService.DeleteImageAsync(media.PublicId);
            if (deleteResult.Error != null)
                return ErrorResponse("Cloudinary", deleteResult.Error.Message);
        }

        // 2. Upload ảnh mới
        var uploadResult = await _mediaService.AddImageAsync(request.File);
        if (uploadResult.Error != null)
            return ErrorResponse("Cloudinary", uploadResult.Error.Message);

        // 3. Update DB
        media.Url = uploadResult.SecureUrl.AbsoluteUri;
        media.PublicId = uploadResult.PublicId;
        media.FileName = request.File.FileName;
       // media.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync();

        return SuccessResponse(media, "Cập nhật ảnh thành công");
    }

    // ===============================
    // DELETE: api/media/{id}
    // ===============================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _uow.Medias.GetByIdAsync(id);
        if (media == null)
            return ErrorResponse("Delete", "Ảnh không tồn tại");

        // 1. Xóa Cloudinary
        if (!string.IsNullOrEmpty(media.PublicId))
        {
            var deleteResult = await _mediaService.DeleteImageAsync(media.PublicId);
            if (deleteResult.Error != null)
                return ErrorResponse("Cloudinary", deleteResult.Error.Message);
        }

        // 2. Xóa DB
        _uow.Medias.Delete(media);
        await _uow.CompleteAsync();

        return SuccessResponse<object>(null, "Đã xóa ảnh vĩnh viễn");
    }
}

// ===============================
// DTO
// ===============================
public class UploadMediaRequest
{
    public IFormFile File { get; set; }
}
