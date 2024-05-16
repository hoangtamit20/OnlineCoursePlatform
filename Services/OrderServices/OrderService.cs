using OnlineCoursePlatform.Data.DbContext;

namespace OnlineCoursePlatform.Services.OrderServices
{
    public class OrderService : IOrderService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OnlineCoursePlatformDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        
    }
}