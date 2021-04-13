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
        private readonly IRequestClient<IUserCredentialsRequest> _requestClient;
        private readonly ILogger<LoginCommand> _logger;
        private readonly HttpContext _httpContext;

        public LoginCommand(
            ITokenEngine tokenEngine,
            ILoginValidator validator,
            IRequestClient<IUserCredentialsRequest> requestClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LoginCommand> logger)
        {
            _tokenEngine = tokenEngine;
            _validator = validator;
            _requestClient = requestClient;
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<LoginResult> Execute(LoginRequest request)
        {
            _logger.LogInformation(
                "User login request for LoginData: '{loginData}' from IP: '{requestIP}'.",
                request.LoginData,
                _httpContext.Connection.RemoteIpAddress);

            _validator.ValidateAndThrowCustom(request);

            IUserCredentialsResponse userCredentials = await GetUserCredentials(request.LoginData);

            if (userCredentials == null)
            {
                throw new NotFoundException(
                    "User was not found, please check your credentials and try again. In case this error occurred again contact DO support team by email 'spartak.ryabtsev@lanit-tercom.com'.");
            }

            VerifyPasswordHash(userCredentials, request.Password);

            var result = new LoginResult
            {
                UserId = userCredentials.UserId,
                Token = _tokenEngine.Create(userCredentials.UserId)
            };

            _logger.LogInformation(
                "User was successfully logged in with LoginData: '{loginData}' from IP: {requestIP}",
                request.LoginData,
                _httpContext.Connection.RemoteIpAddress);

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string loginData)
        {
            IUserCredentialsResponse result = null;

            try
            {
                var brokerResponse = await _requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    IUserCredentialsRequest.CreateObj(loginData));

                if (!brokerResponse.Message.IsSuccess)
                {
                    _logger.LogWarning("Can't get user credentials for LoginData: '{loginData}'", loginData);
                }
                else
                {
                    result = brokerResponse.Message.Body;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(
                    exc,
                    "Exception was caught while receiving user credentials for LoginData: {loginData}",
                    loginData);
            }

            return result;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse savedUserCredentials, string requestPassword)
        {
            string requestPasswordHash = PasswordHelper.GetPasswordHash(
                savedUserCredentials.UserLogin,
                savedUserCredentials.Salt,
                requestPassword);

            if (!string.Equals(savedUserCredentials.PasswordHash, requestPasswordHash))
            {
                throw new ForbiddenException("Wrong user credentials.");
            }
        }
    }
}
