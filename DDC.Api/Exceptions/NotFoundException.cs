namespace DDC.Api.Exceptions;

class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
