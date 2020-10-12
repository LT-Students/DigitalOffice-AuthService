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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Business
{
    public class LoginCommand : ILoginCommand
    {
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

            var userCredentials = await GetUserCredentials(request.LoginData);

            VerifyPasswordHash(request.Password, userCredentials.PasswordHash);

            var result = new LoginResult
            {
                UserId = userCredentials.UserId,
                Token = tokenEngine.Create(request.LoginData)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string loginData)
        {
            var brokerResponse = await requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                IUserCredentialsRequest.CreateObj(loginData));

            if (!brokerResponse.Message.IsSuccess)
            {
                throw new NotFoundException(brokerResponse.Message.Errors);
            }

            return brokerResponse.Message.Body;
        }

        private void VerifyPasswordHash(string requestPassword, string savedPasswordHash)
        {
            var shaM = new SHA512Managed();

            var requestPasswordHash = Encoding.UTF8.GetString(shaM.ComputeHash(Encoding.UTF8.GetBytes(requestPassword)));

            if (!string.Equals(savedPasswordHash, requestPasswordHash))
            {
                throw new ForbiddenException();
            }
        }
    }
}