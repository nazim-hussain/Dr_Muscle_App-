using System;
namespace DrMuscle.Entity
{
    public class AdvancedEventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Starting { get; set; }
    }
    public class EventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long Id { get; set; }
        public bool IsPast { get; set; }
        public bool IsSystemExercise { get; set; }
        public DateTime? Date { get; set; } 
    }
}
