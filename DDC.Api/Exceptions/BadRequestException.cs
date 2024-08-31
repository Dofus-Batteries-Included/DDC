namespace DDC.Api.Exceptions;

class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
