﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.SqlData;
using TestTask.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace TestTask.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly NpgsqlConnection connection;

        public CategoriesController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
            connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString());
        }
        ~CategoriesController() 
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.CloseAsync();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            List<Category> categoriesFromDb = new List<Category>();
            using (var command = new NpgsqlCommand("SELECT * FROM categories", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) 
                    {
                        Category category = new Category();
                        category.Id = reader.GetInt32("id");
                        category.Name = reader.GetString("name");
                        category.ColorInHex = reader.GetString("hex_color");
                        categoriesFromDb.Add(category);
                    }
                }
            }
            return Ok(categoriesFromDb);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            return Ok(await ExecuteSqlProcedure("call get_category(:_id, :_name, :_hex_color)", id));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory([FromBody] Category category, int id)
        {
            return Ok(await ExecuteSqlProcedure("call modify_category(:_id, :_name, :_hex_color)", id, category));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return Ok(await ExecuteSqlProcedure("call delete_category(:_id)", id));
        }
        [HttpPost]
        public async Task<IActionResult> PostCategory([FromBody] Category category)
        {
            return Ok(await ExecuteSqlProcedure("call insert_category(:_id,:_name, :_hex_color)", default ,category));
        }

        private async Task<Category> ExecuteSqlProcedure(string query, int? id = null, Category? category = null)
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            if (category==null)
                category = new Category();
            using (var command = new NpgsqlCommand(query, connection))
            {
                var idParam = new NpgsqlParameter("_id", NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = id.HasValue ? id.Value : DBNull.Value
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
