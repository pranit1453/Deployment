using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reminderService =
                        scope.ServiceProvider.GetRequiredService<IReminderService>();

                    await reminderService.SendHabitRemindersAsync();

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when application is shutting down
                // DO NOTHING
            }
        }
    }
}
