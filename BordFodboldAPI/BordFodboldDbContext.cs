using BordFodboldAPI.Objects;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore;

namespace BordFodboldAPI
{
    public class BordFodboldDbContext : DbContext
    {
        public BordFodboldDbContext(DbContextOptions<BordFodboldDbContext> options) : base(options)
        {

        }
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<User> Users => Set<User>();
    }
}
