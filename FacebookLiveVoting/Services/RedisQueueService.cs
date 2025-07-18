using StackExchange.Redis;
using System.Text.Json;

namespace FacebookLiveVoting.Services
{
	public class RedisQueueService
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly IDatabase _db;
		private const string QueueKey = "vote-queue"; // Redis list name

		public RedisQueueService(IConnectionMultiplexer redis)
		{
			_redis = redis;
			_db = _redis.GetDatabase();
		}

		public async Task EnqueueVoteAsync(object vote)
		{
			var json = JsonSerializer.Serialize(vote);
			await _db.ListRightPushAsync(QueueKey, json);
		}

		public async Task<object?> DequeueVoteAsync()
		{
			var json = await _db.ListLeftPopAsync(QueueKey);
			if (json.IsNullOrEmpty) return null;
			return JsonSerializer.Deserialize<object>(json!);
		}
	}
}
