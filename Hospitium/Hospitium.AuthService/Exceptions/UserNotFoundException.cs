namespace Hospitium.AuthService.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested user cannot be found in the system.
    /// </summary>
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}
