// snipet: ef-dbcontext

using Microsoft.EntityFrameworkCore;
using UserApi.Models;


namespace UserApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     // lấy chuỗi kết nối từ appsettings.json
        //     string? connectionString = new ConfigurationBuilder()
        //         .SetBasePath(Directory.GetCurrentDirectory())
        //         .AddJsonFile("appsettings.json")
        //         .Build()
        //         .GetConnectionString("UserApiConnection");
        //     optionsBuilder.UseSqlServer(connectionString);
        // } 
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cập nhật trường thuộc tính Email không được trùng nhau
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            UserSeedData.Seed(modelBuilder);
        }
    }
}