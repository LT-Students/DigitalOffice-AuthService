namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Provides a method for validation user token.
    /// </summary>
    public interface IJwtValidator
    {
        /// <summary>
        /// Validate user token.
        /// </summary>
        /// <param name="jwt">User token.</param>
        void ValidateAndThrow(string jwt);
    }
}
