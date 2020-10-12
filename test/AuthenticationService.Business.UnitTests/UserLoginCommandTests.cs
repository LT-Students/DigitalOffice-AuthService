using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.Kernel.UnitTestLibrary;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public string Salt { get; set; }
    }

    public class UserLoginCommandTests
    {
        #region Variables declaration
        private Mock<INewToken> newTokenMock;
        private Mock<IValidator<UserLoginInfoRequest>> validatorMock;
        private Mock<IRequestClient<IUserCredentialsRequest>> requestBrokerMock;

        private IUserLoginCommand command;
        private string internalSalt = "LT.DigitalOffice.SALT3";

        private UserCredentialsResponse brokerResponse;
        private UserLoginInfoRequest newUserCredentials;
        private OperationResult<UserCredentialsResponse> operationResult;
        #endregion

        #region Setup
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            validatorMock = new Mock<IValidator<UserLoginInfoRequest>>();

            newUserCredentials = new UserLoginInfoRequest
            {
                Login = "ExampleLogin",
                Password = "Example_1234"
            };

            BrokerSetUp();

            newTokenMock = new Mock<INewToken>();
            command = new UserLoginCommand(newTokenMock.Object, validatorMock.Object, requestBrokerMock.Object);

            validatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(true);
        }

        public void BrokerSetUp()
        {
            string salt = "Example_Salt1";

            var passwordHash = Encoding.UTF8.GetString(new SHA512Managed().ComputeHash(
                Encoding.UTF8.GetBytes($"{salt}{ newUserCredentials.Login}{newUserCredentials.Password}{internalSalt}")));

            var responseBrokerMock = new Mock<Response<IOperationResult<IUserCredentialsResponse>>>();
            requestBrokerMock = new Mock<IRequestClient<IUserCredentialsRequest>>();


            brokerResponse = new UserCredentialsResponse
            {
                UserId = Guid.NewGuid(),
                PasswordHash = passwordHash,
                Salt = salt
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

            var expectedLoginResponse = new UserLoginResult
            {
                UserId = brokerResponse.UserId,
                Token = JwtToken
            };

            newTokenMock
                .Setup(X => X.GetNewToken(newUserCredentials.Login))
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

            Assert.That(() => command.Execute(newUserCredentials),
                Throws.InstanceOf<ForbiddenException>().And.Message.EqualTo("User email not found"));
        }

        [Test]
        public void ShouldThrowExceptionWhenUserLoginInfoNotValid()
        {
            newUserCredentials.Login = "";

            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("error", "something", null)
                    }));

            Assert.ThrowsAsync<ValidationException>(() => command.Execute(newUserCredentials));
        }
        #endregion
    }
}