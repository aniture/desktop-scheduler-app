using Microsoft.EntityFrameworkCore;
using SchedulerApp.Models;

namespace SchedulerApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ScheduleEvent> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=scheduler.db");
        }
    }
}