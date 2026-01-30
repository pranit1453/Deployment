namespace Habit_Tracker_Backend.Exceptions
{
    public class ScheduleRequiredException : AppException
    {
        public ScheduleRequiredException()
            : base("At least one schedule day is required", "SCHEDULE_REQUIRED") { }

        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
