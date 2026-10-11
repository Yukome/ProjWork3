using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BookStoreCursov.Models;

public partial class BookStoreCursContext : DbContext
{
    public BookStoreCursContext()
    {
    }

    public BookStoreCursContext(DbContextOptions<BookStoreCursContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderPosition> OrderPositions { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PublishingHouse> PublishingHouses { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=Anastasia\\SQLEXPRESS;Initial Catalog=BookStoreCurs;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.IdAuthor).HasName("PK__authors__71DCA0105F0E3D62");

            entity.ToTable("authors");

            entity.Property(e => e.IdAuthor).HasColumnName("ID_author");
            entity.Property(e => e.AuthorName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("author_name");
            entity.Property(e => e.AuthorPatronymic)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("author_patronymic");
            entity.Property(e => e.AuthorSurname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("author_surname");
            entity.Property(e => e.Biography)
                .HasColumnType("text")
                .HasColumnName("biography");
            entity.Property(e => e.CountryId).HasColumnName("country_ID");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.IdBook).HasName("PK__books__F0CCDBC5F688DDA2");

            entity.ToTable("books");

            entity.HasIndex(e => e.Isbn, "UQ__books__99F9D0A44C04B82F").IsUnique();

            entity.Property(e => e.IdBook).HasColumnName("ID_book");
            entity.Property(e => e.AddDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("add_date");
            entity.Property(e => e.AuthorId).HasColumnName("author_ID");
            entity.Property(e => e.BookDescription)
                .HasColumnType("text")
                .HasColumnName("book_description");
            entity.Property(e => e.CoverPath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("cover_path");
            entity.Property(e => e.GenreId).HasColumnName("genre_ID");
            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("isbn");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PublishingHouseId).HasColumnName("publishing_house_ID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.YearOfPublication).HasColumnName("year_of_publication");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.IdCart).HasName("PK__carts__70179490EAD6FAEE");

            entity.ToTable("carts");

            entity.Property(e => e.IdCart).HasColumnName("ID_cart");
            entity.Property(e => e.AddDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("add_date");
            entity.Property(e => e.BookId).HasColumnName("book_ID");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.UsersId).HasColumnName("users_ID");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.IdCountry).HasName("PK__countrie__AC9B4F66007BC843");

            entity.ToTable("countries");

            entity.HasIndex(e => e.Title, "UQ__countrie__E52A1BB39963385F").IsUnique();

            entity.Property(e => e.IdCountry).HasColumnName("ID_country");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.IdGenre).HasName("PK__genres__C8EE3E3458F7C106");

            entity.ToTable("genres");

            entity.HasIndex(e => e.Title, "UQ__genres__E52A1BB3210F58F7").IsUnique();

            entity.Property(e => e.IdGenre).HasColumnName("ID_genre");
            entity.Property(e => e.GenreDescription)
                .HasColumnType("text")
                .HasColumnName("genre_description");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("PK__notifica__6DE3AEC90ED04A80");

            entity.ToTable("notifications");

            entity.Property(e => e.IdNotification).HasColumnName("ID_notification");
            entity.Property(e => e.AddDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("add_date");
            entity.Property(e => e.NotificationText)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("notification_text");
            entity.Property(e => e.StatusN).HasColumnName("status_n");
            entity.Property(e => e.UsersId).HasColumnName("users_ID");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("PK__orders__89BFCD59A9FD126D");

            entity.ToTable("orders");

            entity.Property(e => e.IdOrder).HasColumnName("ID_order");
            entity.Property(e => e.AddDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("add_date");
            entity.Property(e => e.DeliveryAddress)
                .HasColumnType("text")
                .HasColumnName("delivery_address");
            entity.Property(e => e.OrderStatusId).HasColumnName("order_status_ID");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("payment_method");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.TotalSum)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("total_sum");
            entity.Property(e => e.UsersId).HasColumnName("users_ID");
        });

        modelBuilder.Entity<OrderPosition>(entity =>
        {
            entity.HasKey(e => e.IdOrderPosition).HasName("PK__order_po__B637A14ACE8B8A88");

            entity.ToTable("order_positions");

            entity.Property(e => e.IdOrderPosition).HasColumnName("ID_order_position");
            entity.Property(e => e.BookId).HasColumnName("book_ID");
            entity.Property(e => e.OrderId).HasColumnName("order_ID");
            entity.Property(e => e.PurchasePrice)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("purchase_price");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.IdOrderStatus).HasName("PK__order_st__ECADCA460841DF03");

            entity.ToTable("order_statuses");

            entity.HasIndex(e => e.Title, "UQ__order_st__E52A1BB32D2DFB02").IsUnique();

            entity.Property(e => e.IdOrderStatus).HasColumnName("ID_order_status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<PublishingHouse>(entity =>
        {
            entity.HasKey(e => e.IdPublishingHouse).HasName("PK__publishi__3ECC4E333AFE20AB");

            entity.ToTable("publishing_houses");

            entity.HasIndex(e => e.Title, "UQ__publishi__E52A1BB3FB980F6A").IsUnique();

            entity.Property(e => e.IdPublishingHouse).HasColumnName("ID_publishing_house");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.PublishingHouseAddress)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("publishing_house_address");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PK__reviews__4D295FF8A76D6D00");

            entity.ToTable("reviews");

            entity.Property(e => e.IdReview).HasColumnName("ID_review");
            entity.Property(e => e.AddDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("add_date");
            entity.Property(e => e.BookId).HasColumnName("book_ID");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.TextReview)
                .HasColumnType("text")
                .HasColumnName("text_review");
            entity.Property(e => e.UsersId).HasColumnName("users_ID");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK__roles__45DFFBDBA9D9C2E9");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Title, "UQ__roles__E52A1BB340B54ECA").IsUnique();

            entity.Property(e => e.IdRole).HasColumnName("ID_role");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__users__D7B4671E16DE6276");

            entity.ToTable("users");

            entity.HasIndex(e => e.UserLogin, "UQ__users__9EA1B5AF7EEABDF0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164647B62B2").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("ID_user");
            entity.Property(e => e.AvatarPath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("avatar_path");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_ID");
            entity.Property(e => e.UserLogin)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_login");
            entity.Property(e => e.UsersName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("users_name");
            entity.Property(e => e.UsersPatronymic)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("users_patronymic");
            entity.Property(e => e.UsersSurname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("users_surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
