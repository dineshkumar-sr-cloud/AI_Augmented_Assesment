namespace TaskBridge.Core.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate resource).
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConflictException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConflictException(string message) : base(message)
    {
    }
}
