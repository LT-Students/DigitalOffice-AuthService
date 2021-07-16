using LT.DigitalOffice.AuthService.Token.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Text;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;

namespace LT.DigitalOffice.AuthService.Token.UnitTests
{
    public class CreateNewTokenTests
    {
        private Mock<IJwtSigningEncodingKey> signingEncodingKey;
        private SymmetricSecurityKey expectedKey;
        private ITokenEngine tokenEngine;
        private IOptions<TokenSettings> tokenOptions;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string securityKey = "qyfi0sjv1f3uiwkyflnwfvr7thpzxdxygt8t9xbhielymv20";

            signingEncodingKey = new Mock<IJwtSigningEncodingKey>();

            tokenOptions = Options.Create(new TokenSettings
            {
                TokenAudience = "AuthClient",
                TokenIssuer = "AuthClient",
                AccessTokenLifetimeInMinutes = 5,
                RefreshTokenLifetimeInMinutes = 10
            });

            expectedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            tokenEngine = new TokenEngine(signingEncodingKey.Object, tokenOptions);
        }

        [Test]
        public void SuccessfulCreateNewToken()
        {
            string signingAlgorithm = "HS256";
            double tokenExpiresIn;

            signingEncodingKey
                .Setup(x => x.GetKey())
                .Returns(expectedKey);

            signingEncodingKey
                .SetupGet(x => x.SigningAlgorithm)
                .Returns(signingAlgorithm);

            var newJwt = tokenEngine.Create(Guid.NewGuid(), TokenType.Access, out tokenExpiresIn);

            Assert.IsNotEmpty(newJwt);
            Assert.AreEqual(tokenOptions.Value.AccessTokenLifetimeInMinutes, tokenExpiresIn);
        }
    }
}