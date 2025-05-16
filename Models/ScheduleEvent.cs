using System;

namespace SchedulerApp.Models
{
    public class ScheduleEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}