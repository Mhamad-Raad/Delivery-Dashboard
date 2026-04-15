using MalDash.Application.Abstracts.IService;
using MalDash.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalDash.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly RedisOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            IOptions<RedisOptions> options,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _options = options.Value;
            _database = _redis.GetDatabase(_options.DefaultDatabaseId);
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false,
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 128
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                var compressedValue = await _database.StringGetAsync(redisKey);

                if (!compressedValue.HasValue)
                {
                    _logger.LogInformation("Cache MISS for key: {Key}", key);
                    return default;
                }

                var decompressedValue = Decompress(compressedValue!);
                _logger.LogInformation("Cache HIT for key: {Key}, Compressed: {CompressedKB:F2} KB, Decompressed: {DecompressedKB:F2} KB",
                    key, compressedValue.Length() / 1024.0, decompressedValue.Length / 1024.0);

                return JsonSerializer.Deserialize<T>(decompressedValue, _jsonOptions);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while getting key: {Key}", key);
                return default;
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while getting key: {Key}", key);
                return default;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting cache key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var compressedValue = Compress(serializedValue);

                _logger.LogInformation("Cache SET for key: {Key}, Original: {OriginalKB:F2} KB, Compressed: {CompressedKB:F2} KB, Ratio: {Ratio:F1}%, Expiration: {Expiration}",
                    key, serializedValue.Length / 1024.0, compressedValue.Length / 1024.0,
                    (compressedValue.Length * 100.0 / serializedValue.Length),
                    expiration?.ToString() ?? "None");

                if (expiration.HasValue)
                {
                    await _database.StringSetAsync(redisKey, compressedValue, expiration.Value);
                }
                else
                {
                    await _database.StringSetAsync(redisKey, compressedValue);
                }
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while setting key: {Key}", key);
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while setting key: {Key}", key);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON serialization error for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                var removed = await _database.KeyDeleteAsync(redisKey);
                _logger.LogInformation("Cache REMOVE for key: {Key}, Success: {Success}", key, removed);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while removing key: {Key}", key);
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while removing key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while removing cache key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                return await _database.KeyExistsAsync(redisKey);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while checking key existence: {Key}", key);
                return false;
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while checking key existence: {Key}", key);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking cache key existence: {Key}", key);
                return false;
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var redisPattern = GetKey(pattern);
                var keysToDelete = new List<RedisKey>();

                // Use SCAN instead of KEYS (non-blocking, production-safe)
                await foreach (var key in server.KeysAsync(
                    database: _options.DefaultDatabaseId,
                    pattern: redisPattern,
                    pageSize: 250))
                {
                    keysToDelete.Add(key);

                    // Delete in batches of 1000 to avoid memory buildup
                    if (keysToDelete.Count >= 1000)
                    {
                        await _database.KeyDeleteAsync(keysToDelete.ToArray());
                        _logger.LogInformation("Cache REMOVE batch: {Pattern}, Count: {Count}", pattern, keysToDelete.Count);
                        keysToDelete.Clear();
                    }
                }

                // Delete remaining keys
                if (keysToDelete.Count > 0)
                {
                    await _database.KeyDeleteAsync(keysToDelete.ToArray());
                    _logger.LogInformation("Cache REMOVE final batch: {Pattern}, Count: {Count}", pattern, keysToDelete.Count);
                }
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while removing keys by pattern: {Pattern}", pattern);
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while removing keys by pattern: {Pattern}", pattern);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "No Redis endpoints available for pattern removal: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while removing cache keys by pattern: {Pattern}", pattern);
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                var result = await _database.StringIncrementAsync(redisKey, value);

                if (expiration.HasValue)
                {
                    await _database.KeyExpireAsync(redisKey, expiration.Value);
                }

                return result;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while incrementing key: {Key}", key);
                return 0;
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while incrementing key: {Key}", key);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while incrementing cache key: {Key}", key);
                return 0;
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetKey(key);
                return await _database.StringDecrementAsync(redisKey, value);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while decrementing key: {Key}", key);
                return 0;
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogError(ex, "Redis timeout while decrementing key: {Key}", key);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while decrementing cache key: {Key}", key);
                return 0;
            }
        }

        private string GetKey(string key)
        {
            return $"{_options.InstanceName}{key}";
        }

        private static byte[] Compress(string data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                gzip.Write(bytes, 0, bytes.Length);
            }
            return output.ToArray();
        }

        private static string Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return System.Text.Encoding.UTF8.GetString(output.ToArray());
        }
    }
}