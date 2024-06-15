using Microsoft.Extensions.Caching.Distributed;

namespace RockPaperScissors.WebApi.Entities.Constants;

public static class CacheOptions
{
    public const string GamePrefix = "game-";
    
    public static readonly DistributedCacheEntryOptions ExpirationTime = new()
        { SlidingExpiration = TimeSpan.FromMinutes(2) };
}