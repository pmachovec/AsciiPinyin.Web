namespace AsciiPinyin.Web.Data;

using Microsoft.EntityFrameworkCore;
using Models;

// Represents a DB session
public class AsciiPinyinContext : DbContext
{
    public DbSet<Chachar> Chachars => Set<Chachar>();
    private string DbPath { get; }

    /*
     * IWebHostEnvironment is a provider of information about the web hosting environment an application is running in.
     * It's another service that we get it automatically from .NET in the constructor (dependency injection).
     */
    public AsciiPinyinContext(IWebHostEnvironment webHostEnvironment)
    {
        var dbFolderPath = Path.Combine( // Path as string creator
            webHostEnvironment.WebRootPath, // Path to the 'wwwroot' folder
            "database");

        DbPath = Path.Join(dbFolderPath, "asciipinyin.sqlite");
    }

    // Configures EF to load an Sqlite database file from the specified path.
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Needed because of two-value primary key.
        builder.Entity<Chachar>().HasKey(chachar => new {
            chachar.TheCharacter, chachar.Piniyin
        });
    }
}
