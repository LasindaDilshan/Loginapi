using Loginapi.Models;
using Microsoft.EntityFrameworkCore;

namespace Loginapi.Data
{
    public class ApplcationDbContext : DbContext
    {
        public ApplcationDbContext(DbContextOptions<ApplcationDbContext> options) : base(options)
        {

        }

        public DbSet<User> users { get; set; } = default!;
        public DbSet<UserToken> UserToken { get; set; } = default!;

        public DbSet<ResetToken> ResetToken { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity => { entity.HasIndex(e => e.Email).IsUnique(); });
            modelBuilder.Entity<ResetToken>(entity => { entity.HasIndex(e => e.Token).IsUnique(); });
        }
    }
}
