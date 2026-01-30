using Habit_Tracker_Backend.Models.Enums;

namespace Habit_Tracker_Backend.Helpers
{
    public static class DateHelper
    {
        public static HabitDayOfWeek ToHabitDayOfWeek(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => HabitDayOfWeek.MON,
                DayOfWeek.Tuesday => HabitDayOfWeek.TUE,
                DayOfWeek.Wednesday => HabitDayOfWeek.WED,
                DayOfWeek.Thursday => HabitDayOfWeek.THU,
                DayOfWeek.Friday => HabitDayOfWeek.FRI,
                DayOfWeek.Saturday => HabitDayOfWeek.SAT,
                DayOfWeek.Sunday => HabitDayOfWeek.SUN,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
