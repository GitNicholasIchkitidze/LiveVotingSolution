using FacebookLiveVoting.BackgroungServices;
using FacebookLiveVoting.Clients;
using FacebookLiveVoting.Infrastructure;
using FacebookLiveVoting.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

namespace FacebookLiveVoting
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Setup Serilog logging from configuration
			builder.Host.UseSerilog((context, services, configuration) =>
			{
				configuration.ReadFrom.Configuration(context.Configuration);
			});

			// Add services to the container.
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// Add Redis
			var redisHost = builder.Configuration.GetValue<string>("Redis:Host");
			var redisPort = builder.Configuration.GetValue<int>("Redis:Port");

			var redisConfig = new ConfigurationOptions
			{
				EndPoints = { $"{redisHost}:{redisPort}" },
				AbortOnConnectFail = false
			};

			var redis = ConnectionMultiplexer.Connect(redisConfig);
			builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

			builder.Services.AddSingleton<RedisQueueService>();

			builder.Services.AddScoped<VoteProcessingService>();
			builder.Services.AddHttpClient<FacebookClient>();
			builder.Services.AddHostedService<FacebookCommentPoller>();

			builder.Services.AddSingleton<RoundService>();


			// Add PostgreSQL EF Core
			//builder.Services.AddDbContext<AppDbContext>(options =>
			//{
			//	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
			//});

			builder.Services.AddDbContext<AppDbContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseSerilogRequestLogging(); // Serilog HTTP logs

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}
