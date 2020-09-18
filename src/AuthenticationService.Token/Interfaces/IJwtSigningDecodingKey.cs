using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Represents interface for getting decoding key jwt.
    /// </summary>
    public interface IJwtSigningDecodingKey
    {
        /// <summary>
        /// Method for getting decoding key jwt.
        /// </summary>
        /// <returns>Return key to verify the signature(public key).</returns>
        SecurityKey GetKey();
    }
}