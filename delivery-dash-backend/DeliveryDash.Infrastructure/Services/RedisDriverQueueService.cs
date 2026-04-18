using System.Text.Json;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Options;
using DeliveryDash.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DeliveryDash.Infrastructure.Services
{
    public class RedisDriverQueueService : IDriverQueueService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly RedisOptions _options;
        private readonly ILogger<RedisDriverQueueService> _logger;

        private readonly string _queueKey;
        private readonly string _driverSetKey;
        private readonly string _lockKey;

        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);

        public RedisDriverQueueService(
            IConnectionMultiplexer redis,
            IOptions<RedisOptions> options,
            ILogger<RedisDriverQueueService> logger)
        {
            _redis = redis;
            _options = options.Value;
            _database = _redis.GetDatabase(_options.DefaultDatabaseId);
            _logger = logger;

            // Use existing instance name prefix for all keys
            _queueKey = $"{_options.InstanceName}driver:queue";
            _driverSetKey = $"{_options.InstanceName}driver:queue:set";
            _lockKey = $"{_options.InstanceName}driver:queue:lock";
        }

        public async Task EnqueueDriverAsync(Guid driverId, int shiftId, CancellationToken ct = default)
        {
            var entry = new DriverQueueEntry
            {
                DriverId = driverId,
                EnqueuedAt = DateTime.UtcNow,
                ShiftId = shiftId
            };

            var json = JsonSerializer.Serialize(entry);

            var transaction = _database.CreateTransaction();
            _ = transaction.ListRightPushAsync(_queueKey, json);
            _ = transaction.SetAddAsync(_driverSetKey, driverId.ToString());

            if (await transaction.ExecuteAsync())
            {
                _logger.LogInformation("Driver {DriverId} added to queue", driverId);
            }
        }

        public async Task<DriverQueueEntry?> DequeueDriverAsync(CancellationToken ct = default)
        {
            var lockValue = Guid.NewGuid().ToString();

            if (!await AcquireLockAsync(lockValue))
            {
                _logger.LogWarning("Failed to acquire queue lock for dequeue operation");
                return null;
            }

            try
            {
                var json = await _database.ListLeftPopAsync(_queueKey);
                if (json.IsNullOrEmpty)
                {
                    return null;
                }

                var entry = JsonSerializer.Deserialize<DriverQueueEntry>(json!);
                if (entry != null)
                {
                    await _database.SetRemoveAsync(_driverSetKey, entry.DriverId.ToString());
                    _logger.LogInformation("Driver {DriverId} dequeued", entry.DriverId);
                }

                return entry;
            }
            finally
            {
                await ReleaseLockAsync(lockValue);
            }
        }

        public async Task RequeueDriverAsync(Guid driverId, int shiftId, CancellationToken ct = default)
        {
            await RemoveDriverAsync(driverId, ct);
            await EnqueueDriverAsync(driverId, shiftId, ct);

            _logger.LogInformation("Driver {DriverId} requeued to end of queue", driverId);
        }

        public async Task RemoveDriverAsync(Guid driverId, CancellationToken ct = default)
        {
            var lockValue = Guid.NewGuid().ToString();

            if (!await AcquireLockAsync(lockValue))
            {
                _logger.LogWarning("Failed to acquire lock for remove operation");
                return;
            }

            try
            {
                var entries = await _database.ListRangeAsync(_queueKey);
                var remaining = new List<RedisValue>();

                foreach (var entry in entries)
                {
                    var parsed = JsonSerializer.Deserialize<DriverQueueEntry>(entry!);
                    if (parsed?.DriverId != driverId)
                    {
                        remaining.Add(entry);
                    }
                }

                var transaction = _database.CreateTransaction();
                _ = transaction.KeyDeleteAsync(_queueKey);
                if (remaining.Count > 0)
                {
                    _ = transaction.ListRightPushAsync(_queueKey, [.. remaining]);
                }
                _ = transaction.SetRemoveAsync(_driverSetKey, driverId.ToString());

                await transaction.ExecuteAsync();
                _logger.LogInformation("Driver {DriverId} removed from queue", driverId);
            }
            finally
            {
                await ReleaseLockAsync(lockValue);
            }
        }

        public async Task<bool> IsDriverInQueueAsync(Guid driverId, CancellationToken ct = default)
        {
            return await _database.SetContainsAsync(_driverSetKey, driverId.ToString());
        }

        public async Task<int?> GetDriverPositionAsync(Guid driverId, CancellationToken ct = default)
        {
            var entries = await _database.ListRangeAsync(_queueKey);

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = JsonSerializer.Deserialize<DriverQueueEntry>(entries[i]!);
                if (entry?.DriverId == driverId)
                {
                    return i + 1;
                }
            }

            return null;
        }

        public async Task<IReadOnlyList<DriverQueueEntry>> GetQueueSnapshotAsync(CancellationToken ct = default)
        {
            var entries = await _database.ListRangeAsync(_queueKey);

            return entries
                .Select(e => JsonSerializer.Deserialize<DriverQueueEntry>(e!))
                .Where(e => e != null)
                .Cast<DriverQueueEntry>()
                .ToList();
        }

        public async Task<int> GetQueueLengthAsync(CancellationToken ct = default)
        {
            return (int)await _database.ListLengthAsync(_queueKey);
        }

        private async Task<bool> AcquireLockAsync(string lockValue)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (stopwatch.Elapsed < LockTimeout)
            {
                if (await _database.LockTakeAsync(_lockKey, lockValue, LockExpiry))
                {
                    return true;
                }

                await Task.Delay(50);
            }

            return false;
        }

        private async Task ReleaseLockAsync(string lockValue)
        {
            await _database.LockReleaseAsync(_lockKey, lockValue);
        }
    }
}