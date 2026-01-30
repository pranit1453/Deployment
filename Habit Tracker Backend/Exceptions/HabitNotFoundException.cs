namespace Habit_Tracker_Backend.Exceptions
{
    public class HabitNotFoundException : AppException
    {
        public HabitNotFoundException()
            : base("Habit not found", "HABIT_NOT_FOUND") { }

        public override int StatusCode => StatusCodes.Status404NotFound;
    }
}
