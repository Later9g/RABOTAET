using Microsoft.EntityFrameworkCore;
using WikiLive.Api.Domain;

namespace WikiLive.Api.Infrastructure;

public class WikiDbContext : DbContext
{
    public WikiDbContext(DbContextOptions<WikiDbContext> options) : base(options) { }

    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageRevision> PageRevisions => Set<PageRevision>();
    public DbSet<PageLink> PageLinks => Set<PageLink>();
    public DbSet<PageComment> PageComments => Set<PageComment>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Page>().HasKey(x => x.Id);
        modelBuilder.Entity<PageRevision>().HasKey(x => x.Id);
        modelBuilder.Entity<PageLink>().HasKey(x => x.Id);

        modelBuilder.Entity<Page>().HasIndex(x => new { x.SpaceId, x.Slug }).IsUnique();
        modelBuilder.Entity<PageRevision>().HasIndex(x => new { x.PageId, x.CreatedAtUtc });
        modelBuilder.Entity<PageLink>().HasIndex(x => new { x.FromPageId, x.ToPageId });

        modelBuilder.Entity<PageComment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.PageId);
            entity.HasIndex(x => x.ParentCommentId);

            entity.Property(x => x.AnchorJson).IsRequired();
            entity.Property(x => x.Text).IsRequired();
            entity.Property(x => x.AuthorId).IsRequired();
        });
    }
}
