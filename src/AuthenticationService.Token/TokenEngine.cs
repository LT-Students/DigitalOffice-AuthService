using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LT.DigitalOffice.AuthenticationService.Token
{
    public class TokenEngine : ITokenEngine
    {
        public const string ClaimUserId = "UserId";

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
        public string Create(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User was not found.");
            }

            var claims = new[]
            {
                new Claim(ClaimUserId, userId.ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Value.TokenIssuer,
                audience: _tokenOptions.Value.TokenAudience,
                notBefore: DateTime.UtcNow,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_tokenOptions.Value.TokenLifetimeInMinutes),
                signingCredentials: new SigningCredentials(
                    _signingEncodingKey.GetKey(),
                    _signingEncodingKey.SigningAlgorithm));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}