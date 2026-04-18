using Bookano.Web.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookano.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCategory> BookCategories => Set<BookCategory>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Publisher> Publishers => Set<Publisher>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BookCategory>().HasKey(x => new { x.BookId, x.CategoryId });
            builder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
            builder.Entity<Book>().HasIndex(b => b.Isbn).IsUnique().HasFilter("[Isbn] IS NOT NULL");
            base.OnModelCreating(builder);
        }
    }
}
