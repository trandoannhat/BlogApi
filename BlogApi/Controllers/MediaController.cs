using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Authorize] // Chỉ Admin mới được upload
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

    [HttpGet]
    public async Task<IActionResult> GetMediaList()
    {
        var medias = await _uow.Medias.GetAllAsync();
        // Trả về danh sách ảnh mới nhất lên đầu
        return SuccessResponse(medias.OrderByDescending(m => m.CreatedAt), "Lấy danh sách ảnh thành công");
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")] // Chỉ định rõ kiểu dữ liệu cho Swagger
    public async Task<IActionResult> Upload([FromForm] MapUploadRequest request)
    {
        var file = request.File; // Lấy file từ request
        if (file == null || file.Length == 0)
            return ErrorResponse("Upload", "Vui lòng chọn một file ảnh");

        // 1. Upload lên Cloudinary
        var result = await _mediaService.AddImageAsync(file);
        if (result.Error != null)
            return ErrorResponse("Cloudinary", result.Error.Message);

        // 2. Lưu thông tin vào Database
        var media = new Media
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            FileName = file.FileName,
            CreatedAt = DateTime.UtcNow // Dùng UtcNow thay vì Now để Postgres không bắt bẻ
        };

        await _uow.Medias.AddAsync(media);
        await _uow.CompleteAsync();

        return SuccessResponse(media, "Tải ảnh lên thành công");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _uow.Medias.GetByIdAsync(id);
        if (media == null) return ErrorResponse("Delete", "Ảnh không tồn tại");

        // 1. Xóa trên Cloudinary
        var result = await _mediaService.DeleteImageAsync(media.PublicId);

        // 2. Xóa trong Database
        _uow.Medias.Delete(media);
        await _uow.CompleteAsync();

        return SuccessResponse<object>(null, "Đã xóa ảnh vĩnh viễn");
    }
    public class MapUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
