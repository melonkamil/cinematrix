namespace Cinematrix.Data;

public class Reservation
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string ShowtimeId { get; set; }
    public int Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public Showtime Showtime { get; set; }
}