namespace Hospitium.AuthService.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to register a user with an already existing email.
    /// </summary>
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
