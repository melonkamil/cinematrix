namespace Cinematrix.Data;

public class ReservedSeat
{
    public string Id { get; set; }
    public string ShowtimeId { get; set; }
    public string ReservationId { get; set; }
    public string SeatNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}