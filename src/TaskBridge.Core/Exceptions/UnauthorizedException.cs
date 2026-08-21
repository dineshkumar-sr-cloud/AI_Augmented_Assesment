namespace TaskBridge.Core.Exceptions;

/// <summary>
/// Exception thrown when a user lacks authorization to perform an action.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the UnauthorizedException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message) : base(message)
    {
    }
}
