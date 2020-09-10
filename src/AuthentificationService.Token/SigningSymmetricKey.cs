using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LT.DigitalOffice.AuthenticationService.Token
{
    public class SigningSymmetricKey : IJwtSigningEncodingKey, IJwtSigningDecodingKey
    {
        private const string SIGNING_SECURITY_KEY = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

        private readonly SymmetricSecurityKey secretKey;

        public string SigningAlgorithm { get; } = SecurityAlgorithms.HmacSha256;

        public SigningSymmetricKey()
        {
            this.secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_SECURITY_KEY));
        }

        public SecurityKey GetKey() => this.secretKey;
    }
}