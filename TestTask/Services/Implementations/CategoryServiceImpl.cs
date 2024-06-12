using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using TestTask.Models;
using TestTask.SqlData;

namespace TestTask.Services.Implementations
{
    public class CategoryServiceImpl : ICategoryService
    {
        private readonly AppDbContext appDbContext;


        public CategoryServiceImpl(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<Category> DeleteCategory(int id)
        {
            return await ExecuteSqlProcedure("call delete_category(:_id)", new Category() { Id = id });
        }

        public async Task<ICollection<Category>> GetCategories()
        {
            List<Category> categoriesFromDb = new List<Category>();
            await using (var connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString()))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                
                await using (var command = new NpgsqlCommand("SELECT * FROM categories", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Category category = new Category();
                            category.Id = reader.GetInt32("id");
                            category.Name = reader.GetString("name");
                            category.ColorInHex = reader.GetString("hex_color");
                            categoriesFromDb.Add(category);
                        }
                    }
                }
            }
            return categoriesFromDb;
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await ExecuteSqlProcedure("call get_category(:_id, :_name, :_hex_color)", new Category() { Id=id});
        }

        public async Task<Category> GetCategoryByName(string name)
        {
            return await ExecuteSqlProcedure("call get_category(:_id, :_name, :_hex_color)", new Category() { Name = name });
        }

        public async Task<Category> InsertCategory(Category category)
        {
            return await ExecuteSqlProcedure("call insert_category(:_id,:_name, :_hex_color)", category);
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            return await ExecuteSqlProcedure("call modify_category(:_id, :_name, :_hex_color)", category);
        }

        private async Task<Category> ExecuteSqlProcedure(string query, Category category)
        {
            await using (var connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString()))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                await using (var command = new NpgsqlCommand(query, connection))
                {
                    var idParam = new NpgsqlParameter("_id", NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = category.Id.HasValue ? category.Id.Value : DBNull.Value
                    };
                    var nameParam = new NpgsqlParameter("_name", NpgsqlDbType.Varchar, 20)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = category.Name ?? ""
                    };
                    var colorParam = new NpgsqlParameter("_hex_color", NpgsqlDbType.Varchar, 7)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = category.ColorInHex ?? ""
                    };
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(nameParam);
                    command.Parameters.Add(colorParam);

                    Console.WriteLine(command.CommandText + idParam.Value + nameParam.Value + colorParam.Value);

                    await command.ExecuteNonQueryAsync();
                    if (idParam.Value is DBNull)
                        throw new BadHttpRequestException("Такого объекта нет в базе данных");
                    category.Id = (int)idParam.Value;
                    category.Name = nameParam.Value.ToString();
                    category.ColorInHex = colorParam.Value.ToString();
                }
                return category;
            }
        }
    }
}
