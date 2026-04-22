using Bookano.Web.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookano.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<BookCategory> BookCategories => Set<BookCategory>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Publisher> Publishers => Set<Publisher>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence("SerialNumber", schema: "Shared").StartsAt(100001);

            builder
                .Entity<BookCopy>()
                .Property(bc => bc.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR Shared.SerialNumber");

            builder.Entity<BookCategory>().HasKey(x => new { x.BookId, x.CategoryId });
            builder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
            //builder.Entity<BookCopy>().HasKey(x => new { x.BookId, x.Id });
            builder
                .Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique()
                .HasFilter("[Isbn] IS NOT NULL");
            base.OnModelCreating(builder);
        }
    }
}
