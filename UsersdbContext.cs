using Microsoft.EntityFrameworkCore;

namespace MessengerSignalR;

public partial class UsersdbContext : DbContext {
    public UsersdbContext() {
    }

    public UsersdbContext(DbContextOptions<UsersdbContext> options)
        : base(options) {
    }

    public virtual DbSet<UserDb> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=usersdb1;Username=postgres;Password=Ultima57");

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
