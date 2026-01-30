namespace Habit_Tracker_Backend.Exceptions
{
    public class DuplicateHabitException : AppException
    {
        public DuplicateHabitException()
            : base("Habit name already exists", "HABIT_DUPLICATE") { }

        public override int StatusCode => StatusCodes.Status409Conflict;
    }
}
