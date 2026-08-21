namespace TaskBridge.Core.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message) : base(message)
    {
    }
}
