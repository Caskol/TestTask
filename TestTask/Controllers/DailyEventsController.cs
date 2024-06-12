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
        ~DailyEventsController()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.CloseAsync();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            List<DailyEvent> eventsFromDb = new List<DailyEvent>();
            using (var command = new NpgsqlCommand("SELECT e.id ,e.name,e.date,e.category_id,c.name AS category_name,c.hex_color FROM events e  LEFT JOIN categories c ON e.category_id=c.id\r\n", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DailyEvent dailyEvent = new DailyEvent();
                        dailyEvent.Id = reader.GetInt32("id");
                        dailyEvent.Name = reader.GetString("name");
                        dailyEvent.EventDate= reader.GetDateTime("date");
                        if (!reader.IsDBNull("category_id"))
                        {
                            dailyEvent.CategoryId = reader.GetInt32("category_id");
                            dailyEvent.Category = new Category();
                            dailyEvent.Category.Id = reader.GetInt32("category_id");
                            dailyEvent.Category.Name = reader.GetString("category_name");
                            dailyEvent.Category.ColorInHex = reader.GetString("hex_color");
                        }
                        eventsFromDb.Add(dailyEvent);
                    }
                }
            }
            return Ok(eventsFromDb);
        }
        [HttpGet("date/{datetime}")]
        public async Task<IActionResult> GetEventByDate(DateTime datetime)
        {
            List<int> ids = new List<int>();
            List<DailyEvent> events = new List<DailyEvent>();
            string query = $"SELECT id FROM events WHERE date>='{datetime.Date.ToString("yyyy-MM-dd")}' AND date<='{datetime.Date.AddDays(1).ToString("yyyy-MM-dd")}'";
            NpgsqlCommand sqlCommand = new NpgsqlCommand(query, connection);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            using (NpgsqlDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32("id"));
                }
            };
            foreach (int id in ids){
                DailyEvent dailyEvent = new DailyEvent();
                events.Add(await ExecuteSqlProcedure("call get_event(:_id, :_name, :_date, :_category_id, :_category_name, :_category_color)", id, dailyEvent));
            }
            return Ok(events);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
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
            if (connection.State == ConnectionState.Closed)
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
                    Value = dailyEvent.Name
                };
                var dateParam = new NpgsqlParameter("_date", NpgsqlDbType.Timestamp)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.EventDate
                };
                var categoryIdParam = new NpgsqlParameter("_category_id", NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.CategoryId.HasValue ? dailyEvent.CategoryId.Value : DBNull.Value
                };
                var categoryNameParam = new NpgsqlParameter("_category_name", NpgsqlDbType.Varchar,20)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.Category.Name==null ? DBNull.Value : dailyEvent.Category.Name
                };
                var categoryColorParam = new NpgsqlParameter("_category_color", NpgsqlDbType.Varchar,7)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = dailyEvent.Category.ColorInHex==null ? DBNull.Value : dailyEvent.Category.ColorInHex
                };
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
                    if (categoryIdParam.Value is not DBNull)
                    {
                        dailyEvent.CategoryId = (int)categoryIdParam.Value;
                        dailyEvent.Category.Id = (int)categoryIdParam.Value;
                        dailyEvent.Category.Name = categoryNameParam.Value.ToString();
                        dailyEvent.Category.ColorInHex = categoryColorParam.Value.ToString();
                    }
                    else
                    {
                        dailyEvent.CategoryId = null;
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
