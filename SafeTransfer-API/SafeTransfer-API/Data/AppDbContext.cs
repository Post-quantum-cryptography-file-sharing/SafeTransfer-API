using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Models;

namespace SafeTransfer_API.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<UploadBatch> UploadBatches => Set<UploadBatch>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<RecipientUser> RecipientUsers => Set<RecipientUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UploadBatch>(b =>
        {
            b.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StoredFile>(f =>
        {
            f.HasOne(x => x.Batch).WithMany(b => b.Files).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
            f.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.NoAction);
            f.HasIndex(x => x.ExpiresAt);
        });

        builder.Entity<RecipientUser>(t =>
        {
            t.HasOne(x => x.Batch).WithMany(b => b.RecipientUsers).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
            t.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            t.HasIndex(x => new { x.BatchId, x.UserId }).IsUnique();
        });
    }
}