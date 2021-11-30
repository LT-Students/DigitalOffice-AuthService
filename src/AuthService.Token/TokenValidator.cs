using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LT.DigitalOffice.AuthService.Token
{
  /// <inheritdoc/>
  public class TokenValidator : ITokenValidator
  {
    private readonly IJwtSigningDecodingKey _decodingKey;
    private readonly IOptions<TokenSettings> _options;
    private readonly ILogger<TokenValidator> _logger;

    public TokenValidator(
      [FromServices] IJwtSigningDecodingKey decodingKey,
      [FromServices] IOptions<TokenSettings> options,
      [FromServices] ILogger<TokenValidator> logger)
    {
      _decodingKey = decodingKey;
      _options = options;
      _logger = logger;
    }

    /// <inheritdoc/>
    public Guid Validate(string token, TokenType tokenType)
    {
      if (string.IsNullOrEmpty(token))
      {
        throw new BadRequestException("Token can not be empty.");
      }

      try
      {
        var validationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidIssuer = _options.Value.TokenIssuer,
          ValidateAudience = true,
          ValidAudience = _options.Value.TokenAudience,
          ValidateLifetime = true,
          IssuerSigningKey = _decodingKey.GetKey(),
          ValidateIssuerSigningKey = true
        };

        ClaimsPrincipal claims = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        var userIdClaimValue = claims.FindFirst("UserId")?.Value;
        var tokenTypeClaimValue = claims.FindFirst("TokenType")?.Value;
        if (string.IsNullOrEmpty(userIdClaimValue) ||
          !Guid.TryParse(userIdClaimValue, out Guid userId) ||
          tokenType.ToString() != tokenTypeClaimValue)
        {
          throw new SecurityTokenValidationException("Bad token format.");
        }

        return userId;
      }
      catch (SecurityTokenValidationException exc)
      {
        string message = "Token failed validation.";

        _logger.LogInformation($"{message}{Environment.NewLine}{exc}");

        throw new ForbiddenException(message);
      }
      catch (Exception exc)
      {
        string message = "Token format was wrong.";

        _logger.LogInformation($"{message}{Environment.NewLine}{exc}");

        throw new BadRequestException(message);
      }
    }
  }
}
