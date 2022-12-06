using FileStorageAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;

namespace FileStorageAPI.Data.Contexts
{
    public class FileStorageAPIDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sqlite database
            if (!options.IsConfigured)
            {
                options.UseSqlite("DataSource=LocalDatabase.db");
            }
        }

        public FileStorageAPIDBContext(DbContextOptions<FileStorageAPIDBContext> options) : base(options)
        {
        }

        public DbSet<CustomerFile> CustomerFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerFile>(entity =>
            {
                entity.Property(x => x.Data).HasColumnType("blob");
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
