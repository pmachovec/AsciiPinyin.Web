using AsciiPinyin.Web.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Data;

// Represents a DB session
public class AsciiPinyinContext : DbContext
{
    public DbSet<Chachar> Chachars => Set<Chachar>();

    public DbSet<Alternative> Alternatives => Set<Alternative>();

    // Configures EF to load an Sqlite database file from the specified path.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => _ = optionsBuilder.UseSqlite("Data Source=Data/asciipinyin.sqlite");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder
            .Entity<Chachar>()
            .HasKey(chachar =>
                new
                {
                    chachar.TheCharacter,
                    chachar.Pinyin,
                    chachar.Tone
                }
            );

        _ = modelBuilder
            .Entity<Alternative>()
            .HasKey(alternative =>
                new
                {
                    alternative.TheCharacter,
                    alternative.OriginalCharacter,
                    alternative.OriginalPinyin,
                    alternative.OriginalTone
                }
            );
    }
}
