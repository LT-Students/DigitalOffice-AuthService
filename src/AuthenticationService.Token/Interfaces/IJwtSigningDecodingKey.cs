using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Provides a method for getting decoding key jwt.
    /// </summary>
    public interface IJwtSigningDecodingKey
    {
        /// <summary>
        /// Gets the decoding key jwt.
        /// </summary>
        /// <returns>Key to verify the signature(public key).</returns>
        SecurityKey GetKey();
    }
}