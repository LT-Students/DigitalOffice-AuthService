namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    public interface INewToken
    {
        string GetNewToken(string userEmail);
    }
}
