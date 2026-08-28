using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data
{
    public class ApplicationDbContext : IdentityDbContext<User,Role,int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GameList> GameLists => Set<GameList>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Follow> Follows => Set<Follow>();
        public DbSet<UserGame> UserGames => Set<UserGame>();
        public DbSet<GameListItem> GameListItems => Set<GameListItem>();
        public DbSet<GameCategory> GameCategories => Set<GameCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Follow>().ToTable("follows");
            modelBuilder.Entity<Game>().ToTable("games");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<GameCategory>().ToTable("game_categories");
            modelBuilder.Entity<UserGame>().ToTable("user_games");
            modelBuilder.Entity<GameList>().ToTable("game_lists");
            modelBuilder.Entity<GameListItem>().ToTable("game_list_items");

            modelBuilder.Entity<Follow>()
            .HasKey(f => new { f.FollowerId, f.FollowedId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany(u => u.Followeds)
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameCategory>()
                .HasKey(gc => new { gc.GameId, gc.CategoryId });

            modelBuilder.Entity<GameListItem>()
                .HasKey(x => new { x.GameListId, x.GameId });

            modelBuilder.Entity<Game>()
                .HasIndex(g => g.RawgId)
                .IsUnique();
        }
    }
}
