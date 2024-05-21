using OnlineCoursePlatform.Data.DbContext;

namespace OnlineCoursePlatform.Services.BackgroundServices
{
    public class ResetViewAndSalesService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ResetViewAndSalesService> _logger;

        public ResetViewAndSalesService(IServiceProvider serviceProvider, ILogger<ResetViewAndSalesService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<OnlineCoursePlatformDbContext>();
                    var now = DateTime.UtcNow;

                    if (now.DayOfWeek == DayOfWeek.Monday) // Reset weekly views and sales every Monday
                    {
                        foreach (var course in context.Courses)
                        {
                            course.WeeklyViews = 0;
                        }
                        _logger.LogInformation("Weekly views and sales have been reset.");
                    }

                    if (now.Day == 1) // Reset monthly views and sales on the first day of every month
                    {
                        foreach (var course in context.Courses)
                        {
                            course.MonthlyViews = 0;
                            course.MonthlySales = 0;
                        }
                        _logger.LogInformation("Monthly views and sales have been reset.");
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Check every 24 hours
            }
        }
    }
}