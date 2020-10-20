using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LT.DigitalOffice.AuthenticationService.Token
{
    /// <summary>
    /// Provides a method for encoding and decoding key jwt.
    /// </summary>
    public class SigningSymmetricKey : IJwtSigningEncodingKey, IJwtSigningDecodingKey
    {
        private const string SIGNING_SECURITY_KEY = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

        private readonly SymmetricSecurityKey _secretKey;

        public string SigningAlgorithm => SecurityAlgorithms.HmacSha256;

        public SigningSymmetricKey()
        {
            _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_SECURITY_KEY));
        }

        public SecurityKey GetKey() => _secretKey;
    }
}