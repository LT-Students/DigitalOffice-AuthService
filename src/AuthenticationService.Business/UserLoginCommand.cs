﻿using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Business
{
    public class UserLoginCommand : IUserLoginCommand
    {
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
            validator.ValidateAndThrow(loginInfo);

            var userCredentials = await GetUserCredentials(loginInfo.Email);

            VerifyPasswordHash(loginInfo.Password, userCredentials.PasswordHash);

            var result = new UserLoginResult
            {
                UserId = userCredentials.UserId,
                Token = token.GetNewToken(loginInfo.Email)
            };

            return result;
        }

        private async Task<IUserCredentialsResponse> GetUserCredentials(string userEmail)
        {
            var brokerResponse = await requestClient.GetResponse<IOperationResult<IUserCredentialsResponse>>(new
            {
                Email = userEmail
            });

            if (!brokerResponse.Message.IsSuccess)
            {
                throw new Exception(String.Join(", ", brokerResponse.Message.Errors));
            }

            return brokerResponse.Message.Body;
        }

        private void VerifyPasswordHash(string userPassword, string passwordHash)
        {
            var shaM = new SHA512Managed();

            var userPasswordHash = Encoding.UTF8.GetString(shaM.ComputeHash(
                Encoding.UTF8.GetBytes(userPassword)));

            if (passwordHash != userPasswordHash)
            {
                throw new Exception("Wrong password.");
            }
        }
    }
}