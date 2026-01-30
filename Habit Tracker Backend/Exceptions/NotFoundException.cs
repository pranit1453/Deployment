namespace Habit_Tracker_Backend.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
        public override int StatusCode => StatusCodes.Status404NotFound;
    }
}
