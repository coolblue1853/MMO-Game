﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using static Server.DB.DataModel;

namespace Server.DB
{
    internal class AppDbContext : DbContext
    {
        public DbSet<AccountDb> Accounts {get; set;}
        public DbSet<PlayerDb> Players {get; set;}
        static readonly ILoggerFactory _logger = LoggerFactory.Create(
            builder => { builder.AddConsole(); });

        string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameDB;";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseLoggerFactory(_logger)
                .UseSqlServer(ConfigManager.Config == null ? _connectionString : ConfigManager.Config.connectionString );
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AccountDb>()
                .HasIndex(a => a.AccountName)
                .IsUnique();

            builder.Entity<PlayerDb>()
                .HasIndex(a => a.PlayerName)
                .IsUnique();
        }

    }
}
