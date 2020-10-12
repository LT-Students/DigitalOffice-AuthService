using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Responses;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.AuthenticationService.Validation.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.UnitTestLibrary;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Exceptions;

namespace LT.DigitalOffice.AuthenticationService.Business.UnitTests
{
    public class OperationResult<T> : IOperationResult<T>
    {
        public bool IsSuccess { get; set; }

        public List<string> Errors { get; set; }

        public T Body { get; set; }
    }

    public class UserCredentialsResponse : IUserCredentialsResponse
    {
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
    }

    public class UserLoginCommandTests
    {
        #region Variables declaration
        private Mock<ITokenEngine> tokenEngineMock;
        private Mock<ILoginValidator> loginValidatorMock;
        private Mock<IRequestClient<IUserCredentialsRequest>> requestBrokerMock;

        private ILoginCommand command;

        private UserCredentialsResponse brokerResponse;
        private LoginRequest newUserCredentials;
        private OperationResult<UserCredentialsResponse> operationResult;
        #endregion

        #region Setup
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            loginValidatorMock = new Mock<ILoginValidator>();

            newUserCredentials = new LoginRequest
            {
                LoginData = "Example@gmail.com",
                Password = "Example_1234"
            };

            BrokerSetUp();

            tokenEngineMock = new Mock<ITokenEngine>();
            command = new LoginCommand(tokenEngineMock.Object, loginValidatorMock.Object, requestBrokerMock.Object);

            loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(true);
        }

        public void BrokerSetUp()
        {
            var passwordHash = Encoding.UTF8.GetString(new SHA512Managed().ComputeHash(
                Encoding.UTF8.GetBytes(newUserCredentials.Password)));

            var responseBrokerMock = new Mock<Response<IOperationResult<IUserCredentialsResponse>>>();
            requestBrokerMock = new Mock<IRequestClient<IUserCredentialsRequest>>();


            brokerResponse = new UserCredentialsResponse
            {
                UserId = Guid.NewGuid(),
                PasswordHash = passwordHash
            };

            operationResult = new OperationResult<UserCredentialsResponse>
            {
                IsSuccess = true,
                Errors = new List<string>(),
                Body = brokerResponse
            };

            requestBrokerMock.Setup(
                x => x.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    It.IsAny<object>(), default, default))
                .Returns(Task.FromResult(responseBrokerMock.Object));

            responseBrokerMock
                .SetupGet(x => x.Message)
                .Returns(operationResult);
        }
        #endregion

        #region Successful test
        [Test]
        public void ShouldReturnUserIdAndJwtWhenPasswordsAndEmailHasMatch()
        {
            string JwtToken = "Example_jwt";

            var expectedLoginResponse = new LoginResult
            {
                UserId = brokerResponse.UserId,
                Token = JwtToken
            };

            tokenEngineMock
                .Setup(X => X.Create(newUserCredentials.LoginData))
                .Returns(JwtToken);

            SerializerAssert.AreEqual(expectedLoginResponse, command.Execute(newUserCredentials).Result);
        }
        #endregion

        #region Fail tests
        [Test]
        public void ShouldThrowExceptionWhenPasswordsHasNotMatch()
        {
            newUserCredentials.Password = "Example";

            Assert.ThrowsAsync<ForbiddenException>(() => command.Execute(newUserCredentials));
        }

        [Test]
        public void ShouldThrowExceptionWhenUserEmailHasNotMatchInDb()
        {
            operationResult.IsSuccess = false;
            operationResult.Errors = new List<string>() { "User email not found" };
            operationResult.Body = null;

            Assert.ThrowsAsync<NotFoundException>(
                () => command.Execute(newUserCredentials),
                "User email not found");
        }

        [Test]
        public void ShouldThrowExceptionWhenUserLoginInfoNotValid()
        {
            newUserCredentials.LoginData = string.Empty;

            loginValidatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.ThrowsAsync<ValidationException>(() => command.Execute(newUserCredentials));
        }
        #endregion
    }
}