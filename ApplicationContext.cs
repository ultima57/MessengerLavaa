using Microsoft.EntityFrameworkCore;

namespace MessengerSignalR {
    public class ApplicationContext : DbContext {
        public DbSet<UserDb> Users { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;

        public ApplicationContext() {

            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=Ultima57");
        }
    }
}
