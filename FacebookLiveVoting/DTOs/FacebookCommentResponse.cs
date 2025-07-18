using System.Collections.Generic;

namespace FacebookLiveVoting.DTOs
{
	public class FacebookCommentResponse
	{
		public List<FacebookComment> Data { get; set; }
		public PagingInfo Paging { get; set; }
	}

	public class PagingInfo
	{
		public CursorInfo Cursors { get; set; }
	}

	public class CursorInfo
	{
		public string Before { get; set; }
		public string After { get; set; }
	}
}
