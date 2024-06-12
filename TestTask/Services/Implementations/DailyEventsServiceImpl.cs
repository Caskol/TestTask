using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using TestTask.Models;
using TestTask.SqlData;

namespace TestTask.Services.Implementations
{
    public class DailyEventsServiceImpl : IDailyEventService
    {
        private readonly AppDbContext appDbContext;
        public DailyEventsServiceImpl(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<DailyEvent> DeleteDailyEvent(int id)
        {
            return await ExecuteSqlProcedure("call delete_event(:_id)", new DailyEvent() { Id=id});
        }

        public async Task<ICollection<DailyEvent>> GetDailyEventsByDate(DateTime datetime)
        {
            List<DailyEvent> events = new List<DailyEvent>();
            await using (var connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString()))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                List<int> ids = new List<int>();
                
                string query = $"SELECT id FROM events WHERE date>='{datetime.Date.ToString("yyyy-MM-dd")}' AND date<='{datetime.Date.AddDays(1).ToString("yyyy-MM-dd")}'";
                NpgsqlCommand sqlCommand = new NpgsqlCommand(query, connection);
                
                await using (NpgsqlDataReader reader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ids.Add(reader.GetInt32("id"));
                    }
                };
                foreach (int id in ids)
                {
                    events.Add(await ExecuteSqlProcedure("call get_event(:_id, :_name, :_date, :_category_id, :_category_name, :_category_color)", new DailyEvent() {Id=id }));
                }
            }
            return events;
        }

        public async Task<DailyEvent> GetDailyEventById(int id)
        {
            return await ExecuteSqlProcedure("call get_event(:_id, :_name, :_date, :_category_id, :_category_name, :_category_color)", new DailyEvent() { Id = id });
        }

        public async Task<ICollection<DailyEvent>> GetDailyEvents()
        {
            List<DailyEvent> eventsFromDb = new List<DailyEvent>();
            await using (var connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString()))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT e.id ,e.name,e.date,e.category_id,c.name AS category_name,c.hex_color FROM events e  LEFT JOIN categories c ON e.category_id=c.id\r\n", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DailyEvent dailyEvent = new DailyEvent();
                            dailyEvent.Id = reader.GetInt32("id");
                            dailyEvent.Name = reader.GetString("name");
                            dailyEvent.EventDate = reader.GetDateTime("date");
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
            }
            return eventsFromDb;
        }

        public async Task<DailyEvent> InsertDailyEvent(DailyEvent dailyEvent)
        {
            return await ExecuteSqlProcedure("call insert_event(:_id,:_name, :_date, :_category_id)", dailyEvent);
        }

        public async Task<DailyEvent> UpdateDailyEvent(DailyEvent dailyEvent)
        {
            return await ExecuteSqlProcedure("call modify_event(:_id, :_name, :_date, :_category_id)", dailyEvent);
        }

        private async Task<DailyEvent> ExecuteSqlProcedure(string query, DailyEvent dailyEvent)
        {
            await using (var connection = new NpgsqlConnection(appDbContext.Database.GetConnectionString()))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                if (dailyEvent.Category==null)
                    dailyEvent.Category = new Category();
                await using (var command = new NpgsqlCommand(query, connection))
                {
                    var idParam = new NpgsqlParameter("_id", NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = dailyEvent.Id.HasValue ? dailyEvent.Id.Value : DBNull.Value
                    };
                    var nameParam = new NpgsqlParameter("_name", NpgsqlDbType.Varchar, 200)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = dailyEvent.Name == null ? DBNull.Value : dailyEvent.Name
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
                    var categoryNameParam = new NpgsqlParameter("_category_name", NpgsqlDbType.Varchar, 20)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = dailyEvent.Category.Name == null ? DBNull.Value : dailyEvent.Category.Name
                    };
                    var categoryColorParam = new NpgsqlParameter("_category_color", NpgsqlDbType.Varchar, 7)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = dailyEvent.Category.ColorInHex == null ? DBNull.Value : dailyEvent.Category.ColorInHex
                    };
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(nameParam);
                    command.Parameters.Add(dateParam);
                    command.Parameters.Add(categoryIdParam);
                    command.Parameters.Add(categoryNameParam);
                    command.Parameters.Add(categoryColorParam);

                    Console.WriteLine(command.CommandText + idParam.Value + nameParam.Value + dateParam.Value + categoryIdParam);

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
            }
            return dailyEvent;
        }
    }
}
