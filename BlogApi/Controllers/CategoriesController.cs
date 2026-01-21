using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesController : BaseController
    {
        private readonly IUnitOfWork _uow;
        public CategoriesController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _uow.Categories.GetAllAsync();
            return SuccessResponse(categories, "Get all categories");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ErrorResponse("Create category", "Tên không hợp lệ");

            var category = new Category { Name = name, Slug = name.ToSlug() };
            await _uow.Categories.AddAsync(category);
            await _uow.CompleteAsync();

            return SuccessResponse(category, "Create category");
        }
    }
}
