using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Business.Helpers;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace LT.DigitalOffice.AuthService.Business.Commands
{
    public class LoginCommand : ILoginCommand
    {
        private readonly ITokenEngine _tokenEngine;
        private readonly ILoginValidator _validator;
        private readonly ILogger<LoginCommand> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestClient<IUserCredentialsRequest> _requestClient;

        public LoginCommand(
            ITokenEngine tokenEngine,
            ILoginValidator validator,
            ILogger<LoginCommand> logger,
            IHttpContextAccessor httpContextAccessor,
            IRequestClient<IUserCredentialsRequest> requestClient)
        {
            _logger = logger;
            _validator = validator;
            _tokenEngine = tokenEngine;
            _requestClient = requestClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public LoginResult Execute(LoginRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var userCredentials = GetUserCredentials(request.LoginData);

            if (userCredentials == null)
            {
                throw new NotFoundException("Could not find user.");
            }

            VerifyPasswordHash(userCredentials, request.Password);

            var result = new LoginResult
            {
                UserId = userCredentials.UserId,
                Token = _tokenEngine.Create(userCredentials.UserId)
            };

            return result;
        }

        private IUserCredentialsResponse GetUserCredentials(string loginData)
        {
            var userIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            string userIpTemplate = $"User ip address: {userIp}, who tried to authenticate.";

            try
            {
                var brokerResponse = _requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    IUserCredentialsRequest.CreateObj(loginData)).Result;

                if (brokerResponse.Message.IsSuccess)
                {
                    var userCredentials = brokerResponse.Message.Body;

                    _logger.LogInformation($"User login data: '{loginData}'." +
                        "Broker conversation id: {ConversationId}." +
                        userIpTemplate, brokerResponse.ConversationId);

                    return userCredentials;
                }

                _logger.LogWarning($"Can not find user credentials." +
                    $"Reason: '{string.Join(",", brokerResponse.Message.Errors)}'" +
                    "Broker Conversation id: {ConversationId}" + userIpTemplate, brokerResponse.ConversationId);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Exception while searched user to UserDb.");
            }

            return null;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse userCredentials, string requestPassword)
        {
            string requestPasswordHash = PasswordHelper.GetPasswordHash(
              userCredentials.UserLogin,
              userCredentials.Salt,
              requestPassword);

            if (!string.Equals(userCredentials.PasswordHash, requestPasswordHash))
            {
                _logger.LogWarning($"Wrong user credentials. User login data: '{userCredentials.UserLogin}'");

                throw new ForbiddenException("Wrong user credentials.");
            }
        }
    }
}
