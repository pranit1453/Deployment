namespace Habit_Tracker_Backend.Exceptions
{
    public class InvalidScheduleException : AppException
    {
        public InvalidScheduleException(string day)
            : base($"Invalid schedule day: {day}", "SCHEDULE_INVALID") { }

        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
