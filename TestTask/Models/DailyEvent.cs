namespace TestTask.Models
{
    public class DailyEvent
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public DateTime EventDate { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
