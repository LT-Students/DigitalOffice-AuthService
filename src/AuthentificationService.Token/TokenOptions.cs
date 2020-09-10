namespace LT.DigitalOffice.AuthenticationService.Token
{
    public class TokenOptions
    {
        public double TokenLifetimeInMinutes { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
    }
}