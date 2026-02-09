using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServer.Core.context;

public class DuckingContext: DbContext
{
    public DbSet<DuckingUser> users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
}
