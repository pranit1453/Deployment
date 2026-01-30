namespace Habit_Tracker_Backend.Exceptions
{
    public class InvalidCategoryException : AppException
    {
        public InvalidCategoryException()
            : base("Invalid category", "CATEGORY_INVALID") { }

        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
