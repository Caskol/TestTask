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
using NuGet.Packaging.Signing;

namespace TestTask.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class DailyEventsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly NpgsqlConnection connection;

        public DailyEventsController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
            connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString());
        }
        [HttpGet]
        public IActionResult GetEvents()
        {
            return Ok(appDbContext.Events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvents(int id)
        {
            DailyEvent dailyEvent = new DailyEvent();
            await ExecuteSqlProcedure("call get_event(:_id, :_name, :_date, :_category_id, :_category_name, :_category_color)", id, dailyEvent);
            return Ok(dailyEvent);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvents([FromBody] DailyEvent dailyEvent, int id)
        {
            return Ok(await ExecuteSqlProcedure("call modify_event(:_id, :_name, :_date, :_category_id)", id, dailyEvent));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvents(int id)
        {
            await ExecuteSqlProcedure("call delete_event(:_id)", id);
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> PostEvents([FromBody] DailyEvent dailyEvent)
        {
            return Ok(await ExecuteSqlProcedure("call insert_event(:_id,:_name, :_date, :_category_id)", default, dailyEvent));
        }

        private async Task<DailyEvent> ExecuteSqlProcedure(string query, int id = 0, DailyEvent? dailyEvent = null)
        {
            await connection.OpenAsync();
            if (dailyEvent==null)
                dailyEvent = new DailyEvent();
            dailyEvent.Category = new Category();
            using (var command = new NpgsqlCommand(query, connection))
            {
                var idParam = new NpgsqlParameter("_id", NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = id
                };
                var nameParam = new NpgsqlParameter("_name", NpgsqlDbType.Varchar, 200)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.Name ?? ""
                };
                var dateParam = new NpgsqlParameter("_date", NpgsqlDbType.Timestamp)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent?.EventDate ?? DateTime.UnixEpoch
                };
                var categoryIdParam = new NpgsqlParameter("_category_id", NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.CategoryId ?? 0
                };
                var categoryNameParam = new NpgsqlParameter("_category_name", NpgsqlDbType.Varchar,20)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.Category?.Name ?? ""
                };
                var categoryColorParam = new NpgsqlParameter("_category_color", NpgsqlDbType.Varchar,7)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.Category?.ColorInHex ?? ""
                };
                //TODO: ffff
                command.Parameters.Add(idParam);
                command.Parameters.Add(nameParam);
                command.Parameters.Add(dateParam);
                command.Parameters.Add(categoryIdParam);
                command.Parameters.Add(categoryNameParam);
                command.Parameters.Add(categoryColorParam);

                Console.WriteLine(command.CommandText + idParam.Value + nameParam.Value + dateParam.Value+categoryIdParam);

                await command.ExecuteNonQueryAsync();
                try
                {
                    dailyEvent.Id = (int)idParam.Value;
                    dailyEvent.Name = nameParam.Value.ToString();
                    dailyEvent.EventDate = (DateTime)dateParam.Value;
                    if (idParam.Value is not DBNull)
                    {
                        dailyEvent.CategoryId = (int)categoryIdParam.Value;
                        dailyEvent.Category.Id = (int)categoryIdParam.Value;
                        dailyEvent.Category.Name = categoryNameParam.Value.ToString();
                        dailyEvent.Category.ColorInHex = categoryColorParam.Value.ToString();
                    }
                }
                catch (InvalidCastException e)
                {
                    throw new BadHttpRequestException("Такого объекта нет в базе данных");
                }
            }
            return dailyEvent;
        }
    }
}
