using System.Collections.Immutable;

namespace Cinematrix.Services;

public static class ErrorCodeRegistry
{
    public static readonly ImmutableDictionary<int, string> ErrorMessageByCode = 
        new Dictionary<int, string>
        {
            [40001] = "Movie title is invalid",
            [40002] = "Movie description is invalid",
            [40003] = "Movie image url is invalid",
            [40004] = "Movie already exists with the given title",
            [40005] = "Movie not found",
            [40006] = "At least one showtime reservation is purchased",
            [40007] = "Showtime date is invalid",
            [40008] = "Showtime time is invalid",
            [40009] = "Showtime already exists",
            [40010] = "Showtime not found",
            [40011] = "Seats are not valid",
            [40012] = "Seats already reserved",
            [40401] = "Movie not found",
            [40402] = "Showtime not found",
        }.ToImmutableDictionary();
}