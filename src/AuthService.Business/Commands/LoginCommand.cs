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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            [FromServices] ITokenEngine tokenEngine,
            [FromServices] ILoginValidator validator,
            [FromServices] ILogger<LoginCommand> logger,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] IRequestClient<IUserCredentialsRequest> requestClient)
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

            var savedUserCredentials = await GetUserCredentials(request.LoginData);

            VerifyPasswordHash(savedUserCredentials, request.Password, savedUserCredentials.UserLogin);

            var result = new LoginResult
            {
                UserId = savedUserCredentials.UserId,
                Token = _tokenEngine.Create(savedUserCredentials.UserId)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string loginData)
        {
            var brokerResponse = await _requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
              IUserCredentialsRequest.CreateObj(loginData));

            if (!brokerResponse.Message.IsSuccess)
            {
                _logger.LogWarning("Exception while searched user credentials. " +
                    $"Reason: { string.Join(",", brokerResponse.Message.Errors) }");

                throw new NotFoundException(brokerResponse.Message.Errors);
            }

            return brokerResponse.Message.Body;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse savedUserCredentials, string requestPassword, string userLogin)
        {
            string requestPasswordHash = PasswordHelper.GetPasswordHash(
              userLogin,
              savedUserCredentials.Salt,
              requestPassword);

            if (!string.Equals(savedUserCredentials.PasswordHash, requestPasswordHash))
            {
                _logger.LogWarning($"Wrong user credentials. User login data: { userLogin }");

                throw new ForbiddenException("Wrong user credentials.");
            }
        }
    }
}
