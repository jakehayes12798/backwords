namespace Backwords.Api;

using Backwords.Api.Models;
using Microsoft.EntityFrameworkCore;

public class BackwordsDbContext : DbContext
{
    public DbSet<Lexeme> Lexemes { get; set; }
    public DbSet<Derivation> Derivations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite("Data Source=backwords.db");
}
