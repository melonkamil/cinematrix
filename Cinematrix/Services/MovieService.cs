using System.Text.RegularExpressions;
using Cinematrix.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinematrix.Services;

public interface IMovieService
{
    int GetTicketPricePLN();

    Task<Movie> FindMovie(string movieId, CancellationToken ct = default);

    Task<List<ReservationView>> GetUserActiveReservations(string userId, CancellationToken ct = default);

    Task<List<string>> GetReservedSeats(string showtimeId, CancellationToken ct = default);

    Task<List<Movie>> GetMovies(CancellationToken ct = default);

    Task<List<Showtime>> GetShowtimesByDate(string date, CancellationToken ct = default);

    Task<string> AddMovie(string title, string description,
        string imageUrl, CancellationToken ct = default);

    Task DeleteMovieAndShowtimes(string movieId,
        CancellationToken ct = default);

    Task<string> AddShowtime(string movieId, string date, string time,
        CancellationToken ct = default);

    Task<Showtime> FindShowtime(string movieId, DateTime dateTime,
        CancellationToken ct = default);

    Task<string> MakeReservation(string showtimeId, string[] seats,
        string userId, CancellationToken ct = default);
}

public class MovieService : IMovieService
{
    private const int TICKET_PRICE_PLN = 2700;
    private const string SEAT_NUMBER_PATTERN = @"^[A-Z]\d{1,2}$";
    private const string SHOWTIME_DATE_PATTERN = @"^\d{4}-\d{2}-\d{2}$";
    private const string SHOWTIME_TIME_PATTERN = "^(0[0-9]|1[0-9]|2[0-3]):([0-5][0-9])$";

    private readonly ApplicationDbContext _dbContext;

