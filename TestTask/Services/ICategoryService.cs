using TestTask.Models;

namespace TestTask.Services
{
    public interface ICategoryService
    {
        public Task<ICollection<Category>> GetCategories();
        public Task<Category> GetCategoryById(int id);
        public Task<Category> GetCategoryByName(string name);
        public Task<Category> InsertCategory(Category category);
        public Task<Category> UpdateCategory(Category category);
        public Task<Category> DeleteCategory(int id);
    }
}
