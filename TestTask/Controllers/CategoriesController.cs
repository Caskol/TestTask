using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.SqlData;
using TestTask.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using TestTask.Services;

namespace TestTask.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await categoryService.GetCategories();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await categoryService.GetCategoryById(id);
            return Ok(category);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory([FromBody] Category category, int id)
        {
            category.Id = id;
            category = await categoryService.InsertCategory(category);
            return Ok(category);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await categoryService.DeleteCategory(id);
            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> PostCategory([FromBody] Category category)
        {
            category = await categoryService.InsertCategory(category);
            return Ok(category);
        }
    }
}
