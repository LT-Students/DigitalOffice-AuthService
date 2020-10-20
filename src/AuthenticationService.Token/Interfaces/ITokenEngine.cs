namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    public interface ITokenEngine
    {
        /// <summary>
        /// Create new token based on user login.
        /// </summary>
        string Create(string login);
    }
}
