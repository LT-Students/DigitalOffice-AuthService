using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.AuthService.Business.Commands;
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
using LT.DigitalOffice.UnitTestKernel;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Business.UnitTests
{
    public class UserCredentialsResponse : IUserCredentialsResponse
    {
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string UserLogin { get; set; }
    }

    public class UserLoginCommandTests
    {
        #region private fields
        private Mock<ITokenEngine> tokenEngineMock;
        private Mock<ILoginValidator> loginValidatorMock;
        private Mock<ValidationResult> validationResultIsValidMock;
        private Mock<IRequestClient<IUserCredentialsRequest>> requestBrokerMock;
        private Mock<IOperationResult<IUserCredentialsResponse>> operationResultMock;
        private Mock<IUserCredentialsResponse> brokerResponseMock;

        private string salt;
        private ILoginCommand command;

        private LoginRequest newUserCredentials;
        private ValidationResult validationResultError;

        #endregion

        #region Setup
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            salt = "Example_Salt1";

            loginValidatorMock = new Mock<ILoginValidator>();

            newUserCredentials = new LoginRequest
            {
                LoginData = "User_login_example",
                Password = "Example_1234"
            };

            validationResultError = new ValidationResult(
                new List<ValidationFailure>
                {
                    new ValidationFailure("error", "something", null)
                });

            validationResultIsValidMock = new Mock<ValidationResult>();

            validationResultIsValidMock
                .Setup(x => x.IsValid)
                .Returns(true);

            BrokerSetUp();

            tokenEngineMock = new Mock<ITokenEngine>();
            command = new LoginCommand(tokenEngineMock.Object, loginValidatorMock.Object, requestBrokerMock.Object);
        }

        public void BrokerSetUp()
        {
            var passwordHash = PasswordHelper.GetPasswordHash(
                newUserCredentials.LoginData,
                salt,
                newUserCredentials.Password);

            var responseBrokerMock = new Mock<Response<IOperationResult<IUserCredentialsResponse>>>();
            requestBrokerMock = new Mock<IRequestClient<IUserCredentialsRequest>>();

            brokerResponseMock = new Mock<IUserCredentialsResponse>();
            brokerResponseMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
            brokerResponseMock.Setup(x => x.PasswordHash).Returns(passwordHash);
            brokerResponseMock.Setup(x => x.Salt).Returns(salt);
            brokerResponseMock.Setup(x => x.UserLogin).Returns(newUserCredentials.LoginData);

            operationResultMock = new Mock<IOperationResult<IUserCredentialsResponse>>();
            operationResultMock.Setup(x => x.Body).Returns(brokerResponseMock.Object);
            operationResultMock.Setup(x => x.IsSuccess).Returns(true);
            operationResultMock.Setup(x => x.Errors).Returns(new List<string>());

            requestBrokerMock.Setup(
                x => x.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    It.IsAny<object>(), default, default))
                .Returns(Task.FromResult(responseBrokerMock.Object));

            responseBrokerMock
                .SetupGet(x => x.Message)
                .Returns(operationResultMock.Object);
        }
        #endregion

        #region Successful test
        [Test]
        public void ShouldReturnUserIdAndJwtWhenPasswordsAndEmailHasMatch()
        {
            string JwtToken = "Example_jwt";

            var expectedLoginResponse = new LoginResult
            {
                UserId = brokerResponseMock.Object.UserId,
                Token = JwtToken
            };

            loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()))
               .Returns(validationResultIsValidMock.Object);

            tokenEngineMock
                .Setup(X => X.Create(brokerResponseMock.Object.UserId))
                .Returns(JwtToken);

            SerializerAssert.AreEqual(expectedLoginResponse, command.Execute(newUserCredentials).Result);
        }
        #endregion

        #region Fail tests
        [Test]
        public void ShouldThrowExceptionWhenPasswordsHasNotMatch()
        {
            loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()))
               .Returns(validationResultIsValidMock.Object);

            newUserCredentials.Password = "Example";

            Assert.ThrowsAsync<ForbiddenException>(() => command.Execute(newUserCredentials));
        }

        [Test]
        public void ShouldThrowExceptionWhenUserEmailHasNotMatchInDb()
        {
            operationResultMock.Setup(x => x.Body).Returns((IUserCredentialsResponse)null);
            operationResultMock.Setup(x => x.IsSuccess).Returns(false);
            operationResultMock.Setup(x => x.Errors).Returns(new List<string> { "User email not found" });

            loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()))
               .Returns(validationResultIsValidMock.Object);

            Assert.ThrowsAsync<NotFoundException>(
                () => command.Execute(newUserCredentials),
                "User email not found");
        }

        [Test]
        public void ShouldThrowExceptionWhenUserLoginInfoNotValid()
        {
            newUserCredentials.LoginData = string.Empty;

            loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()))
               .Returns(validationResultError);

            Assert.ThrowsAsync<ValidationException>(() => command.Execute(newUserCredentials));
        }
        #endregion
    }
}