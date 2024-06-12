using TestTask.Models;

namespace TestTask.Services
{
    public interface IDailyEventService
    {
        public Task<ICollection<DailyEvent>> GetDailyEvents();
        public Task<DailyEvent> GetDailyEventById(int id);
        public Task<ICollection<DailyEvent>> GetDailyEventsByDate(DateTime date);
        public Task<DailyEvent> DeleteDailyEvent(int id);
        public Task<DailyEvent> InsertDailyEvent(DailyEvent dailyEvent);
        public Task<DailyEvent> UpdateDailyEvent(DailyEvent dailyEvent);
    }
}
