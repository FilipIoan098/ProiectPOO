// CinemaManagemnt.Infrastracture/Data/CinemsDbContext.cs

using CinemaManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Infrastructure.Data
{
    public class CinemaDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public CinemaDbContext(DbContextOptions<CinemaDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("MySqlConnection");

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => options.EnableRetryOnFailure()
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Genre).HasMaxLength(50);
                entity.Property(e => e.Rating).HasPrecision(3, 1);
            });

            modelBuilder.Entity<Hall>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Screening>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BasePrice).HasPrecision(10, 2);

                entity.HasOne(e => e.Movie)
                    .WithMany()
                    .HasForeignKey(e => e.MovieId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hall)
                    .WithMany()
                    .HasForeignKey(e => e.HallId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SeatNumbers).IsRequired();
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Screening)
                    .WithMany()
                    .HasForeignKey(e => e.ScreeningId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User("admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", UserRole.Administrator)
                {
                    Id = 1,
                    CreatedAt = DateTime.Now
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User("user", "BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=", UserRole.Spectator)
                {
                    Id = 2,
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
