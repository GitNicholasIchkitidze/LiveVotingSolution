using FacebookLiveVoting.DTOs;
using System.Text.Json;

namespace FacebookLiveVoting.Clients
{
	public class FacebookClient
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _config;

		public FacebookClient(HttpClient httpClient, IConfiguration config)
		{
			_httpClient = httpClient;
			_config = config;
		}

		public async Task<List<FacebookComment>> GetLatestCommentsAsync(string? afterCursor = null)
		{
			string videoId = _config["Facebook:VideoId"]!;
			string token = _config["Facebook:AccessToken"]!;
			string url = $"https://graph.facebook.com/v19.0/{videoId}/comments?access_token={token}&order=chronological&filter=stream";

			if (!string.IsNullOrEmpty(afterCursor))
				url += $"&after={afterCursor}";

			var response = await _httpClient.GetStringAsync(url);
			var data = JsonSerializer.Deserialize<FacebookCommentResponse>(response);
			return data?.Data ?? new List<FacebookComment>();
		}
	}

}
