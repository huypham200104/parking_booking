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

        // Khởi tạo ConnectionMultiplexer (Singleton)
        var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        // Khởi tạo RedLock Factory
        var multiplexers = new List<RedLockMultiplexer> { connectionMultiplexer };
        var redLockFactory = RedLockFactory.Create(multiplexers);

        services.AddSingleton<IDistributedLockFactory>(redLockFactory);

        return services;
    }
}
