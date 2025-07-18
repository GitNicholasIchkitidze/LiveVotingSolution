namespace FacebookLiveVoting.DTOs
{
	public class FacebookComment
	{
		public string? Id { get; set; }
		public FromUser? From { get; set; }
		public string? Message { get; set; }
		public DateTime? CreatedTime { get; set; }
	}

	public class FromUser
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
	}
}
