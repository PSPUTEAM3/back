﻿using Microsoft.EntityFrameworkCore;
using WebApplication3;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<ApplicationUser> users { get; set; }
    public DbSet<InvalidToken> InvalidTokens { get; set; }

}