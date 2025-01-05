namespace Cinematrix;

public class NotFoundException(int errorCode) : Exception
{
    public int ErrorCode { get; set; } = errorCode;
}