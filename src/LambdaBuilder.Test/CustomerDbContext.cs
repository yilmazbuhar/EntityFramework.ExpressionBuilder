using Microsoft.EntityFrameworkCore;

namespace LambdaBuilder
{
    public class CustomerDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("CustomerDb");
        }

        public DbSet<Person>? Customer { get; set; }
        public DbSet<Team>? Team { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasOne(c => c.Team)
                .WithMany(x => x.People)
                .HasPrincipalKey(c => c.Id);

            modelBuilder.Entity<Team>()
                .HasKey(x => x.Id);
        }
    }
}
