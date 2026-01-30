namespace Habit_Tracker_Backend.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string message, string? errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public abstract int StatusCode { get; }

        public string? ErrorCode { get; }
    }
}
