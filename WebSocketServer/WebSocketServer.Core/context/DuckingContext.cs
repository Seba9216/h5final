using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServer.Core.context;

public class DuckingContext : DbContext
{
    public DuckingContext(DbContextOptions<DuckingContext> options)
        : base(options)
    {
    }

    public DbSet<DuckingUser> Users { get; set; }
    public DbSet<DuckingGame> Games { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuration is now handled via dependency injection
        // Keep this empty or add additional configuration if needed
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship between DuckingGame and DuckingUser
        modelBuilder.Entity<DuckingGame>()
            .HasMany(g => g.Players)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "GamePlayers",
                j => j.HasOne<DuckingUser>().WithMany(),
                j => j.HasOne<DuckingGame>().WithMany());
    }
}