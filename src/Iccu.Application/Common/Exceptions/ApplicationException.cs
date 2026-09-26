namespace Iccu.Application.Common.Exceptions;

using Iccu.Domain.Common;

public sealed class ApplicationException : Exception
{
    public ApplicationException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
