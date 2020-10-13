using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Responses;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.AuthenticationService.Validation.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Business
{
    public class LoginCommand : ILoginCommand
    {
        private string loginData;
        private readonly ITokenEngine tokenEngine;
        private readonly ILoginValidator validator;
        private readonly IRequestClient<IUserCredentialsRequest> requestClient;

        public LoginCommand(
            [FromServices] ITokenEngine tokenEngine,
            [FromServices] ILoginValidator validator,
            [FromServices] IRequestClient<IUserCredentialsRequest> requestClient)
        {
            this.tokenEngine = tokenEngine;
            this.validator = validator;
            this.requestClient = requestClient;
        }

        public async Task<LoginResult> Execute(LoginRequest request)
        {
            validator.ValidateAndThrowCustom(request);

            this.loginData = request.LoginData;

            var savedUserCredentials = await GetUserCredentials();

            VerifyPasswordHash(savedUserCredentials, request.Password);

            var result = new LoginResult
            {
                UserId = savedUserCredentials.UserId,
                Token = tokenEngine.Create(request.LoginData)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials()
        {
            var brokerResponse = await requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                IUserCredentialsRequest.CreateObj(loginData));

            if (!brokerResponse.Message.IsSuccess)
            {
                throw new NotFoundException(brokerResponse.Message.Errors);
            }

            return brokerResponse.Message.Body;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse savedUserCredentials, string requestPassword)
        {
            string requestPasswordHash = UserPassword.GetPasswordHash(
                loginData,
                savedUserCredentials.Salt,
                requestPassword);

            if (savedUserCredentials.PasswordHash != requestPasswordHash)
            {
                throw new ForbiddenException("Wrong user password.");
            }
        }
    }
}