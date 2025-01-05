namespace Cinematrix.Data;

public class Showtime
{
    public string Id { get; set; }
    public string MovieId { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Movie Movie { get; set; }
}