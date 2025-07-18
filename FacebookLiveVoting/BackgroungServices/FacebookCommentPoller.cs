using FacebookLiveVoting.Clients;
using FacebookLiveVoting.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FacebookLiveVoting.BackgroungServices
{
	public class FacebookCommentPoller : BackgroundService
	{
		private readonly FacebookClient _facebookClient;
		private readonly ILogger<FacebookCommentPoller> _logger;
		//private readonly VoteProcessingService _voteProcessor;
		private readonly IConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;

		public FacebookCommentPoller(FacebookClient facebookClient, ILogger<FacebookCommentPoller> logger, IServiceProvider serviceProvider,
		IConfiguration configuration)
		{
			_facebookClient = facebookClient;
			_logger = logger;
			_serviceProvider = serviceProvider;
			_configuration = configuration;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var interval = _configuration.GetValue<int>("Facebook:PollingIntervalSeconds", 5);
			string? lastCursor = null;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					//var comments = await _facebookClient.GetLatestCommentsAsync(lastCursor);
					//foreach (var comment in comments)
					//{
					//	await _voteProcessor.ProcessCommentAsync(comment);
					//}

					using (var scope = _serviceProvider.CreateScope())
					{
						var voteProcessor = scope.ServiceProvider.GetRequiredService<VoteProcessingService>();

						var comments = await _facebookClient.GetLatestCommentsAsync(lastCursor);
						foreach (var comment in comments)
						{
							await voteProcessor.ProcessCommentAsync(comment);
						}
					}

					//lastCursor = comments.Paging?.Cursors?.After;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error polling Facebook comments.");
				}

				await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
			}
		}
	}

}
