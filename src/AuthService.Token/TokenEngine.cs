using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthService.Token
{
  public class TokenEngine : ITokenEngine
  {
    public const string ClaimUserId = "UserId";
    public const string ClaimTokenType = "TokenType";

    private readonly IJwtSigningEncodingKey _signingEncodingKey;
    private readonly IOptions<TokenSettings> _tokenOptions;

    public TokenEngine(
      [FromServices] IJwtSigningEncodingKey signingEncodingKey,
      [FromServices] IOptions<TokenSettings> tokenOptions)
    {
      _signingEncodingKey = signingEncodingKey;
      _tokenOptions = tokenOptions;
    }

    /// <inheritdoc />
    public string Create(Guid userId, TokenType tokenType, out double tokenExpiresIn)
    {
      if (userId == Guid.Empty)
      {
        throw new NotFoundException("User was not found.");
      }

      var claims = new[]
      {
        new Claim(ClaimUserId, userId.ToString()),
        new Claim(ClaimTokenType, tokenType.ToString())
      };

      tokenExpiresIn = tokenType == TokenType.Access
        ? _tokenOptions.Value.AccessTokenLifetimeInMinutes
        : _tokenOptions.Value.RefreshTokenLifetimeInMinutes;

      var jwt = new JwtSecurityToken(
        issuer: _tokenOptions.Value.TokenIssuer,
        audience: _tokenOptions.Value.TokenAudience,
        notBefore: DateTime.UtcNow,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(tokenExpiresIn),
        signingCredentials: new SigningCredentials(
          _signingEncodingKey.GetKey(),
          _signingEncodingKey.SigningAlgorithm));

      return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
  }
}
