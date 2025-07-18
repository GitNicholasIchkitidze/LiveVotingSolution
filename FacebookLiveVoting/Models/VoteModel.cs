using System;

namespace FacebookLiveVoting.Models
{
	public class VoteModel
	{
		public Guid Id { get; set; } = Guid.NewGuid(); // Assigned by app
		public string FacebookUserId { get; set; } = null!;
		public string DisplayName { get; set; } = null!;
		public string Answer { get; set; } = null!;
		public DateTime? DateTimestamp { get; set; }
		public int RoundId { get; set; } = 1;
	}

	public class VoteSummaryModel
	{
		public int TotalVotes { get; set; }
		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }
		public int D { get; set; }
	}
}
