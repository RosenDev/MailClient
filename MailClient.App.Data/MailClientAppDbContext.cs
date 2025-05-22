using MailClient.App.Domain;
using Microsoft.EntityFrameworkCore;

namespace MailClient.App.Data
{
    public class MailClientAppDbContext : DbContext
    {
        public MailClientAppDbContext(DbContextOptions<MailClientAppDbContext> options) : base(options)
        {
        }

        public DbSet<ServerCredential> ServerCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerCredential>()
                .HasKey(x => x.Id);
        }
    }
}
