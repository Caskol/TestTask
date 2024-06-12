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
using TestTask.Services;

namespace TestTask.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class DailyEventsController : ControllerBase
    {
        private readonly IDailyEventService dailyEventService;

        public DailyEventsController(IDailyEventService dailyEventService)
        {
            this.dailyEventService = dailyEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await dailyEventService.GetDailyEvents();
            return Ok(events);
        }
        [HttpGet("date/{datetime}")]
        public async Task<IActionResult> GetEventsByDate(DateTime datetime)
        {
            var events = await dailyEventService.GetDailyEventsByDate(datetime);
            return Ok(events);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var dailyEvent = await dailyEventService.GetDailyEventById(id);
            return Ok(dailyEvent);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent([FromBody] DailyEvent dailyEvent, int id)
        {
            dailyEvent.Id = id;
            dailyEvent = await dailyEventService.UpdateDailyEvent(dailyEvent);
            return Ok(dailyEvent);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var dailyEvent = await dailyEventService.DeleteDailyEvent(id);
            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> PostEvents([FromBody] DailyEvent dailyEvent)
        {
            dailyEvent = await dailyEventService.InsertDailyEvent(dailyEvent);
            return Ok(dailyEvent);
        }

        
    }
}
