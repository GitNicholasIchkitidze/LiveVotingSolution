using FacebookLiveVoting.Infrastructure;
using FacebookLiveVoting.Models;
using FacebookLiveVoting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacebookLiveVoting.Controllers
{
	public class casparCgController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly RoundService _roundService;
		private readonly VoteProcessingService _voteProcessor;
		public casparCgController(AppDbContext context, RoundService roundService, VoteProcessingService voteProcessor)
		{
			_context = context;
			_roundService = roundService;
			_voteProcessor = voteProcessor;

		}

		[HttpPost("push-to-caspar")]
		public async Task<IActionResult> PushToCaspar()
		{
			var round = _roundService.GetCurrentRound();
			var votes = _context.Votes.Where(v => v.RoundId == round).ToList();

			var summary = new VoteSummaryModel();

			summary.TotalVotes = votes.Count;
			summary.A = votes.Count(v => v.Answer == "A");
			summary.B = votes.Count(v => v.Answer == "B");
			summary.C = votes.Count(v => v.Answer == "C");
			summary.D = votes.Count(v => v.Answer == "D");
			

			var casparClient = new CasparCgClient("127.0.0.1", 5250); // Add this class
			await casparClient.SendDataToTemplateAsync("FBVotes/FBVotingTemplate", summary);

			return Ok("✅ Data pushed to CasparCG");
		}

	}
}
