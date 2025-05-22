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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfoRules();

            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfoRules()
        {
            var changedEntries = this.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach(var entry in changedEntries)
            {
                var entity = (EntityBase)entry.Entity;

                if(entry.State == EntityState.Added)
                {
                    entity.Created = DateTime.UtcNow;
                }

                entity.Updated = DateTime.UtcNow;
            }
        }
    }
}
