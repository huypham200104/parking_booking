using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;
using System.Net;
using RedLockNet;

namespace parking_booking_backend.Extensions;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] 
            ?? throw new InvalidOperationException("Redis connection string is missing.");

        // Connect lazily so startup and integration tests can replace Redis dependencies.
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));

        // Khởi tạo RedLock Factory
        services.AddSingleton<IDistributedLockFactory>(provider =>
        {
            var connectionMultiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            var multiplexers = new List<RedLockMultiplexer> { (ConnectionMultiplexer)connectionMultiplexer };
            return RedLockFactory.Create(multiplexers);
        });

        return services;
    }
}
