using System;
using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.AuthService.Business.Commands
{
    public class RefreshTokenCommand : IRefreshTokenCommand
    {
        private readonly ITokenValidator _tokenValidator;
        private readonly ITokenEngine _tokenEngine;
        private readonly IRefreshValidator _validator;
        private readonly ILogger<RefreshTokenCommand> _logger;
        private readonly HttpContext _httpContext;

        public RefreshTokenCommand(
            ITokenValidator loginCommand,
            ITokenEngine tokenEngine,
            IRefreshValidator validator,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RefreshTokenCommand> logger)
        {
            _tokenValidator = loginCommand;
            _tokenEngine = tokenEngine;
            _validator = validator;
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public LoginResult Execute(RefreshRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            Guid userId = _tokenValidator.Validate(request.RefreshToken, TokenType.Refresh);

            var result = new LoginResult
            {
                UserId = userId,
                AccessToken = _tokenEngine.Create(userId, TokenType.Access),
                RefreshToken = _tokenEngine.Create(userId, TokenType.Refresh)
            };
            
            _logger.LogInformation(
                "User was successfully refreshed token with UserId: '{userId}' from IP: {requestIP}",
                userId,
                _httpContext.Connection.RemoteIpAddress);

            return result;
        }
    }
}