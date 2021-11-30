using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace LT.DigitalOffice.AuthService.Token.UnitTests
{
  public class JwtValidationTests
  {
    private string _audience;
    private string _userJwt;
    private SymmetricSecurityKey _encodingKey;
    private TokenValidator _jwtValidation;
    private IJwtSigningDecodingKey _decodingKey;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      const string signingSecurityKey = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

      _encodingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecurityKey));

      _decodingKey = (IJwtSigningDecodingKey)new SigningSymmetricKey();

      var options = Options.Create(new TokenSettings
      {
        TokenIssuer = "AuthService",
        TokenAudience = "Client"
      });

      _jwtValidation = new TokenValidator(_decodingKey, options, NullLogger<TokenValidator>.Instance);
    }

    [Test]
    public void ShouldThrowExceptionWhenTokenIsNotValid()
    {
      _audience = "";

      CreateToken();

      Assert.Throws<ForbiddenException>(
        () => _jwtValidation.Validate(_userJwt, TokenType.Access),
        "Token failed validation.");
    }

    [Test]
    public void ShouldThrowExceptionWhenTokenWasWrongFormat()
    {
      _userJwt = "Example_userJwt";

      Assert.Throws<BadRequestException>(
        () => _jwtValidation.Validate(_userJwt, TokenType.Access),
        "Token was wrong format.");
    }

    [Test]
    public void ShouldNotThrowsWhenTokenIsValid()
    {
      _audience = "Client";

      CreateToken();

      _jwtValidation.Validate(_userJwt, TokenType.Access);
    }

    private void CreateToken()
    {
      var signingAlgorithm = SecurityAlgorithms.HmacSha256;

      var claims = new[]
      {
        new Claim(TokenEngine.ClaimUserId, Guid.NewGuid().ToString()),
        new Claim(TokenEngine.ClaimTokenType, TokenType.Access.ToString())
      };

      var jwt = new JwtSecurityToken(
        issuer: "AuthService",
        audience: _audience,
        notBefore: DateTime.UtcNow,
        claims: claims,
        expires: DateTime.Now.AddMinutes(5),
        signingCredentials: new SigningCredentials(
          _encodingKey,
          signingAlgorithm));

      _userJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
    }
  }
}
