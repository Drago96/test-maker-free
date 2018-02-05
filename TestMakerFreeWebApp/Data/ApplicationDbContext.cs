﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestMakerFreeWebApp.Data.Models;

namespace TestMakerFreeWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Constructor

        public ApplicationDbContext(DbContextOptions options) :
            base(options)
        {
        }

        #endregion Constructor

        #region Methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().HasMany(u =>
                u.Quizzes).WithOne(i => i.User);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Tokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Quiz>().ToTable("Quizzes");
            modelBuilder.Entity<Quiz>().Property(i =>
                i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Quiz>().HasOne(i => i.User).WithMany(u
                => u.Quizzes);
            modelBuilder.Entity<Quiz>().HasMany(i =>
                i.Questions).WithOne(c => c.Quiz);

            modelBuilder.Entity<Question>().ToTable("Questions");
            modelBuilder.Entity<Question>().Property(i =>
                i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Question>().HasOne(i =>
                i.Quiz).WithMany(u => u.Questions);
            modelBuilder.Entity<Question>().HasMany(i =>
                i.Answers).WithOne(c => c.Question);

            modelBuilder.Entity<Answer>().ToTable("Answers");
            modelBuilder.Entity<Answer>().Property(i =>
                i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Answer>().HasOne(i =>
                i.Question).WithMany(u => u.Answers);

            modelBuilder.Entity<Result>().ToTable("Results");
            modelBuilder.Entity<Result>().Property(i =>
                i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Result>().HasOne(i =>
                i.Quiz).WithMany(u => u.Results);

            modelBuilder.Entity<Token>().ToTable("Tokens");
            modelBuilder.Entity<Token>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Token>().HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId);
        }

        #endregion Methods

        #region Properties

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Token> Tokens { get; set; }

        #endregion Properties
    }
}