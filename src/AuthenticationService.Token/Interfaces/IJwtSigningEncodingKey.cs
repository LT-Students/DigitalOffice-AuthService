using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    /// <summary>
    /// Provides a method for create encoding key jwt.
    /// </summary>
    public interface IJwtSigningEncodingKey
    {
        ///<value>Type of algorithm encoding(HS256).</value>
        string SigningAlgorithm { get; }

        /// <summary>
        /// Gets the encoding key jwt.
        /// </summary>
        /// <returns>Key to create the signature(private key).</returns>
        SecurityKey GetKey();
    }
}