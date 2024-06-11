using Microsoft.EntityFrameworkCore;
using TestTask.Models;

namespace TestTask.SqlData
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<DailyEvent> Events { get; set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                //Первичная настройка
                entity.ToTable("categories");
                entity.HasKey(c => c.Id); //  назначаем первичный ключ для таблицы
                entity.HasIndex(c => c.Name).IsUnique();


                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(20).IsRequired();
                entity.Property(c => c.ColorInHex).HasColumnName("hex_color").HasMaxLength(7).IsRequired();
            });

            modelBuilder.Entity<DailyEvent>(entity =>
            {
                entity.ToTable("events");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
                entity.Property(c => c.EventDate).HasColumnName("date").IsRequired();
                entity.Property(c => c.CategoryId).HasColumnName("category_id");

                //Настраиваем бидирекциональную связь
                entity.HasOne(c => c.Category).WithMany(c => c.DailyEvents).HasForeignKey(c => c.CategoryId);
            });
        }
    }
}
