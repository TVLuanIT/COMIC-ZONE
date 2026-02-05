using System;
using System.Collections.Generic;
using COMICZONE.Models;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Data;

public partial class ComiczoneContext : DbContext
{
    public ComiczoneContext()
    {
    }

    public ComiczoneContext(DbContextOptions<ComiczoneContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ARTISTS__3214EC2763439081");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PICTURES__3214EC271DE41584");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PRODUCTS__3214EC27AC9A8092");

            entity.HasMany(d => d.Artists).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductArtist",
                    r => r.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_A__ARTIS__1B0907CE"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_A__PRODU__1A14E395"),
                    j =>
                    {
                        j.HasKey("ProductId", "ArtistId").HasName("PK__PRODUCT___C61447605FF82DE2");
                        j.ToTable("PRODUCT_ARTIST");
                        j.IndexerProperty<int>("ProductId").HasColumnName("PRODUCT_ID");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("ARTIST_ID");
                    });

            entity.HasMany(d => d.Pictures).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductPicture",
                    r => r.HasOne<Picture>().WithMany()
                        .HasForeignKey("PictureId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_P__PICTU__22AA2996"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_P__PRODU__21B6055D"),
                    j =>
                    {
                        j.HasKey("ProductId", "PictureId").HasName("PK__PRODUCT___0EF281E8F575EAB0");
                        j.ToTable("PRODUCT_PICTURE");
                        j.IndexerProperty<int>("ProductId").HasColumnName("PRODUCT_ID");
                        j.IndexerProperty<int>("PictureId").HasColumnName("PICTURE_ID");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_T__TAG_I__1ED998B2"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PRODUCT_T__PRODU__1DE57479"),
                    j =>
                    {
                        j.HasKey("ProductId", "TagId").HasName("PK__PRODUCT___58167A98A046EDB4");
                        j.ToTable("PRODUCT_TAG");
                        j.IndexerProperty<int>("ProductId").HasColumnName("PRODUCT_ID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TAG_ID");
                    });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TAGS__3214EC2799A79064");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
