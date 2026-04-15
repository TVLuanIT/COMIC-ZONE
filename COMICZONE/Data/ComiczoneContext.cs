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

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BlogCommentLike> BlogCommentLikes { get; set; }

    public virtual DbSet<BlogCommentReply> BlogCommentReplies { get; set; }

    public virtual DbSet<BlogCommentReplyLike> BlogCommentReplyLikes { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<InventoryLog> InventoryLogs { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MarketplaceAdvertisement> MarketplaceAdvertisements { get; set; }

    public virtual DbSet<MarketplaceFavorite> MarketplaceFavorites { get; set; }

    public virtual DbSet<MarketplaceMessage> MarketplaceMessages { get; set; }

    public virtual DbSet<MarketplacePost> MarketplacePosts { get; set; }

    public virtual DbSet<MarketplacePostImage> MarketplacePostImages { get; set; }

    public virtual DbSet<MarketplacePostPromotion> MarketplacePostPromotions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductReviewLike> ProductReviewLikes { get; set; }

    public virtual DbSet<ProductReviewReply> ProductReviewReplies { get; set; }

    public virtual DbSet<ProductReviewReplyLike> ProductReviewReplyLikes { get; set; }

    public virtual DbSet<ProductReviewSummary> ProductReviewSummaries { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProductView> UserProductViews { get; set; }

    public virtual DbSet<ViolationReport> ViolationReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ARTISTS__3214EC2763439081");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BLOG__3214EC2772788182");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("PENDING");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BLOG_USER");

            entity.HasMany(d => d.Categories).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogCategoryMap",
                    r => r.HasOne<BlogCategory>().WithMany()
                        .HasForeignKey("Categoryid")
                        .HasConstraintName("FK_BLOG_CATEGORY_MAP_CATEGORY"),
                    l => l.HasOne<Blog>().WithMany()
                        .HasForeignKey("Blogid")
                        .HasConstraintName("FK_BLOG_CATEGORY_MAP_BLOG"),
                    j =>
                    {
                        j.HasKey("Blogid", "Categoryid");
                        j.ToTable("BLOG_CATEGORY_MAP");
                        j.IndexerProperty<int>("Blogid").HasColumnName("BLOGID");
                        j.IndexerProperty<int>("Categoryid").HasColumnName("CATEGORYID");
                    });
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BLOGCATE__3214EC27848A46AC");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isdeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogComments).HasConstraintName("FK_BLOG_COMMENT_BLOG");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BLOG_COMMENT_USER");
        });

        modelBuilder.Entity<BlogCommentLike>(entity =>
        {
            entity.HasKey(e => new { e.Commentid, e.Userid }).HasName("PK__BLOG_COM__76496CD23D4AF486");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Islike).HasDefaultValue(true);

            entity.HasOne(d => d.Comment).WithMany(p => p.BlogCommentLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COMMENT_LIKE_COMMENT");

            entity.HasOne(d => d.User).WithMany(p => p.BlogCommentLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COMMENT_LIKE_USER");
        });

        modelBuilder.Entity<BlogCommentReply>(entity =>
        {
            entity.HasKey(e => e.Replyid).HasName("PK__BLOG_COM__4B22DB5A6F906101");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isdeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Comment).WithMany(p => p.BlogCommentReplies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BLOG_COMMENT_REPLY_COMMENT");

            entity.HasOne(d => d.Parentreply).WithMany(p => p.InverseParentreply).HasConstraintName("FK_BLOG_COMMENT_REPLY_PARENT");

            entity.HasOne(d => d.Replytouser).WithMany(p => p.BlogCommentReplyReplytousers).HasConstraintName("FK_BLOG_COMMENT_REPLY_TO_USER");

            entity.HasOne(d => d.User).WithMany(p => p.BlogCommentReplyUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BLOG_COMMENT_REPLY_USER");
        });

        modelBuilder.Entity<BlogCommentReplyLike>(entity =>
        {
            entity.HasKey(e => new { e.Replyid, e.Userid }).HasName("PK__BLOG_COM__0C9B3CA9DB75D44F");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Islike).HasDefaultValue(true);

            entity.HasOne(d => d.Reply).WithMany(p => p.BlogCommentReplyLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_REPLY_LIKE_REPLY");

            entity.HasOne(d => d.User).WithMany(p => p.BlogCommentReplyLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_REPLY_LIKE_USER");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__CART__AB01BF6F058AAECE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CART_USER");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CART_ITE__0A3C6BC4AE32D3FB");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CARTITEM_CART");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CARTITEM_PRODUCT");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("PK__CUSTOMER__61DBD788ADF0132A");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithOne(p => p.Customer).HasConstraintName("FK_CUSTOMERS_USERS");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__INVENTOR__3214EC27281A5665");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INVENTORY_PRODUCT");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__INVOICE__3214EC272A7D76C4");

            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INVOICE_ORDER");
        });

        modelBuilder.Entity<MarketplaceAdvertisement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC27AF1B9A7E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("ACTIVE");
        });

        modelBuilder.Entity<MarketplaceFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC2763EE0058");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.MarketplaceFavorites).HasConstraintName("FK_MARKETPLACE_FAVORITE_POST");

            entity.HasOne(d => d.User).WithMany(p => p.MarketplaceFavorites)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MARKETPLACE_FAVORITE_USER");
        });

        modelBuilder.Entity<MarketplaceMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC27A6B1827E");

            entity.Property(e => e.CreatedAt1).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isread).HasDefaultValue(false);

            entity.HasOne(d => d.Post).WithMany(p => p.MarketplaceMessages).HasConstraintName("FK_MARKETPLACE_MESSAGE_POST");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MarketplaceMessageReceivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MARKETPLACE_MESSAGE_RECEIVER");

            entity.HasOne(d => d.Sender).WithMany(p => p.MarketplaceMessageSenders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MARKETPLACE_MESSAGE_SENDER");
        });

        modelBuilder.Entity<MarketplacePost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC27E35B9C3C");

            entity.Property(e => e.CreatedAt1).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isdeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Seller).WithMany(p => p.MarketplacePosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MARKETPLACE_POST_USER");
        });

        modelBuilder.Entity<MarketplacePostImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC27D88D2C1F");

            entity.HasOne(d => d.Post).WithMany(p => p.MarketplacePostImages).HasConstraintName("FK_MARKETPLACE_IMAGE_POST");
        });

        modelBuilder.Entity<MarketplacePostPromotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MARKETPL__3214EC276E121914");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("ACTIVE");

            entity.HasOne(d => d.Post).WithMany(p => p.MarketplacePostPromotions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MARKETPLA__POSTI__226010D3");

            entity.HasOne(d => d.User).WithMany(p => p.MarketplacePostPromotions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MARKETPLA__USERI__2354350C");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__NOTIFICA__83A4A446073FCFEC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.NotificationCreatedByNavigations).HasConstraintName("FK__NOTIFICAT__CREAT__02925FBF");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NOTIFICAT__USER___019E3B86");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ORDER__460A9464EECAFB3A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__5B78929E");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__ORDER_IT__E15C431676182876");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_ITE__ORDER__6225902D");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_ITE__PRODU__6319B466");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ORDER_ST__3214EC27ABCEA6E0");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_STA__ORDER__66EA454A");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.OrderStatusHistories).HasConstraintName("FK__ORDER_STA__UPDAT__67DE6983");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("PK__PAYMENT__D2C4FF4636EF55F3");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Paymentstatus).HasDefaultValue("PENDING");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAYMENT_ORDER");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT___3214EC271B14E5B9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAYMENT_TRANSACTION_PAYMENT");
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
                        .HasConstraintName("FK_PRODUCT_ARTIST_ARTIST"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PRODUCT_ARTIST_PRODUCT"),
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
                        .HasConstraintName("FK_PRODUCT_PICTURE_PICTURE"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PRODUCT_PICTURE_PRODUCT"),
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
                        .HasConstraintName("FK_PRODUCT_TAG_TAG"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PRODUCT_TAG_PRODUCT"),
                    j =>
                    {
                        j.HasKey("ProductId", "TagId").HasName("PK__PRODUCT___58167A98A046EDB4");
                        j.ToTable("PRODUCT_TAG");
                        j.IndexerProperty<int>("ProductId").HasColumnName("PRODUCT_ID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TAG_ID");
                    });
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Reviewid).HasName("PK__PRODUCT___DDDCEB4A41B2EC7A");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT_R__PRODU__1AD3FDA4");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT_R__USERI__1BC821DD");
        });

        modelBuilder.Entity<ProductReviewLike>(entity =>
        {
            entity.HasKey(e => new { e.Reviewid, e.Userid }).HasName("PK_REVIEW_LIKE");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Review).WithMany(p => p.ProductReviewLikes).HasConstraintName("FK__PRODUCT_R__REVIE__1F98B2C1");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviewLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT_R__USERI__208CD6FA");
        });

        modelBuilder.Entity<ProductReviewReply>(entity =>
        {
            entity.HasKey(e => e.Replyid).HasName("PK__PRODUCT___4B22DB5A37E61D2E");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Parentreply).WithMany(p => p.InverseParentreply).HasConstraintName("FK_ProductReviewReply_Parent");

            entity.HasOne(d => d.Replytouser).WithMany(p => p.ProductReviewReplyReplytousers).HasConstraintName("FK_PRODUCT_REVIEW_REPLY_USER");

            entity.HasOne(d => d.Review).WithMany(p => p.ProductReviewReplies).HasConstraintName("FK_REPLY_REVIEW");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviewReplyUsers).HasConstraintName("FK_REPLY_USER");
        });

        modelBuilder.Entity<ProductReviewReplyLike>(entity =>
        {
            entity.HasOne(d => d.Reply).WithMany(p => p.ProductReviewReplyLikes).HasConstraintName("FK_PRODUCT_REVIEW_REPLY_LIKE_REPLY");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviewReplyLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCT_REVIEW_REPLY_LIKE_USER");
        });

        modelBuilder.Entity<ProductReviewSummary>(entity =>
        {
            entity.HasKey(e => e.Productid).HasName("PK__PRODUCT___34980AA2424C1B98");

            entity.Property(e => e.Productid).ValueGeneratedNever();
            entity.Property(e => e.Lastupdated).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductReviewSummary)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT_R__PRODU__2B0A656D");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REFUND__3214EC27FF609E12");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("PENDING");

            entity.HasOne(d => d.Payment).WithMany(p => p.Refunds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_REFUND_PAYMENT");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TAGS__3214EC2799A79064");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USERS__7B9E7F3500BD3BA0");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Isactive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValue("USER");
        });

        modelBuilder.Entity<UserProductView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USER_PRO__3214EC27F3A9CF02");

            entity.Property(e => e.ViewedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.UserProductViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_PRODUCT_VIEW_PRODUCT");

            entity.HasOne(d => d.User).WithMany(p => p.UserProductViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_PRODUCT_VIEW_USER");
        });

        modelBuilder.Entity<ViolationReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VIOLATIO__3214EC275B182AA6");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.User).WithMany(p => p.ViolationReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VIOLATIONREPORT_USER");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
