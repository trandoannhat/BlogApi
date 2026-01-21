using BlogApi.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        // Helper trả về thành công
        protected OkObjectResult SuccessResponse<T>(T data, string action, string message = "Thành công")
        {
            return Ok(new ApiResponse<T>(data, action, message));
        }

        // Helper trả về lỗi BadRequest
        protected BadRequestObjectResult ErrorResponse(string action, string message, List<string>? errors = null)
        {
            return BadRequest(new ApiResponse<string>(action, message, errors));
        }

        // Helper trả về lỗi NotFound
        protected NotFoundObjectResult NotFoundResponse(string action, string message = "Không tìm thấy dữ liệu")
        {
            return NotFound(new ApiResponse<string>(action, message));
        }
        // Lấy tên hàm đang chạy
        protected string CurrentAction => ControllerContext.ActionDescriptor.ActionName;
    }
}
