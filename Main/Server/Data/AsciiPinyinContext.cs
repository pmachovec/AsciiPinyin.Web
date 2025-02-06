using AsciiPinyin.Web.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Data;

// Represents a DB session
public class AsciiPinyinContext(DbContextOptions<AsciiPinyinContext> options) : DbContext(options)
{
    public virtual DbSet<Chachar> Chachars => Set<Chachar>();

    public virtual DbSet<Alternative> Alternatives => Set<Alternative>();

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
