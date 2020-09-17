namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Represents interface for validation user token.
    /// </summary>
    public interface IJwtValidator
    {
        /// <summary>
        /// Validate user token.
        /// </summary>
        /// <param name="jwt">String user token.</param>
        void ValidateJwt(string jwt);
    }
}
