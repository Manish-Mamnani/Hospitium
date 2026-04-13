namespace Hospitium.AuthService.Exceptions
{
    /// <summary>
    /// Exception thrown when provided authentication credentials are invalid.
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message)
        {
        }
    }
}
