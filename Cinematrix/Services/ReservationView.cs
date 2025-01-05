namespace Cinematrix.Services;

public class ReservationView
{
    public string ReservationId { get; set; }
    public string MovieTitle { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public int TotalCost { get; set; }
    public string[] Seats { get; set; }
}