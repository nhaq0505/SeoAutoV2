namespace SeoAuto.BuildingBlocks.Exceptions;

public class CustomException : Exception
{
    public int StatusCode { get; }

    public CustomException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }

}

public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message, 404){}
}

public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message, 400){}
}

public class UnAuthorizedException : CustomException
{
    public UnAuthorizedException(string message) : base(message, 401){}
}