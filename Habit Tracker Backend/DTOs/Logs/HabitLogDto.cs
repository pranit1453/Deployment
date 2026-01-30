namespace Habit_Tracker_Backend.DTOs.Logs
{
    using Habit_Tracker_Backend.Models.Enums;
    using System.Text.Json.Serialization;

    public class HabitLogDto
    {
        public long LogId { get; set; }

        public long HabitId { get; set; }

        public DateOnly LogDate { get; set; }  

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HabitLogStatus Status { get; set; }

        public string? Remarks { get; set; }
    }

}
