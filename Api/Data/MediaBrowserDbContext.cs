using Microsoft.EntityFrameworkCore;

public class MediaBrowserDbContext(DbContextOptions<MediaBrowserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

}