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
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Business.Commands
{
    public class LoginCommand : ILoginCommand
    {
        private readonly ITokenEngine tokenEngine;
        private readonly ILoginValidator validator;
        private readonly IRequestClient<IUserCredentialsRequest> requestClient;

        public LoginCommand(
            ITokenEngine tokenEngine,
            ILoginValidator validator,
            IRequestClient<IUserCredentialsRequest> requestClient)
        {
            this.tokenEngine = tokenEngine;
            this.validator = validator;
            this.requestClient = requestClient;
        }

        public async Task<LoginResult> Execute(LoginRequest request)
        {
            validator.ValidateAndThrowCustom(request);

            IUserCredentialsResponse userCredentials = await GetUserCredentials(request.LoginData);

            VerifyPasswordHash(userCredentials, request.Password);

            var result = new LoginResult
            {
                UserId = userCredentials.UserId,
                Token = tokenEngine.Create(userCredentials.UserId)
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
