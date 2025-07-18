using FacebookLiveVoting.Infrastructure;
using FacebookLiveVoting.Models;
using FacebookLiveVoting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacebookLiveVoting.Controllers
{
	[ApiController]
	[Route("api/votes")]
	public class VotesController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly RoundService _roundService;
		private readonly VoteProcessingService _voteProcessor;
		public VotesController(AppDbContext context, RoundService roundService, VoteProcessingService voteProcessor)
		{
			_context = context;
			_roundService = roundService;
			_voteProcessor = voteProcessor;

		}

		[HttpGet]
		public async Task<IActionResult> GetVotes(
			[FromQuery] string? facebookUserId,
			[FromQuery] string? answer,
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to)
		{
			var query = _context.Votes.AsQueryable();

			if (!string.IsNullOrWhiteSpace(facebookUserId))
				query = query.Where(v => v.FacebookUserId == facebookUserId);

			if (!string.IsNullOrWhiteSpace(answer))
				query = query.Where(v => v.Answer == answer);

			if (from.HasValue)
				query = query.Where(v => v.DateTimestamp >= from.Value);

			if (to.HasValue)
				query = query.Where(v => v.DateTimestamp <= to.Value);

			var results = await query
				.OrderByDescending(v => v.DateTimestamp)
				.ToListAsync();

			return Ok(results);
		}

		[HttpGet("summary")]
		public IActionResult GetVoteSummary()
		{
			var round = _roundService.GetCurrentRound();
			var votes = _context.Votes.Where(v => v.RoundId == round).ToList();
			//var votes = _context.Votes.ToList();

			var summary = new
			{
				Round = round,
				TotalVotes = votes.Count,
				A = votes.Count(v => v.Answer == "A"),
				B = votes.Count(v => v.Answer == "B"),
				C = votes.Count(v => v.Answer == "C"),
				D = votes.Count(v => v.Answer == "D")
			};

			return Ok(summary);
		}

		[HttpPost("test-vote")]
		public async Task<IActionResult> TestVote()
		{
			var _currRound = _roundService.GetCurrentRound();
			var vote = new VoteModel
			{
				FacebookUserId = Guid.NewGuid().ToString(),
				DisplayName = "Test User",
				Answer = "A",
				DateTimestamp = DateTime.UtcNow,
				RoundId = _currRound
			};

			_context.Votes.Add(vote);
			await _context.SaveChangesAsync();

			return Ok($"Test vote saved for Round. {_roundService.GetCurrentRound()}");
		}
		[HttpPost("next-round")]
		public IActionResult NextRound()
		{
			_roundService.NextRound();
			return Ok(new { message = "✅ Round advanced", currentRound = _roundService.GetCurrentRound() });
		}

		[HttpPost("set-round/{round}")]
		public IActionResult SetRound(int round)
		{
			_roundService.SetRound(round);
			return Ok(new { message = "✅ Round set manually", currentRound = _roundService.GetCurrentRound() });
		}
		[HttpDelete("reset-round")]
		public async Task<IActionResult> ResetCurrentRound()
		{
			var round = _roundService.GetCurrentRound();

			var votesToDelete = _context.Votes.Where(v => v.RoundId == round);
			_context.Votes.RemoveRange(votesToDelete);
			await _context.SaveChangesAsync();

			_voteProcessor.ClearVotesForRound(round);
			return Ok(new { message = $"✅ All votes removed for Round {round}" });
		}
	}
}
