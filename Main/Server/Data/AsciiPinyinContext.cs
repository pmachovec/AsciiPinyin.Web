using AsciiPinyin.Web.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Data;

// Represents a DB session
public class AsciiPinyinContext : DbContext
{
    public DbSet<Chachar> Chachars => Set<Chachar>();
    public DbSet<Alternative> Alternatives => Set<Alternative>();

    // Configures EF to load an Sqlite database file from the specified path.
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=Data/asciipinyin.sqlite");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Chachar>()
            .HasKey(chachar => new
            {
                chachar.TheCharacter,
                chachar.AsciiPinyin
            });

        builder.Entity<Alternative>()
            .HasKey(alternative => new
            {
                alternative.TheCharacter,
                alternative.OriginalCharacter,
                alternative.OriginalAsciiPinyin
            });

        builder.Entity<Chachar>()
            .HasOne(chachar => chachar.RadicalChachar)
            .WithOne()
            .HasForeignKey<Chachar>(chachar => new
            {
                chachar.RadicalCharacter,
                chachar.RadicalAsciiPinyin
            })
            .HasPrincipalKey<Chachar>(radicalChachar => new
            {
                radicalChachar.TheCharacter,
                radicalChachar.AsciiPinyin
            });

        builder.Entity<Chachar>()
            .HasOne(chachar => chachar.RadicalAlternative)
            .WithOne()
            .HasForeignKey<Chachar>(chachar => new
            {
                chachar.RadicalAlternativeCharacter,
                chachar.RadicalCharacter,
                chachar.RadicalAsciiPinyin
            })
            .HasPrincipalKey<Alternative>(radicalAlternative => new
            {
                radicalAlternative.TheCharacter,
                radicalAlternative.OriginalCharacter,
                radicalAlternative.OriginalAsciiPinyin
            });
    }
}
