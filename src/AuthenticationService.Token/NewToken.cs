using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LT.DigitalOffice.AuthenticationService.Token
{
    public class NewToken : INewToken
    {
        private readonly IJwtSigningEncodingKey signingEncodingKey;
        private readonly IOptions<TokenOptions> tokenOptions;

        public NewToken(
            [FromServices] IJwtSigningEncodingKey signingEncodingKey,
            [FromServices] IOptions<TokenOptions> tokenOptions)
        {
            this.signingEncodingKey = signingEncodingKey;
            this.tokenOptions = tokenOptions;
        }

        public string GetNewToken(string userEmail)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userEmail)
            };

            var jwt = new JwtSecurityToken(
                    issuer: tokenOptions.Value.TokenIssuer,
                    audience: tokenOptions.Value.TokenAudience,
                    notBefore: DateTime.UtcNow,
                    claims: claims,     // Contains information of user email
                    expires: DateTime.Now.AddMinutes(tokenOptions.Value.TokenLifetimeInMinutes),
                    signingCredentials: new SigningCredentials(
                            signingEncodingKey.GetKey(),
                            signingEncodingKey.SigningAlgorithm)
                    );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}