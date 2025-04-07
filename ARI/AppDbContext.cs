using ARI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARI;

public class AppDbContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
	public DbSet<AriEntity> Aris { get; init; }
}