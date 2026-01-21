using BlogApi.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize] // Chỉ Admin mới được upload
    public class UploadController : BaseController // Kế thừa từ BaseController
    {
        private readonly Cloudinary _cloudinary;

        public UploadController(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // Sử dụng helper CurrentAction từ BaseController (nếu bạn đã thêm) 
            // hoặc viết cứng chuỗi action
            string action = "Upload to Cloudinary";

            if (file == null || file.Length == 0)
                return ErrorResponse(action, "Vui lòng chọn một file ảnh");

            var uploadResult = new ImageUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "blog",
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(450)
                        .Crop("fill")
                        .Gravity("auto")
                        .Quality("auto")
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
                return ErrorResponse(action, uploadResult.Error.Message);

            // Trả về response chuẩn nhatdev qua SuccessResponse
            return SuccessResponse(
                new { url = uploadResult.SecureUrl.ToString() },
                action,
                "Tải ảnh lên thư mục /blog thành công"
            );
        }
    }
}