using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketServer.Core.context;

namespace WebSocketServer.server.context;

public class DuckingContext : DbContext
{
    public DuckingContext(DbContextOptions<DuckingContext> options)
        : base(options)
    {
    }

    public DbSet<DuckingUser> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuration is now handled via dependency injection
        // Keep this empty or add additional configuration if needed
    }
}