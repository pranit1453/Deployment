//using Habit_Tracker_Backend.Services.Interfaces;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.DependencyInjection;

//namespace Habit_Tracker_Backend.Services.Implementations
//{
//    public class ReminderBackgroundService : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;

//        public ReminderBackgroundService(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            try
//            {

//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    using var scope = _scopeFactory.CreateScope();
//                    var reminderService =
//                        scope.ServiceProvider.GetRequiredService<IReminderService>();

//                    await reminderService.SendHabitRemindersAsync();

//                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//                }
//            }
//            catch (TaskCanceledException)
//            {
//                // Expected when application is shutting down
//                // DO NOTHING
//            }
//        }
//    }
//}
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderBackgroundService> _logger;

        public ReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ? Give AWS RDS time to be reachable after container start
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reminderService =
                        scope.ServiceProvider.GetRequiredService<IReminderService>();

                    await reminderService.SendHabitRemindersAsync();
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
                catch (Exception ex)
                {
                    // ? NEVER crash the host because of background task failure
                    _logger.LogError(ex, "Reminder background service failed.");
                }

                // ? Run every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
