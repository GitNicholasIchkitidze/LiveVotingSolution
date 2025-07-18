using FacebookLiveVoting.DTOs;
using FacebookLiveVoting.Infrastructure;
using FacebookLiveVoting.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace FacebookLiveVoting.Services
{

		public class VoteProcessingService
		{
			private readonly IConnectionMultiplexer _redis;
			private readonly AppDbContext _dbContext;
			private readonly ILogger<VoteProcessingService> _logger;
			//private readonly HashSet<string> _votedUserIds;
			private readonly Dictionary<int, HashSet<string>> _votedUsersByRound = new();
			private readonly RoundService _roundService;


			public VoteProcessingService(IConnectionMultiplexer redis, RoundService roundService,
								AppDbContext dbContext, ILogger<VoteProcessingService> logger)
				{
				_redis = redis;
				_dbContext = dbContext;
				_logger = logger;
				//_votedUserIds = new HashSet<string>(); // In-memory duplicate checker
				_votedUsersByRound = new Dictionary<int, HashSet<string>>();
				_roundService = roundService;

			}

			public async Task ProcessCommentAsync(FacebookComment comment)
			{
				if (comment.From == null || string.IsNullOrEmpty(comment.From.Id) || string.IsNullOrEmpty(comment.Message))
				{
					_logger.LogWarning("❌ Invalid comment structure: {Comment}", comment);
					return;
				}
				// Normalize user ID and message
				var userId = comment.From?.Id;
				var message = comment.Message?.Trim().ToUpper();
				var roundId = _roundService.GetCurrentRound();


				if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(message))
				{
					_logger.LogWarning("❌ Comment missing user ID or Message.");
					return;
				}
				// ✅ Initialize memory tracking for round
				if (!_votedUsersByRound.ContainsKey(roundId))
					_votedUsersByRound[roundId] = new HashSet<string>();

				if (!IsValidVote(message))
				{
					_logger.LogWarning("❌ Invalid vote from {User}: {Message}", comment.From!.Name, message);
					return;
				}

				if (_votedUsersByRound[roundId].Contains(userId))
				{
					_logger.LogInformation("⚠️ Duplicate vote from {User} ignored.", comment.From!.Name);
					return;
				}

				_votedUsersByRound[roundId].Add(userId);


				// Save to SQL
				var vote = new VoteModel
				{
					FacebookUserId = userId,
					DisplayName = comment.From?.Name ?? "Unknown",
					Answer = message,
					DateTimestamp = comment.CreatedTime.Value,
					RoundId = _roundService.GetCurrentRound()
				};

			_dbContext.Votes.Add(vote);
			await _dbContext.SaveChangesAsync();
			_logger.LogInformation("✅ Vote accepted from {User}: {Answer} (Round {Round})", vote.DisplayName, vote.Answer, roundId);



			// Also enqueue in Redis
			var voteJson = JsonSerializer.Serialize(vote);
			await _redis.GetDatabase().ListRightPushAsync("votes", $"{vote.FacebookUserId}|{vote.Answer}|{vote.RoundId}");
			
			
			}

			private bool IsValidVote(string msg)
			{
				return new[] { "A", "B", "C", "D" }.Contains(msg);
			}

			public void ClearVotesForRound(int roundId)
			{
				if (_votedUsersByRound.ContainsKey(roundId))
				{
					_votedUsersByRound.Remove(roundId);
				}
			}
	}

}
