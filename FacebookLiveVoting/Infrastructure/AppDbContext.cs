using FacebookLiveVoting.Models;
using Microsoft.EntityFrameworkCore;

namespace FacebookLiveVoting.Infrastructure
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<VoteModel> Votes { get; set; } = null!;
	}
}
