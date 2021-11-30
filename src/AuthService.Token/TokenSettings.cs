namespace LT.DigitalOffice.AuthService.Token
{
  /// <summary>
  /// Token configuration class filled in Startup.cs.
  /// </summary>
  public class TokenSettings
  {
    public double AccessTokenLifetimeInMinutes { get; set; }
    public double RefreshTokenLifetimeInMinutes { get; set; }
    public string TokenIssuer { get; set; }
    public string TokenAudience { get; set; }
  }
}
