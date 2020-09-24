using LT.DigitalOffice.AuthenticationService.Token;
using LT.DigitalOffice.Kernel.Exceptions;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LT.DigitalOffice.AuthenticationService.Token.UnitTests
{
    class JwtValidationTests
    {
        string audience;
        string userEmail;
        private string userJwt;
        SymmetricSecurityKey encodingKey;
        private JwtValidator jwtValidation;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string signingSecurityKey = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

            userEmail = "Example_123@gmail.com";

            encodingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecurityKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "AuthService",
                ValidateAudience = true,
                ValidAudience = "Client",
                ValidateLifetime = true,
                IssuerSigningKey = encodingKey,
                ValidateIssuerSigningKey = true
            };

            jwtValidation = new JwtValidator(validationParameters);
        }

        [Test]
        public void ShouldThrowExceptionWhenTokenIsNotValid()
        {
            audience = "";

            CreateToken();

            var expectedExeption = "IDX10214: Audience validation failed. Audiences: '[PII is hidden. For more " +
                "details, see https://aka.ms/IdentityModel/PII.]'. Did not match: " +
                "validationParameters.ValidAudience: '[PII is hidden. For more details, " +
                "see https://aka.ms/IdentityModel/PII.]' or validationParameters.ValidAudiences: " +
                "'[PII is hidden. For more details, see https://aka.ms/IdentityModel/PII.]'.";

            Assert.Throws<BadRequestException>(() => jwtValidation.ValidateJwt(userJwt),
                $"Token failed validation: { expectedExeption }");
        }

        [Test]
        public void ShouldThrowExceptionWhenTokeWasWrongFormat()
        {
            userJwt = "Example_userJwt";

            var expectedExeption = "IDX12741: JWT: '[PII is hidden. For more details, " +
                "see https://aka.ms/IdentityModel/PII.]' must have three segments (JWS) or five segments (JWE).";


            Assert.Throws<BadRequestException>(() => jwtValidation.ValidateJwt(userJwt),
                $"Token was wrong format: { expectedExeption}");
        }

        [Test]
        public void ShouldNotThrowsWhenTokenIsValid()
        {
            audience = "Client";

            CreateToken();
            jwtValidation.ValidateJwt(userJwt);
        }

        private void CreateToken()
        {
            var signingAlgorithm = SecurityAlgorithms.HmacSha256;

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userEmail)
            };

            var jwt = new JwtSecurityToken(
                    issuer: "AuthService",
                    audience: audience,
                    notBefore: DateTime.UtcNow,
                    claims: claims,     // Contains information of user email
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: new SigningCredentials(
                            encodingKey,
                            signingAlgorithm)
                    );

            userJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
