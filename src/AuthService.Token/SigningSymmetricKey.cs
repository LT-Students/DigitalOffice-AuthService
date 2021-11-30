using System.Text;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthService.Token
{
  public class SigningSymmetricKey : IJwtSigningEncodingKey, IJwtSigningDecodingKey
  {
    private const string SIGNING_SECURITY_KEY = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

    private readonly SymmetricSecurityKey _secretKey;

    public string SigningAlgorithm => SecurityAlgorithms.HmacSha512;

    public SigningSymmetricKey()
    {
      _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_SECURITY_KEY));
    }

    public SecurityKey GetKey() => _secretKey;
  }
}
