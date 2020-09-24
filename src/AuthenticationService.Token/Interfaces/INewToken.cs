namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Provides a method for getting new token.
    /// </summary>
    public interface INewToken
    {
        string GetNewToken(string userEmail);
    }
}
