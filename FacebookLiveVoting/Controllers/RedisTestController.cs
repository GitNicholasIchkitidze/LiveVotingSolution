using FacebookLiveVoting.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacebookLiveVoting.Controllers
{
	[ApiController]
	[Route("api/test/redis")]
	public class RedisTestController : ControllerBase
	{
		private readonly RedisQueueService _queue;

		public RedisTestController(RedisQueueService queue)
		{
			_queue = queue;
		}

		[HttpPost("enqueue")]
		public async Task<IActionResult> Enqueue()
		{
			var testVote = new { Username = "testuser", Answer = "A", Timestamp = DateTime.UtcNow };
			await _queue.EnqueueVoteAsync(testVote);
			return Ok("Vote enqueued.");
		}

		[HttpGet("dequeue")]
		public async Task<IActionResult> Dequeue()
		{
			var vote = await _queue.DequeueVoteAsync();
			if (vote == null) return NotFound("No vote in queue.");
			return Ok(vote);
		}
	}
}
