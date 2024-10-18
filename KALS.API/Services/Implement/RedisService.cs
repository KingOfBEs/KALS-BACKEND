using KALS.API.Services.Interface;
using StackExchange.Redis;

namespace KALS.API.Services.Implement;

public class RedisService: IRedisService
{
    private readonly IDatabase _db;
    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }
    public async Task<string> GetStringAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<bool> RemoveKeyAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }
}