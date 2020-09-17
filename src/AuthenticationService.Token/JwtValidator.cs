using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace LT.DigitalOffice.AuthenticationService.Token
{
    public class JwtValidator : IJwtValidator
    {
        private readonly TokenValidationParameters validationParameters;

        public JwtValidator([FromServices] TokenValidationParameters validationParameters)
        {
            this.validationParameters = validationParameters;
        }

        public void ValidateJwt(string jwt)
        {
            try
            {
                new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out _);
            }
            catch(SecurityTokenValidationException exception)
            {
                throw new Exception($"Token failed validation: {exception.Message}");
            }
            catch(ArgumentException exception)
            {
                throw new Exception($"Token was wrong format: {exception.Message}");
            }
        }
    }
}