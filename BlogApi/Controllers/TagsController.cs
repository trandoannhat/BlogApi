using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    public class TagsController : BaseController // Kế thừa từ BaseController
    {
        private readonly IUnitOfWork _uow;
        public TagsController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _uow.Tags.GetAllAsync();
            // Cách dùng trong Controller con
            return SuccessResponse(tags, CurrentAction);
            // return SuccessResponse(tags, "Get all tags"); // Dùng hàm từ Base
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            string action = "Create tag";
            if (string.IsNullOrEmpty(name))
                return ErrorResponse(action, "Tên thẻ không được để trống");

            var tag = new Tag { Name = name, Slug = name.ToSlug() };
            await _uow.Tags.AddAsync(tag);
            await _uow.CompleteAsync();

            return SuccessResponse(tag, action, "Tạo thẻ thành công");
        }
    }
}
