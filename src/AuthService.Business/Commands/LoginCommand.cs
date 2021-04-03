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
using System.Threading.Tasks;

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

        public async Task<LoginResult> Execute(LoginRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var userIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            _logger.LogInformation($"User login data: { request.LoginData }. " +
                $"User ip address: { userIp } ,who tried to authenticate.");

            var userCredentials = await GetUserCredentials(request.LoginData);

            VerifyPasswordHash(userCredentials, request.Password);

            var result = new LoginResult
            {
                UserId = userCredentials.UserId,
                Token = _tokenEngine.Create(userCredentials.UserId)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string loginData)
        {
            IUserCredentialsResponse userCredentials = null;
            Response<IOperationResult<IUserCredentialsResponse>> brokerResponse;

            try
            {
                brokerResponse = await _requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    IUserCredentialsRequest.CreateObj(loginData));

                if (brokerResponse.Message.IsSuccess)
                {
                    userCredentials = brokerResponse.Message.Body;
                }
            }
            catch (Exception exc)
            {
                var message = "Exception while searched user to UserDb.";

                _logger.LogError(exc, message);

                throw new Exception(message);
            }

            if (!brokerResponse.Message.IsSuccess)
            {
                _logger.LogWarning($"Can not find user credentials. " +
                    $"Reason: { string.Join(",", brokerResponse.Message.Errors) }");

                throw new NotFoundException(brokerResponse.Message.Errors);
            }

            return userCredentials;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse userCredentials, string requestPassword)
        {
            string requestPasswordHash = PasswordHelper.GetPasswordHash(
              userCredentials.UserLogin,
              userCredentials.Salt,
              requestPassword);

            if (!string.Equals(userCredentials.PasswordHash, requestPasswordHash))
            {
                _logger.LogWarning($"Wrong user credentials. User login data: { userCredentials.UserLogin }");

                throw new ForbiddenException("Wrong user credentials.");
            }
        }
    }
}
