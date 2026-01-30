namespace Habit_Tracker_Backend.Configurations
{
    public class EmailOptions
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
    }
}
