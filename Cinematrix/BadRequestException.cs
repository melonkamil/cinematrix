namespace Cinematrix;

public class BadRequestException(int errorCode) : Exception
{
    public int ErrorCode { get; set; } = errorCode;
}