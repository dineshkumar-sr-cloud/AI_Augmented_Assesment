namespace TaskBridge.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
