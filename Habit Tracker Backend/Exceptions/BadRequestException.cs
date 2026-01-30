namespace Habit_Tracker_Backend.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message) { }
        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
