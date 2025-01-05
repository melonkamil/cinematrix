using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cinematrix.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    private const int GUID_LENGTH_WITH_HYPHENS = 36;
    private const int IDENTITY_USER_ID_LENGTH = 450;
    private const string DATE_PATTERN = "yyyy-MM-dd";
    private const string TIME_PATTERN = "HH:mm";

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservedSeat> ReservedSeats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS); // Assuming GUID-style ID
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.Property(m => m.ImageUrl).HasMaxLength(500);
            entity.Property(m => m.IsDeleted).HasDefaultValue(false);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(st => st.Id);
            entity.Property(st => st.Id).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(st => st.MovieId).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(st => st.Date).IsRequired().HasMaxLength(DATE_PATTERN.Length);
            entity.Property(st => st.Time).IsRequired().HasMaxLength(TIME_PATTERN.Length);
            entity.Property(st => st.IsDeleted).HasDefaultValue(false);
            entity.Property(st => st.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(st => st.Movie)
                .WithMany()
                .HasForeignKey(st => st.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(r => r.UserId).IsRequired().HasMaxLength(IDENTITY_USER_ID_LENGTH);
            entity.Property(r => r.ShowtimeId).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(r => r.Cost).IsRequired();
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");
            
            entity.HasOne(st => st.Showtime)
                .WithMany()
                .HasForeignKey(st => st.ShowtimeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReservedSeat>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.Property(rs => rs.Id).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(rs => rs.ShowtimeId).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(rs => rs.ReservationId).IsRequired().HasMaxLength(GUID_LENGTH_WITH_HYPHENS);
            entity.Property(rs => rs.SeatNumber).IsRequired().HasMaxLength(4);
            entity.Property(rs => rs.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne<Reservation>()
                .WithMany()
                .HasForeignKey(rs => rs.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}