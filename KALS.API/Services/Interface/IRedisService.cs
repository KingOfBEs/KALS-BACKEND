namespace KALS.API.Services.Interface;

public interface IRedisService
{
    Task<string> GetStringAsync(string key);
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
    Task<bool> RemoveKeyAsync(string key);
}