using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
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
    public class UserLoginCommand : IUserLoginCommand
    {
        private string userLogin;
        private readonly INewToken token;

        private readonly IValidator<UserLoginInfoRequest> validator;
        private readonly IRequestClient<IUserCredentialsRequest> requestClient;

        public UserLoginCommand(
            [FromServices] INewToken token,
            [FromServices] IValidator<UserLoginInfoRequest> validator,
            [FromServices] IRequestClient<IUserCredentialsRequest> requestClient)
        {
            this.token = token;
            this.validator = validator;
            this.requestClient = requestClient;
        }

        public async Task<UserLoginResult> Execute(UserLoginInfoRequest loginInfo)
        {
            userLogin = loginInfo.Login;

            validator.ValidateAndThrowCustom(loginInfo);

            var userCredentials = await GetUserCredentials(loginInfo.Login);

            VerifyPasswordHash(userCredentials, loginInfo.Password);

            var result = new UserLoginResult
            {
                UserId = userCredentials.UserId,
                Token = token.GetNewToken(loginInfo.Login)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string userLogin)
        {
            var brokerResponse = await requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(new
            {
                Login = userLogin
            });

            if (!brokerResponse.Message.IsSuccess)
            {
                throw new ForbiddenException(brokerResponse.Message.Errors);
            }

            return brokerResponse.Message.Body;
        }

        private void VerifyPasswordHash(IUserCredentialsResponse userCredentials, string userPassword)
        {
            string userPasswordHash = UserPassword.GetPasswordHash(
                userLogin,
                userCredentials.Salt,
                userPassword);

            if (userCredentials.PasswordHash != userPasswordHash)
            {
                throw new ForbiddenException("Wrong user password.");
            }
        }
    }
}