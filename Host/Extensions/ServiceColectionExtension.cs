using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Host.Extensions
{
    public static class ServiceColectionExtension
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderEaseDbContext>(config => config.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }
}
