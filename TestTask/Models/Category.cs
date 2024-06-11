namespace TestTask.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string ColorInHex { get; set; } = null!;

        public virtual ICollection<DailyEvent>? DailyEvents { get; set; }
    }
}