    public MovieService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Movie> FindMovie(string movieId, CancellationToken ct = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == movieId, ct);
        return movie ?? throw new NotFoundException(40401);
    }
    
    public async Task<List<ReservationView>> GetUserActiveReservations(string userId, CancellationToken ct = default)
    {
        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.Showtime)
            .ThenInclude(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var activeReservations = reservations
            .Where(x => DateTime.Parse(x.Showtime.Date).Date >= DateTime.Now.Date)
            .ToList();

        var activeReservationIds = activeReservations.Select(x => x.Id).ToList();

        var reservedSeats = await _dbContext.ReservedSeats
            .Where(x => activeReservationIds.Contains(x.ReservationId))
            .ToListAsync(ct);

        var reservedSeatsByReservationId = reservedSeats.GroupBy(x => x.ReservationId)
            .ToDictionary(x => x.Key, x => x.Select(x => x.SeatNumber).ToArray());

        return activeReservations.Select(x => new ReservationView
        {
            ReservationId = x.Id,
            MovieTitle = x.Showtime.Movie.Title,
            Date = x.Showtime.Date,
            Time = x.Showtime.Time,
            TotalCost = x.Cost,
            Seats = reservedSeatsByReservationId.TryGetValue(x.Id, out var seats) ? seats : [],
        }).ToList();
    }

    public int GetTicketPricePLN()
        => TICKET_PRICE_PLN;

    public async Task<List<string>> GetReservedSeats(string showtimeId, CancellationToken ct = default)
    {
        var showtimeExists = await _dbContext.Showtimes
            .AnyAsync(x => x.Id == showtimeId && x.IsDeleted == false, ct);

        if (!showtimeExists)
            return [];

        var reservedSeatNumbers = await _dbContext.ReservedSeats
            .Where(x => x.ShowtimeId == showtimeId)
            .Select(x => x.SeatNumber)
            .ToListAsync(ct);

        reservedSeatNumbers = reservedSeatNumbers.Select(x => x.ToUpper())
            .OrderBy(x => x)
            .ToList();

        return reservedSeatNumbers;
    }

    public async Task<List<Movie>> GetMovies(CancellationToken ct = default)
    {
        var movies = await _dbContext.Movies
            .Where(x => x.IsDeleted == false)
            .OrderBy(x => x.Title)
            .ToListAsync(ct);

        return movies;
    }

    public async Task<List<Showtime>> GetShowtimesByDate(string date, CancellationToken ct = default)
    {
        var showtimes = await _dbContext.Showtimes
            .Include(x => x.Movie)
            .Where(x => x.Date == date && x.Movie.IsDeleted == false)
            .ToListAsync(ct);

        return showtimes;
    }

    public async Task<string> AddMovie(string title, string description,
        string imageUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BadRequestException(40001);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new BadRequestException(40002);
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new BadRequestException(40003);
        }

        var movie = await _dbContext.Movies
            .FirstOrDefaultAsync(x => x.Title.ToLower() == title.ToLower() && x.IsDeleted == false, ct);

        if (movie != null)
        {
            throw new BadRequestException(40004);
        }

        movie = new Movie
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            IsDeleted = false,
            CreatedAt = DateTime.Now
        };

        _dbContext.Add(movie);

        await _dbContext.SaveChangesAsync(ct);
        return movie.Id;
    }

    public async Task DeleteMovieAndShowtimes(string movieId,
        CancellationToken ct = default)
    {
        var movie = await _dbContext.Movies
            .FirstOrDefaultAsync(x => x.Id == movieId && x.IsDeleted == false, ct);

        if (movie is null)
        {
            throw new BadRequestException(40005);
        }

        var showtimes = await _dbContext.Showtimes
            .Where(x => x.MovieId == movieId && x.IsDeleted == false)
            .ToListAsync(ct);

        var showtimeIds = showtimes.Select(x => x.Id).ToList();

        var anyReservedSeat = await _dbContext.ReservedSeats
            .Where(x => showtimeIds.Contains(x.ShowtimeId))
            .AnyAsync(ct);

        if (anyReservedSeat)
        {
            throw new BadRequestException(40006);
        }

        movie.IsDeleted = true;

        foreach (var showtime in showtimes)
        {
            showtime.IsDeleted = true;
        }

        _dbContext.Update(movie);
        _dbContext.UpdateRange(showtimes);

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<string> AddShowtime(string movieId, string date, string time,
        CancellationToken ct = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == movieId && x.IsDeleted == false, ct);

        if (movie is null)
        {
            throw new BadRequestException(40005);
        }

        if (!Regex.IsMatch(date, SHOWTIME_DATE_PATTERN))
        {
            throw new BadRequestException(40007);
        }

        if (!Regex.IsMatch(time, SHOWTIME_TIME_PATTERN))
        {
            throw new BadRequestException(40008);
        }

        var showtime = await _dbContext.Showtimes
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.Date == date && x.Time == time, ct);

        if (showtime != null)
        {
            throw new BadRequestException(40009);
        }

        showtime = new Showtime
        {
            Id = Guid.NewGuid().ToString(),
            MovieId = movieId,
            Date = date,
            Time = time,
            CreatedAt = DateTime.Now
        };

        _dbContext.Add(showtime);

        await _dbContext.SaveChangesAsync(ct);
        return showtime.Id;
    }

    public async Task<Showtime> FindShowtime(string movieId, DateTime dateTime,
        CancellationToken ct = default)
    {
        var formattedDate = dateTime.ToString("yyyy-MM-dd");
        var formattedTime = dateTime.ToString("HH:mm");

        var showtime = await _dbContext.Showtimes
            .Include(x => x.Movie)
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId && x.Date == formattedDate && x.Time == formattedTime &&
                     x.Movie.IsDeleted == false, ct);

        return showtime ?? throw new NotFoundException(40402);
    }

    public async Task<string> MakeReservation(string showtimeId, string[] seats,
        string userId, CancellationToken ct = default)
    {
        var showtimeExists = await _dbContext.Showtimes
            .AnyAsync(x => x.Id == showtimeId && x.IsDeleted == false, cancellationToken: ct);

        if (!showtimeExists)
        {
            throw new BadRequestException(40010);
        }

        var sanitizedSeats = seats.Select(x => x.ToUpper())
            .Where(seat => Regex.IsMatch(seat, SEAT_NUMBER_PATTERN))
            .ToArray();

        if (seats.Length == 0)
        {
            throw new BadRequestException(40011);
        }

        var cost = sanitizedSeats.Length * TICKET_PRICE_PLN;

        var reservedShowtimeSeats = await _dbContext.ReservedSeats
            .Where(x => x.ShowtimeId == showtimeId)
            .ToListAsync(ct);

        var alreadyReservedSeats = reservedShowtimeSeats.Where(x => sanitizedSeats.Contains(x.SeatNumber)).ToList();
        if (alreadyReservedSeats.Count > 0)
        {
            throw new BadRequestException(40012);
        }

        var createdAt = DateTime.Now;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid().ToString(),
            ShowtimeId = showtimeId,
            UserId = userId,
            Cost = cost,
            CreatedAt = createdAt
        };

        var reservedSeats = sanitizedSeats.Select(seat => new ReservedSeat
        {
            Id = Guid.NewGuid().ToString(),
            ShowtimeId = showtimeId,
            ReservationId = reservation.Id,
            SeatNumber = seat,
            CreatedAt = createdAt
        }).ToList();

        _dbContext.Add(reservation);
        _dbContext.AddRange(reservedSeats);

        await _dbContext.SaveChangesAsync(ct);
        return reservation.Id;
    }
}