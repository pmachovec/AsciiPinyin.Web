namespace AsciiPinyin.Web.Server.Data;

using AsciiPinyin.Web.Shared.Models;
using Microsoft.EntityFrameworkCore;

// Represents a DB session
public class AsciiPinyinContext : DbContext
{
    public DbSet<Chachar> Chachars => Set<Chachar>();

    // Configures EF to load an Sqlite database file from the specified path.
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=Data/asciipinyin.sqlite");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Needed because of two-value primary key.
        builder.Entity<Chachar>().HasKey(chachar => new {
            chachar.TheCharacter, chachar.Piniyin
        });
    }
}
